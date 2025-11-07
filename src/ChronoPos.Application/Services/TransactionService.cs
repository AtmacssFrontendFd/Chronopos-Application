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
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IProductRepository _productRepository;
        private readonly IServiceChargeRepository _serviceChargeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IShiftRepository shiftRepository,
            IProductRepository productRepository,
            IServiceChargeRepository serviceChargeRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _shiftRepository = shiftRepository;
            _productRepository = productRepository;
            _serviceChargeRepository = serviceChargeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransactionDto>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllWithDetailsAsync();
            return transactions.Select(MapToDto);
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id);
            return transaction != null ? MapToDto(transaction) : null;
        }

        public async Task<IEnumerable<TransactionDto>> GetByShiftIdAsync(int shiftId)
        {
            var transactions = await _transactionRepository.GetByShiftIdAsync(shiftId);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetByCustomerIdAsync(int customerId)
        {
            var transactions = await _transactionRepository.GetByCustomerIdAsync(customerId);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetByStatusAsync(string status)
        {
            var transactions = await _transactionRepository.GetByStatusAsync(status);
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
            return transactions.Select(MapToDto);
        }

        public async Task<TransactionDto?> GetWithDetailsAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id);
            return transaction != null ? MapToDto(transaction) : null;
        }

        public async Task<TransactionDto?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var transactions = await _transactionRepository.GetAllWithDetailsAsync();
            var transaction = transactions.FirstOrDefault(t => t.InvoiceNumber == invoiceNumber);
            return transaction != null ? MapToDto(transaction) : null;
        }

        public async Task<IEnumerable<TransactionDto>> GetTodaysTransactionsAsync()
        {
            var today = DateTime.Today;
            var transactions = await _transactionRepository.GetByDateRangeAsync(today, today.AddDays(1));
            return transactions.Select(MapToDto);
        }

        public async Task<TransactionDto> CreateAsync(CreateTransactionDto createDto, int currentUserId)
        {
            // Validate shift exists and is open (skip validation for default shift ID 1)
            if (createDto.ShiftId != 1)
            {
                var shift = await _shiftRepository.GetByIdAsync(createDto.ShiftId);
                if (shift == null)
                {
                    throw new ArgumentException("Shift not found.");
                }
                if (shift.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot create transaction on a closed shift.");
                }
            }

            // Validate customer if provided
            if (createDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId.Value);
                if (customer == null)
                {
                    throw new ArgumentException("Customer not found.");
                }
            }

            // Create transaction entity
            var transaction = new Transaction
            {
                ShiftId = createDto.ShiftId,
                CustomerId = createDto.CustomerId,
                UserId = createDto.UserId,
                ShopLocationId = createDto.ShopLocationId,
                TableId = createDto.TableId,
                ReservationId = createDto.ReservationId,
                SellingTime = createDto.SellingTime,
                TotalAmount = createDto.TotalAmount,
                TotalVat = createDto.TotalVat,
                TotalDiscount = createDto.TotalDiscount,
                TotalAppliedVat = 0, // Will be calculated
                TotalAppliedDiscountValue = 0, // Will be calculated
                AmountPaidCash = createDto.AmountPaidCash,
                AmountCreditRemaining = createDto.AmountCreditRemaining,
                CreditDays = createDto.CreditDays,
                IsPercentageDiscount = createDto.IsPercentageDiscount,
                DiscountValue = createDto.DiscountValue,
                DiscountMaxValue = createDto.DiscountMaxValue,
                Vat = createDto.Vat,
                DiscountNote = createDto.DiscountNote,
                InvoiceNumber = GenerateInvoiceNumber(),
                Status = createDto.Status,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now
            };

            // Add transaction products
            if (createDto.Products != null && createDto.Products.Any())
            {
                foreach (var productDto in createDto.Products)
                {
                    var transactionProduct = new TransactionProduct
                    {
                        ProductId = productDto.ProductId,
                        BuyerCost = productDto.BuyerCost,
                        SellingPrice = productDto.SellingPrice,
                        IsPercentageDiscount = productDto.IsPercentageDiscount,
                        DiscountValue = productDto.DiscountValue,
                        DiscountMaxValue = productDto.DiscountMaxValue,
                        Vat = productDto.Vat,
                        Quantity = productDto.Quantity,
                        ProductUnitId = productDto.ProductUnitId,
                        Status = "active",
                        CreatedAt = DateTime.Now
                    };

                    // Add modifiers if any
                    if (productDto.ModifierIds != null && productDto.ModifierIds.Any())
                    {
                        foreach (var modifierId in productDto.ModifierIds)
                        {
                            transactionProduct.TransactionModifiers.Add(new TransactionModifier
                            {
                                ProductModifierId = modifierId,
                                ExtraPrice = 0, // Will be set from ProductModifier
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    transaction.TransactionProducts.Add(transactionProduct);
                }
            }

            // Add service charges if any
            if (createDto.ServiceChargeOptionIds != null && createDto.ServiceChargeOptionIds.Any())
            {
                foreach (var serviceChargeOptionId in createDto.ServiceChargeOptionIds)
                {
                    var serviceCharge = await _serviceChargeRepository.GetByIdAsync(serviceChargeOptionId);
                    if (serviceCharge != null && serviceCharge.IsActive)
                    {
                        var chargeAmount = CalculateServiceChargeAmount(serviceCharge, transaction.TotalAmount);
                        var chargeVat = serviceCharge.TaxTypeId.HasValue && serviceCharge.TaxType != null && serviceCharge.TaxType.IsPercentage 
                            ? chargeAmount * (serviceCharge.TaxType.Value) / 100 
                            : 0;

                        transaction.TransactionServiceCharges.Add(new TransactionServiceCharge
                        {
                            ServiceChargeOptionId = serviceChargeOptionId,
                            TotalAmount = chargeAmount,
                            TotalVat = chargeVat,
                            Status = "Active",
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            await _transactionRepository.AddAsync(transaction);
            
            // Update stock for all products in the transaction
            if (createDto.Products != null && createDto.Products.Any())
            {
                foreach (var productDto in createDto.Products)
                {
                    var product = await _productRepository.GetByIdAsync(productDto.ProductId);
                    if (product != null && product.IsStockTracked)
                    {
                        // Decrease both InitialStock and StockQuantity
                        product.InitialStock -= productDto.Quantity;
                        product.StockQuantity -= (int)productDto.Quantity;
                        product.UpdatedAt = DateTime.Now;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }
            
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(await _transactionRepository.GetByIdWithDetailsAsync(transaction.Id) ?? transaction);
        }

        public async Task<TransactionDto> UpdateAsync(int id, UpdateTransactionDto updateDto, int currentUserId)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            // Only prevent updating settled transactions (allow updating draft and billed for payment processing)
            if (transaction.Status == "settled")
            {
                throw new InvalidOperationException("Cannot update a settled transaction.");
            }

            // Update properties
            if (updateDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(updateDto.CustomerId.Value);
                if (customer == null)
                {
                    throw new ArgumentException("Customer not found.");
                }
                transaction.CustomerId = updateDto.CustomerId;
            }

            transaction.TableId = updateDto.TableId ?? transaction.TableId;
            transaction.ReservationId = updateDto.ReservationId ?? transaction.ReservationId;
            transaction.TotalAmount = updateDto.TotalAmount;
            transaction.TotalVat = updateDto.TotalVat;
            transaction.TotalDiscount = updateDto.TotalDiscount;
            transaction.AmountPaidCash = updateDto.AmountPaidCash;
            transaction.AmountCreditRemaining = updateDto.AmountCreditRemaining;
            transaction.CreditDays = updateDto.CreditDays;
            transaction.IsPercentageDiscount = updateDto.IsPercentageDiscount;
            transaction.DiscountValue = updateDto.DiscountValue;
            transaction.DiscountMaxValue = updateDto.DiscountMaxValue;
            transaction.Vat = updateDto.Vat;
            transaction.DiscountNote = updateDto.DiscountNote;
            transaction.Status = updateDto.Status;
            transaction.UpdatedAt = DateTime.Now;
            transaction.UpdatedBy = currentUserId;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(transaction);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return false;
            }

            // Only allow deletion of draft transactions
            if (transaction.Status != "draft")
            {
                throw new InvalidOperationException("Only draft transactions can be deleted.");
            }

            _transactionRepository.Delete(transaction);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<TransactionDto> ChangeStatusAsync(int id, string newStatus, int currentUserId)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            // Validate status transition
            if (!IsValidStatusTransition(transaction.Status, newStatus))
            {
                throw new InvalidOperationException($"Invalid status transition from {transaction.Status} to {newStatus}.");
            }

            transaction.Status = newStatus;
            transaction.UpdatedAt = DateTime.Now;

            // Generate invoice number when billing
            if (newStatus == "billed" && string.IsNullOrEmpty(transaction.InvoiceNumber))
            {
                transaction.InvoiceNumber = GenerateInvoiceNumber();
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(transaction);
        }

        public async Task<TransactionDto> GenerateInvoiceNumberAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(id);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            if (!string.IsNullOrEmpty(transaction.InvoiceNumber))
            {
                throw new InvalidOperationException("Transaction already has an invoice number.");
            }

            transaction.InvoiceNumber = GenerateInvoiceNumber();
            transaction.UpdatedAt = DateTime.Now;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(transaction);
        }

        public async Task<decimal> CalculateTransactionTotalAsync(int transactionId)
        {
            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(transactionId);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            decimal productTotal = transaction.TransactionProducts?.Sum(p => p.SellingPrice * p.Quantity) ?? 0m;
            decimal modifierTotal = transaction.TransactionProducts?.SelectMany(p => p.TransactionModifiers)
                                                                    .Sum(m => m.ExtraPrice) ?? 0m;
            decimal serviceChargeTotal = transaction.TransactionServiceCharges?.Sum(sc => sc.TotalAmount) ?? 0m;

            return productTotal + modifierTotal + serviceChargeTotal - transaction.TotalDiscount;
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "draft", new List<string> { "hold", "billed", "settled", "pending_payment", "partial_payment", "cancelled" } }, // Allow direct settlement/partial/pending from draft
                { "hold", new List<string> { "draft", "billed", "settled", "pending_payment", "partial_payment", "cancelled" } }, // Allow direct settlement/partial/pending from hold
                { "billed", new List<string> { "settled", "pending_payment", "partial_payment", "refunded", "cancelled" } }, // Allow payment transitions from billed
                { "pending_payment", new List<string> { "billed", "settled", "partial_payment", "cancelled" } },
                { "partial_payment", new List<string> { "settled", "pending_payment", "cancelled" } },
                { "settled", new List<string> { "refunded", "exchanged" } }, // Settled can be refunded or exchanged
                { "cancelled", new List<string>() }, // No transitions from cancelled
                { "refunded", new List<string>() }, // No transitions from refunded (final state)
                { "exchanged", new List<string>() } // No transitions from exchanged (final state)
            };

            return validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus);
        }

        private decimal CalculateServiceChargeAmount(ServiceCharge serviceCharge, decimal baseAmount)
        {
            if (serviceCharge.IsPercentage)
            {
                return baseAmount * serviceCharge.Value / 100;
            }
            return serviceCharge.Value;
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}";
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                ShiftId = transaction.ShiftId,
                CustomerId = transaction.CustomerId,
                CustomerName = transaction.Customer?.CustomerFullName,
                UserId = transaction.UserId,
                UserName = transaction.User?.FullName,
                ShopLocationId = transaction.ShopLocationId,
                TableId = transaction.TableId,
                TableNumber = transaction.Table?.TableNumber.ToString(),
                ReservationId = transaction.ReservationId,
                SellingTime = transaction.SellingTime,
                TotalAmount = transaction.TotalAmount,
                TotalVat = transaction.TotalVat,
                TotalDiscount = transaction.TotalDiscount,
                TotalAppliedVat = transaction.TotalAppliedVat,
                TotalAppliedDiscountValue = transaction.TotalAppliedDiscountValue,
                AmountPaidCash = transaction.AmountPaidCash,
                AmountCreditRemaining = transaction.AmountCreditRemaining,
                CreditDays = transaction.CreditDays,
                IsPercentageDiscount = transaction.IsPercentageDiscount,
                DiscountValue = transaction.DiscountValue,
                DiscountMaxValue = transaction.DiscountMaxValue,
                Vat = transaction.Vat,
                DiscountNote = transaction.DiscountNote,
                InvoiceNumber = transaction.InvoiceNumber,
                Status = transaction.Status,
                CreatedBy = transaction.CreatedBy,
                CreatedAt = transaction.CreatedAt,
                TransactionProducts = transaction.TransactionProducts?.Select(tp => new TransactionProductDto
                {
                    Id = tp.Id,
                    TransactionId = tp.TransactionId,
                    ProductId = tp.ProductId,
                    ProductName = tp.Product?.Name,
                    ProductCode = tp.Product?.Code,
                    BuyerCost = tp.BuyerCost,
                    SellingPrice = tp.SellingPrice,
                    IsPercentageDiscount = tp.IsPercentageDiscount,
                    DiscountValue = tp.DiscountValue,
                    DiscountMaxValue = tp.DiscountMaxValue,
                    Vat = tp.Vat,
                    Quantity = tp.Quantity,
                    ProductUnitId = tp.ProductUnitId,
                    Status = tp.Status,
                    LineSubtotal = tp.SellingPrice * tp.Quantity,
                    LineTotal = (tp.SellingPrice * tp.Quantity) - tp.DiscountValue + (tp.Vat * tp.Quantity),
                    Modifiers = tp.TransactionModifiers?.Select(tm => new TransactionModifierDto
                    {
                        Id = tm.Id,
                        TransactionProductId = tm.TransactionProductId,
                        ProductModifierId = tm.ProductModifierId,
                        ModifierName = tm.ProductModifier?.Name,
                        ExtraPrice = tm.ExtraPrice
                    }).ToList() ?? new List<TransactionModifierDto>()
                }).ToList() ?? new List<TransactionProductDto>(),
                TransactionServiceCharges = transaction.TransactionServiceCharges?.Select(sc => new TransactionServiceChargeDto
                {
                    Id = sc.Id,
                    TransactionId = sc.TransactionId,
                    ServiceChargeOptionId = sc.ServiceChargeOptionId,
                    ServiceChargeName = sc.ServiceChargeOption?.Name,
                    TotalAmount = sc.TotalAmount,
                    TotalVat = sc.TotalVat,
                    Status = sc.Status
                }).ToList() ?? new List<TransactionServiceChargeDto>()
            };
        }
    }
}
