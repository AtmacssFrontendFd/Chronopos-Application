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
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ShiftService(
            IShiftRepository shiftRepository,
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _shiftRepository = shiftRepository;
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ShiftDto>> GetAllAsync()
        {
            var shifts = await _shiftRepository.GetAllAsync();
            return shifts.Select(MapToDto);
        }

        public async Task<ShiftDto?> GetByIdAsync(int id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            return shift != null ? MapToDto(shift) : null;
        }

        public async Task<IEnumerable<ShiftDto>> GetByUserIdAsync(int userId)
        {
            var shifts = await _shiftRepository.GetByUserIdAsync(userId);
            return shifts.Select(MapToDto);
        }

        public async Task<IEnumerable<ShiftDto>> GetByStatusAsync(string status)
        {
            var shifts = await _shiftRepository.GetByStatusAsync(status);
            return shifts.Select(MapToDto);
        }

        public async Task<IEnumerable<ShiftDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var shifts = await _shiftRepository.GetByDateRangeAsync(startDate, endDate);
            return shifts.Select(MapToDto);
        }

        public async Task<ShiftDto?> GetActiveShiftForUserAsync(int userId)
        {
            var shift = await _shiftRepository.GetActiveShiftForUserAsync(userId);
            return shift != null ? MapToDto(shift) : null;
        }

        public async Task<ShiftDto> OpenShiftAsync(CreateShiftDto createDto)
        {
            // UserId is now optional - validate only if provided
            if (createDto.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(createDto.UserId.Value);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }

                // Check if user already has an open shift
                var existingShift = await _shiftRepository.GetActiveShiftForUserAsync(createDto.UserId.Value);
                if (existingShift != null)
                {
                    throw new InvalidOperationException($"User already has an open shift (Shift ID: {existingShift.ShiftId}).");
                }
            }

            // Create new shift
            var shift = new Shift
            {
                UserId = createDto.UserId,
                ShopLocationId = createDto.ShopLocationId,
                StartTime = DateTime.Now,
                OpeningCash = createDto.OpeningCash ?? 0,
                Status = "Open",
                CreatedAt = DateTime.Now
            };

            await _shiftRepository.AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(shift);
        }

        public async Task<ShiftDto> UpdateAsync(int id, UpdateShiftDto updateDto)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                throw new ArgumentException("Shift not found.");
            }

            // Prevent updating closed shifts
            if (shift.Status == "Closed")
            {
                throw new InvalidOperationException("Cannot update a closed shift.");
            }

            shift.OpeningCash = updateDto.OpeningCash ?? shift.OpeningCash;
            shift.UpdatedAt = DateTime.Now;

            _shiftRepository.Update(shift);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(shift);
        }

        public async Task<ShiftDto> CloseShiftAsync(int id, CloseShiftDto closeDto)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                throw new ArgumentException("Shift not found.");
            }

            if (shift.Status == "Closed")
            {
                throw new InvalidOperationException("Shift is already closed.");
            }

            // Calculate expected cash from transactions
            var transactions = await _transactionRepository.GetByShiftIdAsync(id);
            var cashTransactions = transactions.Where(t => t.AmountPaidCash > 0 && 
                                                          (t.Status == "billed" || t.Status == "settled"));
            var expectedCash = shift.OpeningCash + cashTransactions.Sum(t => t.AmountPaidCash);

            shift.EndTime = DateTime.Now;
            shift.ClosingCash = closeDto.ClosingCash;
            shift.ExpectedCash = expectedCash;
            shift.CashDifference = closeDto.ClosingCash - expectedCash;
            shift.Status = "Closed";
            shift.UpdatedAt = DateTime.Now;

            _shiftRepository.Update(shift);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(shift);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
            {
                return false;
            }

            // Check if shift has transactions
            var transactions = await _transactionRepository.GetByShiftIdAsync(id);
            if (transactions.Any())
            {
                throw new InvalidOperationException("Cannot delete a shift with transactions.");
            }

            _shiftRepository.Delete(shift);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ShiftSummaryDto> GetShiftSummaryAsync(int shiftId)
        {
            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new ArgumentException("Shift not found.");
            }

            var transactions = await _transactionRepository.GetByShiftIdAsync(shiftId);
            var settledTransactions = transactions.Where(t => t.Status == "settled" || t.Status == "billed");

            var summary = new ShiftSummaryDto
            {
                ShiftId = shift.ShiftId,
                UserId = shift.UserId,
                UserName = shift.User?.FullName,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Status = shift.Status,
                OpeningCash = shift.OpeningCash,
                ClosingCash = shift.ClosingCash,
                ExpectedCash = shift.ExpectedCash,
                CashDifference = shift.CashDifference,

                // Transaction statistics
                TotalTransactions = settledTransactions.Count(),
                TotalSales = settledTransactions.Sum(t => t.TotalAmount),
                TotalCashSales = settledTransactions.Where(t => t.AmountPaidCash > 0)
                                                   .Sum(t => t.AmountPaidCash),
                TotalCardSales = settledTransactions.Where(t => t.AmountPaidCash == 0 && t.AmountCreditRemaining == 0)
                                                   .Sum(t => t.TotalAmount),
                TotalCreditSales = settledTransactions.Where(t => t.AmountCreditRemaining > 0)
                                                     .Sum(t => t.AmountCreditRemaining),
                TotalVat = settledTransactions.Sum(t => t.TotalVat),
                TotalDiscount = settledTransactions.Sum(t => t.TotalDiscount),

                // Transaction status breakdown
                DraftTransactions = transactions.Count(t => t.Status == "draft"),
                HoldTransactions = transactions.Count(t => t.Status == "hold"),
                BilledTransactions = transactions.Count(t => t.Status == "billed"),
                SettledTransactions = transactions.Count(t => t.Status == "settled"),
                CancelledTransactions = transactions.Count(t => t.Status == "cancelled"),
                PendingPaymentTransactions = transactions.Count(t => t.Status == "pending_payment"),
                PartialPaymentTransactions = transactions.Count(t => t.Status == "partial_payment")
            };

            return summary;
        }

        private ShiftDto MapToDto(Shift shift)
        {
            return new ShiftDto
            {
                ShiftId = shift.ShiftId,
                UserId = shift.UserId,
                UserName = shift.User?.FullName,
                ShopLocationId = shift.ShopLocationId,
                ShopLocationName = shift.ShopLocation?.LocationName,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                OpeningCash = shift.OpeningCash,
                ClosingCash = shift.ClosingCash,
                ExpectedCash = shift.ExpectedCash,
                CashDifference = shift.CashDifference,
                Status = shift.Status,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt
            };
        }
    }
}
