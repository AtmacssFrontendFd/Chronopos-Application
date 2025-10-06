using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

public class ProductGroupRepository : Repository<ProductGroup>, IProductGroupRepository
{
    public ProductGroupRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<ProductGroup?> GetByIdAsync(int id)
    {
        return await _context.ProductGroups
            .Include(pg => pg.Discount)
            .Include(pg => pg.TaxType)
            .Include(pg => pg.PriceType)
            .Include(pg => pg.ProductGroupItems)
                .ThenInclude(pgi => pgi.Product)
            .Include(pg => pg.ProductGroupItems)
                .ThenInclude(pgi => pgi.Discount)
            .Include(pg => pg.CreatedByUser)
            .Include(pg => pg.ModifiedByUser)
            .FirstOrDefaultAsync(pg => pg.Id == id);
    }

    public override async Task<IEnumerable<ProductGroup>> GetAllAsync()
    {
        return await _context.ProductGroups
            .Include(pg => pg.Discount)
            .Include(pg => pg.TaxType)
            .Include(pg => pg.PriceType)
            .Include(pg => pg.ProductGroupItems)
            .Include(pg => pg.CreatedByUser)
            .Include(pg => pg.ModifiedByUser)
            .ToListAsync();
    }
}
