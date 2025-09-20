using Microsoft.EntityFrameworkCore.Storage;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ChronoPosDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IProductRepository? _productRepository;
    private IRepository<Category>? _categoryRepository;
    private IRepository<Customer>? _customerRepository;
    private ISaleRepository? _saleRepository;
    private IRepository<SaleItem>? _saleItemRepository;
    private IRepository<UnitOfMeasurement>? _unitOfMeasurementRepository;
    private IProductImageRepository? _productImageRepository;
    private IRepository<TaxType>? _taxTypeRepository;
    private IProductDiscountRepository? _productDiscountRepository;
    private ICategoryDiscountRepository? _categoryDiscountRepository;
    
    public UnitOfWork(ChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    

    public IProductRepository Products => 
        _productRepository ??= new ProductRepository(_context);

    public IProductImageRepository ProductImages =>
        _productImageRepository ??= new ProductImageRepository(_context);
    
    public IRepository<Category> Categories => 
        _categoryRepository ??= new Repository<Category>(_context);
    
    public IRepository<Customer> Customers => 
        _customerRepository ??= new Repository<Customer>(_context);
    
    public ISaleRepository Sales => 
        _saleRepository ??= new SaleRepository(_context);
    
    public IRepository<SaleItem> SaleItems => 
        _saleItemRepository ??= new Repository<SaleItem>(_context);
    
    public IRepository<UnitOfMeasurement> UnitsOfMeasurement => 
        _unitOfMeasurementRepository ??= new Repository<UnitOfMeasurement>(_context);

    public IRepository<TaxType> TaxTypes =>
        _taxTypeRepository ??= new Repository<TaxType>(_context);
    
    public IProductDiscountRepository ProductDiscounts =>
        _productDiscountRepository ??= new ProductDiscountRepository(_context);
    
    public ICategoryDiscountRepository CategoryDiscounts =>
        _categoryDiscountRepository ??= new CategoryDiscountRepository(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
