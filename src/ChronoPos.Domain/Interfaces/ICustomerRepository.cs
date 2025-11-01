using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Customer entity operations
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    // Inherits all methods from IRepository<Customer>
    // The implementation will override GetAllAsync and GetByIdAsync to include CustomerDiscounts
}
