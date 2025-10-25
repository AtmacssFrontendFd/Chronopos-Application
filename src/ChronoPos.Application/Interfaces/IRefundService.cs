using ChronoPos.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Application.Interfaces
{
    public interface IRefundService
    {
        Task<IEnumerable<RefundTransactionDto>> GetAllAsync();
        Task<RefundTransactionDto?> GetByIdAsync(int id);
        Task<IEnumerable<RefundTransactionDto>> GetByTransactionIdAsync(int transactionId);
        Task<IEnumerable<RefundTransactionDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<RefundTransactionDto>> GetByShiftIdAsync(int shiftId);
        Task<RefundTransactionDto> CreateAsync(CreateRefundTransactionDto createDto);
        Task<bool> DeleteAsync(int id);
    }
}
