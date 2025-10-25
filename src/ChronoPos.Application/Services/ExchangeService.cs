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
    public class ExchangeService : IExchangeService
    {
        private readonly IExchangeTransactionRepository _exchangeRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionProductRepository _transactionProductRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExchangeService(
            IExchangeTransactionRepository exchangeRepository,
            ITransactionRepository transactionRepository,
            ITransactionProductRepository transactionProductRepository,
            IProductRepository productRepository,
            IShiftRepository shiftRepository,
            IUnitOfWork unitOfWork)
        {
            _exchangeRepository = exchangeRepository;
            _transactionRepository = transactionRepository;
            _transactionProductRepository = transactionProductRepository;
            _productRepository = productRepository;
            _shiftRepository = shiftRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ExchangeTransactionDto>> GetAllAsync()
        {
            var exchanges = await _exchangeRepository.GetAllWithDetailsAsync();
            return exchanges.Select(MapToDto);
        }

        public async Task<ExchangeTransactionDto?> GetByIdAsync(int id)
        {
            var exchange = await _exchangeRepository.GetByIdWithDetailsAsync(id);
            return exchange != null ? MapToDto(exchange) : null;
        }

        public async Task<IEnumerable<ExchangeTransactionDto>> GetByTransactionIdAsync(int transactionId)
        {
            var exchanges = await _exchangeRepository.GetByTransactionIdAsync(transactionId);
            return exchanges.Select(MapToDto);
        }

        public async Task<IEnumerable<ExchangeTransactionDto>> GetByCustomerIdAsync(int customerId)
        {
            var exchanges = await _exchangeRepository.GetByCustomerIdAsync(customerId);
            return exchanges.Select(MapToDto);
        }

        public async Task<IEnumerable<ExchangeTransactionDto>> GetByShiftIdAsync(int shiftId)
        {
            var exchanges = await _exchangeRepository.GetByShiftIdAsync(shiftId);
            return exchanges.Select(MapToDto);
        }

        public async Task<ExchangeTransactionDto> CreateAsync(CreateExchangeTransactionDto createDto)
        {
            // Validate original transaction exists
            var originalTransaction = await _transactionRepository.GetByIdWithDetailsAsync(createDto.SellingTransactionId);
            if (originalTransaction == null)
            {
                throw new ArgumentException("Original transaction not found.");
            }

            // Validate transaction is eligible for exchange
            if (originalTransaction.Status != "settled" && originalTransaction.Status != "billed")
            {
                throw new InvalidOperationException("Only settled or billed transactions can have exchanges.");
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
                    throw new InvalidOperationException("Cannot create exchange on a closed shift.");
                }
            }

            // Calculate exchange totals
            decimal totalExchangedAmount = 0;
            decimal totalExchangedVat = 0;
            decimal productExchangedQuantity = 0;

            // Validate exchange products
            if (createDto.Products == null || !createDto.Products.Any())
            {
                throw new ArgumentException("At least one product must be selected for exchange.");
            }

            foreach (var exchangeProduct in createDto.Products)
            {
                if (!exchangeProduct.OriginalTransactionProductId.HasValue)
                {
                    throw new ArgumentException("Original transaction product ID is required.");
                }
                
                var originalProduct = await _transactionProductRepository.GetByIdAsync(exchangeProduct.OriginalTransactionProductId.Value);
                if (originalProduct == null)
                {
                    throw new ArgumentException($"Original product {exchangeProduct.OriginalTransactionProductId} not found.");
                }

                if (originalProduct.TransactionId != createDto.SellingTransactionId)
                {
                    throw new ArgumentException($"Product {exchangeProduct.OriginalTransactionProductId} does not belong to transaction {createDto.SellingTransactionId}.");
                }

                var newProduct = await _productRepository.GetByIdAsync(exchangeProduct.NewProductId);
                if (newProduct == null)
                {
                    throw new ArgumentException($"New product {exchangeProduct.NewProductId} not found.");
                }

                // Calculate amounts
                var oldProductAmount = originalProduct.SellingPrice * exchangeProduct.ReturnedQuantity;
                var newProductAmount = newProduct.Price * exchangeProduct.NewQuantity;
                var priceDifference = newProductAmount - oldProductAmount;

                var oldVat = originalProduct.Vat * exchangeProduct.ReturnedQuantity / originalProduct.Quantity;
                var newVat = 0m; // VAT should be calculated based on newProduct's tax type if needed
                var vatDifference = newVat - oldVat;

                totalExchangedAmount += Math.Abs(priceDifference);
                totalExchangedVat += Math.Abs(vatDifference);
                productExchangedQuantity += exchangeProduct.ReturnedQuantity;
            }

            // Create exchange transaction
            var exchange = new ExchangeTransaction
            {
                SellingTransactionId = createDto.SellingTransactionId,
                CustomerId = createDto.CustomerId ?? originalTransaction.CustomerId,
                ShiftId = createDto.ShiftId,
                ExchangeTime = DateTime.Now,
                TotalExchangedAmount = totalExchangedAmount,
                TotalExchangedVat = totalExchangedVat,
                ProductExchangedQuantity = productExchangedQuantity,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            // Add exchange products
            foreach (var exchangeProductDto in createDto.Products)
            {
                var originalProduct = await _transactionProductRepository.GetByIdAsync(exchangeProductDto.OriginalTransactionProductId.Value);
                var newProduct = await _productRepository.GetByIdAsync(exchangeProductDto.NewProductId);

                var oldProductAmount = originalProduct.SellingPrice * exchangeProductDto.ReturnedQuantity;
                var newProductAmount = newProduct.Price * exchangeProductDto.NewQuantity;
                var priceDifference = newProductAmount - oldProductAmount;

                var oldVat = originalProduct.Vat * exchangeProductDto.ReturnedQuantity / originalProduct.Quantity;
                var newVat = 0m; // VAT calculation for new product
                var vatDifference = newVat - oldVat;

                exchange.ExchangeTransactionProducts.Add(new ExchangeTransactionProduct
                {
                    OriginalTransactionProductId = exchangeProductDto.OriginalTransactionProductId,
                    NewProductId = exchangeProductDto.NewProductId,
                    ReturnedQuantity = exchangeProductDto.ReturnedQuantity,
                    NewQuantity = exchangeProductDto.NewQuantity,
                    OldProductAmount = oldProductAmount,
                    NewProductAmount = newProductAmount,
                    PriceDifference = priceDifference,
                    VatDifference = vatDifference,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                });
            }

            await _exchangeRepository.AddAsync(exchange);
            
            // Update original transaction status to 'exchanged'
            originalTransaction.Status = "exchanged";
            originalTransaction.UpdatedAt = DateTime.Now;
            _transactionRepository.Update(originalTransaction);
            
            // Update stock for exchange products
            foreach (var exchangeProductDto in createDto.Products)
            {
                // Get the original product (being returned) from the transaction
                var originalTransactionProduct = await _transactionProductRepository.GetByIdAsync(exchangeProductDto.OriginalTransactionProductId!.Value);
                if (originalTransactionProduct != null)
                {
                    var returnedProduct = await _productRepository.GetByIdAsync(originalTransactionProduct.ProductId);
                    if (returnedProduct != null && returnedProduct.IsStockTracked)
                    {
                        // Increase stock for returned items
                        returnedProduct.InitialStock += exchangeProductDto.ReturnedQuantity;
                        returnedProduct.StockQuantity += (int)exchangeProductDto.ReturnedQuantity;
                        returnedProduct.UpdatedAt = DateTime.Now;
                        await _productRepository.UpdateAsync(returnedProduct);
                    }
                }

                // Get the new product (being given)
                var newProduct = await _productRepository.GetByIdAsync(exchangeProductDto.NewProductId);
                if (newProduct != null && newProduct.IsStockTracked)
                {
                    // Decrease stock for given items
                    newProduct.InitialStock -= exchangeProductDto.NewQuantity;
                    newProduct.StockQuantity -= (int)exchangeProductDto.NewQuantity;
                    newProduct.UpdatedAt = DateTime.Now;
                    await _productRepository.UpdateAsync(newProduct);
                }
            }
            
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(await _exchangeRepository.GetByIdWithDetailsAsync(exchange.Id) ?? exchange);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exchange = await _exchangeRepository.GetByIdAsync(id);
            if (exchange == null)
            {
                return false;
            }

            _exchangeRepository.Delete(exchange);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private ExchangeTransactionDto MapToDto(ExchangeTransaction exchange)
        {
            return new ExchangeTransactionDto
            {
                Id = exchange.Id,
                SellingTransactionId = exchange.SellingTransactionId,
                CustomerId = exchange.CustomerId,
                CustomerName = exchange.Customer?.CustomerFullName,
                InvoiceNumber = exchange.SellingTransaction?.InvoiceNumber,
                ShiftId = exchange.ShiftId,
                ExchangeTime = exchange.ExchangeTime,
                TotalExchangedAmount = exchange.TotalExchangedAmount,
                TotalExchangedVat = exchange.TotalExchangedVat,
                ProductExchangedQuantity = exchange.ProductExchangedQuantity,
                Status = exchange.Status,
                CreatedAt = exchange.CreatedAt,
                ExchangeProducts = exchange.ExchangeTransactionProducts?.Select(ep => new ExchangeTransactionProductDto
                {
                    Id = ep.Id,
                    ExchangeTransactionId = ep.ExchangeTransactionId,
                    OriginalTransactionProductId = ep.OriginalTransactionProductId,
                    OldProductName = ep.OriginalTransactionProduct?.Product?.Name,
                    NewProductId = ep.NewProductId,
                    NewProductName = ep.NewProduct?.Name,
                    ReturnedQuantity = ep.ReturnedQuantity,
                    NewQuantity = ep.NewQuantity,
                    OldProductAmount = ep.OldProductAmount,
                    NewProductAmount = ep.NewProductAmount,
                    PriceDifference = ep.PriceDifference,
                    VatDifference = ep.VatDifference,
                    Status = ep.Status
                }).ToList() ?? new List<ExchangeTransactionProductDto>()
            };
        }
    }
}
