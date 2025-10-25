using ChronoPos.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Application.Interfaces
{
    public interface IExchangeService
    {
        Task<IEnumerable<ExchangeTransactionDto>> GetAllAsync();
        Task<ExchangeTransactionDto?> GetByIdAsync(int id);
        Task<IEnumerable<ExchangeTransactionDto>> GetByTransactionIdAsync(int transactionId);
        Task<IEnumerable<ExchangeTransactionDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ExchangeTransactionDto>> GetByShiftIdAsync(int shiftId);
        Task<ExchangeTransactionDto> CreateAsync(CreateExchangeTransactionDto createDto);
        Task<bool> DeleteAsync(int id);
    }
}
