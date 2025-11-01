using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Logging;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository for Customer entity with CustomerDiscounts navigation property loaded
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all customers with their discount relationships loaded
    /// </summary>
    public override async Task<IEnumerable<Customer>> GetAllAsync()
    {
        AppLogger.Log("CustomerRepository: Loading all customers with CustomerDiscounts", filename: "customer_discounts");
        
        var customers = await _dbSet
            .Include(c => c.CustomerDiscounts)
            .ToListAsync();
        
        AppLogger.Log($"CustomerRepository: Loaded {customers.Count} customers", filename: "customer_discounts");
        
        foreach (var customer in customers)
        {
            AppLogger.Log($"  - Customer ID {customer.Id} ({customer.CustomerFullName}) has {customer.CustomerDiscounts?.Count ?? 0} CustomerDiscount entries", filename: "customer_discounts");
            
            if (customer.CustomerDiscounts != null && customer.CustomerDiscounts.Any())
            {
                foreach (var cd in customer.CustomerDiscounts)
                {
                    AppLogger.Log($"    - CustomerDiscount: DiscountId={cd.DiscountsId}, DeletedAt={cd.DeletedAt}", filename: "customer_discounts");
                }
            }
        }
        
        return customers;
    }

    /// <summary>
    /// Gets a customer by ID with discount relationships loaded
    /// </summary>
    public override async Task<Customer?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.CustomerDiscounts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
