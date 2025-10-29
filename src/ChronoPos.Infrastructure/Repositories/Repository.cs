using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Interfaces;
using System.IO;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ChronoPosDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    // Prefer per-user LocalApplicationData for logs so installed apps (Program Files) don't fail
    private static readonly string DefaultLogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChronoPos", "Logs");
    private static readonly string LogFilePath = Path.Combine(DefaultLogDirectory, $"repository_{DateTime.Now:yyyyMMdd}.log");
    private static readonly object LockObject = new object();
    
    private static void Log(string message)
    {
        try
        {
            lock (LockObject)
            {
                try
                {
                    // Ensure directory exists; if it fails (e.g., permissions), fall back to Temp folder
                    var dir = Path.GetDirectoryName(LogFilePath) ?? DefaultLogDirectory;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                catch
                {
                    // fallback to temp
                    var tmp = Path.GetTempPath();
                    var fallback = Path.Combine(tmp, "ChronoPosLogs");
                    try { Directory.CreateDirectory(fallback); } catch { /* noop */ }
                    var fallbackPath = Path.Combine(fallback, Path.GetFileName(LogFilePath));
                    File.AppendAllText(fallbackPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
                    return;
                }

                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Silent fail to prevent logging from breaking the application
        }
    }
    
    public Repository(ChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var typeName = typeof(T).Name;
        Log($"Repository<{typeName}>.GetAllAsync: Starting...");
        
        try
        {
            Log($"Repository<{typeName}>.GetAllAsync: DbSet null check: {_dbSet == null}");
            Log($"Repository<{typeName}>.GetAllAsync: Context null check: {_context == null}");
            
            if (_dbSet == null)
            {
                Log($"Repository<{typeName}>.GetAllAsync: ERROR - _dbSet is null");
                return new List<T>();
            }
            
            // Count total entities
            var totalCount = await _dbSet.CountAsync();
            Log($"Repository<{typeName}>.GetAllAsync: Total {typeName} entities in database: {totalCount}");
            
            // Execute the query
            Log($"Repository<{typeName}>.GetAllAsync: Executing ToListAsync()...");
            var result = await _dbSet.ToListAsync();
            
            Log($"Repository<{typeName}>.GetAllAsync: Query returned {result.Count} {typeName} entities");
            
            return result;
        }
        catch (Exception ex)
        {
            Log($"Repository<{typeName}>.GetAllAsync: ERROR - {ex.Message}");
            Log($"Repository<{typeName}>.GetAllAsync: Inner Exception - {ex.InnerException?.Message}");
            Log($"Repository<{typeName}>.GetAllAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
    }
    
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }
    
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
    
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
    
    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
