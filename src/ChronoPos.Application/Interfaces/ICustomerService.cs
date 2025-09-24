using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto);
    Task<CustomerDto> UpdateCustomerAsync(CustomerDto customerDto);
    Task DeleteCustomerAsync(int id);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
}