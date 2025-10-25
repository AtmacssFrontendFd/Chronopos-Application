using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronoPos.Application.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRefundTransactionRepository _refundRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionProductRepository _transactionProductRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RefundService(
            IRefundTransactionRepository refundRepository,
            ITransactionRepository transactionRepository,
            ITransactionProductRepository transactionProductRepository,
            IShiftRepository shiftRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _refundRepository = refundRepository;
            _transactionRepository = transactionRepository;
            _transactionProductRepository = transactionProductRepository;
            _shiftRepository = shiftRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RefundTransactionDto>> GetAllAsync()
        {
            var refunds = await _refundRepository.GetAllWithDetailsAsync();
            return refunds.Select(MapToDto);
        }

        public async Task<RefundTransactionDto?> GetByIdAsync(int id)
        {
            var refund = await _refundRepository.GetByIdWithDetailsAsync(id);
            return refund != null ? MapToDto(refund) : null;
        }

        public async Task<IEnumerable<RefundTransactionDto>> GetByTransactionIdAsync(int transactionId)
        {
            var refunds = await _refundRepository.GetByTransactionIdAsync(transactionId);
            return refunds.Select(MapToDto);
        }

        public async Task<IEnumerable<RefundTransactionDto>> GetByCustomerIdAsync(int customerId)
        {
            var refunds = await _refundRepository.GetByCustomerIdAsync(customerId);
            return refunds.Select(MapToDto);
        }

        public async Task<IEnumerable<RefundTransactionDto>> GetByShiftIdAsync(int shiftId)
        {
            var refunds = await _refundRepository.GetByShiftIdAsync(shiftId);
            return refunds.Select(MapToDto);
        }

        public async Task<RefundTransactionDto> CreateAsync(CreateRefundTransactionDto createDto)
        {
            // Validate original transaction exists
            var originalTransaction = await _transactionRepository.GetByIdWithDetailsAsync(createDto.SellingTransactionId);
            if (originalTransaction == null)
            {
                throw new ArgumentException("Original transaction not found.");
            }

            // Validate transaction is eligible for refund
            if (originalTransaction.Status != "settled" && originalTransaction.Status != "billed")
            {
                throw new InvalidOperationException("Only settled or billed transactions can be refunded.");
            }

            // Validate shift if provided
            if (createDto.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(createDto.ShiftId.Value);
                if (shift == null)
                {
                    throw new ArgumentException("Shift not found.");
                }
                if (shift.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot create refund on a closed shift.");
                }
            }

            // Calculate refund totals
            decimal totalAmount = 0;
            decimal totalVat = 0;

            // Validate refund products
            if (createDto.Products == null || !createDto.Products.Any())
            {
                throw new ArgumentException("At least one product must be selected for refund.");
            }

            foreach (var refundProduct in createDto.Products)
            {
                var transactionProduct = await _transactionProductRepository.GetByIdAsync(refundProduct.TransactionProductId);
                if (transactionProduct == null)
                {
                    throw new ArgumentException($"Transaction product {refundProduct.TransactionProductId} not found.");
                }

                if (transactionProduct.TransactionId != createDto.SellingTransactionId)
                {
                    throw new ArgumentException($"Product {refundProduct.TransactionProductId} does not belong to transaction {createDto.SellingTransactionId}.");
                }

                // Calculate refund amount for this product
                var productAmount = transactionProduct.SellingPrice * refundProduct.TotalQuantityReturned;
                var productVat = transactionProduct.Vat * refundProduct.TotalQuantityReturned / transactionProduct.Quantity;

                totalAmount += productAmount;
                totalVat += productVat;
            }

            // Create refund transaction
            var refund = new RefundTransaction
            {
                SellingTransactionId = createDto.SellingTransactionId,
                CustomerId = createDto.CustomerId ?? originalTransaction.CustomerId,
                ShiftId = createDto.ShiftId,
                UserId = createDto.UserId,
                RefundTime = DateTime.Now,
                TotalAmount = totalAmount,
                TotalVat = totalVat,
                IsCash = createDto.IsCash,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            // Add refund products
            foreach (var refundProductDto in createDto.Products)
            {
                var transactionProduct = await _transactionProductRepository.GetByIdAsync(refundProductDto.TransactionProductId);
                var productAmount = transactionProduct.SellingPrice * refundProductDto.TotalQuantityReturned;
                var productVat = transactionProduct.Vat * refundProductDto.TotalQuantityReturned / transactionProduct.Quantity;

                refund.RefundTransactionProducts.Add(new RefundTransactionProduct
                {
                    TransactionProductId = refundProductDto.TransactionProductId,
                    TotalQuantityReturned = refundProductDto.TotalQuantityReturned,
                    TotalAmount = productAmount,
                    TotalVat = productVat,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                });
            }

            // Update original transaction status to 'refunded'
            originalTransaction.Status = "refunded";
            originalTransaction.UpdatedAt = DateTime.Now;
            _transactionRepository.Update(originalTransaction);

            await _refundRepository.AddAsync(refund);
            
            // Update stock for refunded products - return them to inventory
            foreach (var refundProductDto in createDto.Products)
            {
                var transactionProduct = await _transactionProductRepository.GetByIdAsync(refundProductDto.TransactionProductId);
                if (transactionProduct != null)
                {
                    var product = await _productRepository.GetByIdAsync(transactionProduct.ProductId);
                    if (product != null && product.IsStockTracked)
                    {
                        // Increase stock for refunded items (return to inventory)
                        product.InitialStock += refundProductDto.TotalQuantityReturned;
                        product.StockQuantity += (int)refundProductDto.TotalQuantityReturned;
                        product.UpdatedAt = DateTime.Now;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }
            
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(await _refundRepository.GetByIdWithDetailsAsync(refund.Id) ?? refund);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var refund = await _refundRepository.GetByIdAsync(id);
            if (refund == null)
            {
                return false;
            }

            // Restore original transaction status
            var originalTransaction = await _transactionRepository.GetByIdAsync(refund.SellingTransactionId);
            if (originalTransaction != null)
            {
                originalTransaction.Status = "settled";
                originalTransaction.UpdatedAt = DateTime.Now;
                _transactionRepository.Update(originalTransaction);
            }

            _refundRepository.Delete(refund);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private RefundTransactionDto MapToDto(RefundTransaction refund)
        {
            return new RefundTransactionDto
            {
                Id = refund.Id,
                SellingTransactionId = refund.SellingTransactionId,
                CustomerId = refund.CustomerId,
                CustomerName = refund.Customer?.CustomerFullName,
                InvoiceNumber = refund.SellingTransaction?.InvoiceNumber,
                ShiftId = refund.ShiftId,
                UserId = refund.UserId,
                UserName = refund.User?.FullName,
                RefundTime = refund.RefundTime,
                TotalAmount = refund.TotalAmount,
                TotalVat = refund.TotalVat,
                IsCash = refund.IsCash,
                Status = refund.Status,
                CreatedAt = refund.CreatedAt,
                RefundProducts = refund.RefundTransactionProducts?.Select(rp => new RefundTransactionProductDto
                {
                    Id = rp.Id,
                    RefundTransactionId = rp.RefundTransactionId,
                    TransactionProductId = rp.TransactionProductId,
                    ProductName = rp.TransactionProduct?.Product?.Name,
                    TotalQuantityReturned = rp.TotalQuantityReturned,
                    TotalAmount = rp.TotalAmount,
                    TotalVat = rp.TotalVat,
                    Status = rp.Status
                }).ToList() ?? new List<RefundTransactionProductDto>()
            };
        }
    }
}
