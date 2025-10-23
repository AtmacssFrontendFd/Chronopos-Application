using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

public class ProductGroupItemRepository : Repository<ProductGroupItem>, IProductGroupItemRepository
{
    public ProductGroupItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<ProductGroupItem?> GetByIdAsync(int id)
    {
        return await _context.ProductGroupItems
            .Include(pgi => pgi.Product)
            .Include(pgi => pgi.Discount)
            .Include(pgi => pgi.TaxType)
            .Include(pgi => pgi.SellingPriceType)
            .Include(pgi => pgi.ProductGroup)
            .FirstOrDefaultAsync(pgi => pgi.Id == id);
    }

    public override async Task<IEnumerable<ProductGroupItem>> GetAllAsync()
    {
        return await _context.ProductGroupItems
            .Include(pgi => pgi.Product)
            .Include(pgi => pgi.Discount)
            .Include(pgi => pgi.TaxType)
            .Include(pgi => pgi.SellingPriceType)
            .Include(pgi => pgi.ProductGroup)
            .ToListAsync();
    }
}
