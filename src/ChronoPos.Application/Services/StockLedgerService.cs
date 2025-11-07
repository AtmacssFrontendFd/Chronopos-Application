using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for StockLedger operations
/// </summary>
public class StockLedgerService : IStockLedgerService
{
    private readonly IStockLedgerRepository _stockLedgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockLedgerService(IStockLedgerRepository stockLedgerRepository, IUnitOfWork unitOfWork)
    {
        _stockLedgerRepository = stockLedgerRepository ?? throw new ArgumentNullException(nameof(stockLedgerRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all stock ledger entries
    /// </summary>
    /// <returns>Collection of stock ledger DTOs</returns>
    public async Task<IEnumerable<StockLedgerDto>> GetAllAsync()
    {
        var stockLedgers = await _stockLedgerRepository.GetAllAsync();
        return stockLedgers.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock ledger entry by ID
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <returns>Stock ledger DTO if found</returns>
    public async Task<StockLedgerDto?> GetByIdAsync(int id)
    {
        var stockLedger = await _stockLedgerRepository.GetByIdAsync(id);
        return stockLedger != null ? MapToDto(stockLedger) : null;
    }

    /// <summary>
    /// Gets all stock ledger entries for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    public async Task<IEnumerable<StockLedgerDto>> GetByProductIdAsync(int productId)
    {
        var stockLedgers = await _stockLedgerRepository.GetByProductIdAsync(productId);
        return stockLedgers.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock ledger entries for a product within a date range
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    public async Task<IEnumerable<StockLedgerDto>> GetByProductIdAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var stockLedgers = await _stockLedgerRepository.GetByProductIdAndDateRangeAsync(productId, startDate, endDate);
        return stockLedgers.Select(MapToDto);
    }

    /// <summary>
    /// Gets the current balance for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Current balance</returns>
    public async Task<decimal> GetCurrentBalanceAsync(int productId)
    {
        return await _stockLedgerRepository.GetCurrentBalanceAsync(productId);
    }

    /// <summary>
    /// Gets stock ledger entries by movement type
    /// </summary>
    /// <param name="movementType">Movement type</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    public async Task<IEnumerable<StockLedgerDto>> GetByMovementTypeAsync(StockMovementType movementType)
    {
        var stockLedgers = await _stockLedgerRepository.GetByMovementTypeAsync(movementType);
        return stockLedgers.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock ledger entries by reference
    /// </summary>
    /// <param name="referenceType">Reference type</param>
    /// <param name="referenceId">Reference ID</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    public async Task<IEnumerable<StockLedgerDto>> GetByReferenceAsync(StockReferenceType? referenceType, int? referenceId)
    {
        var stockLedgers = await _stockLedgerRepository.GetByReferenceAsync(referenceType, referenceId);
        return stockLedgers.Select(MapToDto);
    }

    /// <summary>
    /// Gets the latest stock entry for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Latest stock ledger DTO</returns>
    public async Task<StockLedgerDto?> GetLatestByProductIdAsync(int productId)
    {
        var stockLedger = await _stockLedgerRepository.GetLatestByProductIdAsync(productId);
        return stockLedger != null ? MapToDto(stockLedger) : null;
    }

    /// <summary>
    /// Creates a new stock ledger entry
    /// </summary>
    /// <param name="createStockLedgerDto">Stock ledger data</param>
    /// <returns>Created stock ledger DTO</returns>
    public async Task<StockLedgerDto> CreateAsync(CreateStockLedgerDto createStockLedgerDto)
    {
        // Get current balance for the product
        var currentBalance = await _stockLedgerRepository.GetCurrentBalanceAsync(createStockLedgerDto.ProductId);

        // Calculate new balance based on movement type
        var newBalance = CalculateNewBalance(currentBalance, createStockLedgerDto.Qty, createStockLedgerDto.MovementType);

        var stockLedger = new StockLedger
        {
            ProductId = createStockLedgerDto.ProductId,
            UnitId = createStockLedgerDto.UnitId,
            MovementType = createStockLedgerDto.MovementType,
            Qty = createStockLedgerDto.Qty,
            Balance = newBalance,
            Location = createStockLedgerDto.Location?.Trim(),
            ReferenceType = createStockLedgerDto.ReferenceType,
            ReferenceId = createStockLedgerDto.ReferenceId,
            Note = createStockLedgerDto.Note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _stockLedgerRepository.AddAsync(stockLedger);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(stockLedger);
    }

    /// <summary>
    /// Updates an existing stock ledger entry
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <param name="updateStockLedgerDto">Updated stock ledger data</param>
    /// <returns>Updated stock ledger DTO</returns>
    public async Task<StockLedgerDto> UpdateAsync(int id, UpdateStockLedgerDto updateStockLedgerDto)
    {
        var stockLedger = await _stockLedgerRepository.GetByIdAsync(id);
        if (stockLedger == null)
        {
            throw new ArgumentException($"Stock ledger entry with ID {id} not found");
        }

        // Get the balance before this entry
        var entriesBeforeThis = await _stockLedgerRepository.GetByProductIdAsync(stockLedger.ProductId);
        var balanceBeforeThis = entriesBeforeThis
            .Where(e => e.CreatedAt < stockLedger.CreatedAt)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault()?.Balance ?? 0;

        // Calculate new balance based on updated quantity and movement type
        var newBalance = CalculateNewBalance(balanceBeforeThis, updateStockLedgerDto.Qty, updateStockLedgerDto.MovementType);

        stockLedger.ProductId = updateStockLedgerDto.ProductId;
        stockLedger.UnitId = updateStockLedgerDto.UnitId;
        stockLedger.MovementType = updateStockLedgerDto.MovementType;
        stockLedger.Qty = updateStockLedgerDto.Qty;
        stockLedger.Balance = newBalance;
        stockLedger.Location = updateStockLedgerDto.Location?.Trim();
        stockLedger.ReferenceType = updateStockLedgerDto.ReferenceType;
        stockLedger.ReferenceId = updateStockLedgerDto.ReferenceId;
        stockLedger.Note = updateStockLedgerDto.Note?.Trim();

        await _stockLedgerRepository.UpdateAsync(stockLedger);
        
        // Recalculate balances for all subsequent entries
        await RecalculateSubsequentBalances(stockLedger.ProductId, stockLedger.CreatedAt);
        
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(stockLedger);
    }

    /// <summary>
    /// Deletes a stock ledger entry
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var stockLedger = await _stockLedgerRepository.GetByIdAsync(id);
        if (stockLedger == null)
        {
            return false;
        }

        var productId = stockLedger.ProductId;
        var createdAt = stockLedger.CreatedAt;

        await _stockLedgerRepository.DeleteAsync(stockLedger.Id);
        
        // Recalculate balances for all subsequent entries
        await RecalculateSubsequentBalances(productId, createdAt);
        
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Calculates new balance based on movement type
    /// </summary>
    /// <param name="currentBalance">Current balance</param>
    /// <param name="qty">Quantity</param>
    /// <param name="movementType">Movement type</param>
    /// <returns>New balance</returns>
    private static decimal CalculateNewBalance(decimal currentBalance, decimal qty, StockMovementType movementType)
    {
        return movementType switch
        {
            StockMovementType.Purchase => currentBalance + qty,
            StockMovementType.Sale => currentBalance - qty,
            StockMovementType.Adjustment => currentBalance + qty, // Can be positive or negative
            StockMovementType.TransferIn => currentBalance + qty,
            StockMovementType.TransferOut => currentBalance - qty,
            StockMovementType.Return => currentBalance + qty,
            StockMovementType.Replace => currentBalance, // No change in total stock
            StockMovementType.Waste => currentBalance - qty,
            StockMovementType.Opening => qty, // Set to opening balance
            StockMovementType.Closing => qty, // Set to closing balance
            _ => currentBalance
        };
    }

    /// <summary>
    /// Recalculates balances for all entries after a certain date
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="afterDate">Date after which to recalculate</param>
    private async Task RecalculateSubsequentBalances(int productId, DateTime afterDate)
    {
        var allEntries = await _stockLedgerRepository.GetByProductIdAsync(productId);
        var subsequentEntries = allEntries
            .Where(e => e.CreatedAt > afterDate)
            .OrderBy(e => e.CreatedAt)
            .ToList();

        if (!subsequentEntries.Any())
            return;

        // Get the balance at the point of change
        var balanceAtChange = allEntries
            .Where(e => e.CreatedAt <= afterDate)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault()?.Balance ?? 0;

        var runningBalance = balanceAtChange;

        foreach (var entry in subsequentEntries)
        {
            runningBalance = CalculateNewBalance(runningBalance, entry.Qty, entry.MovementType);
            entry.Balance = runningBalance;
            await _stockLedgerRepository.UpdateAsync(entry);
        }
    }

    /// <summary>
    /// Maps StockLedger entity to StockLedgerDto
    /// </summary>
    /// <param name="stockLedger">StockLedger entity</param>
    /// <returns>StockLedger DTO</returns>
    private static StockLedgerDto MapToDto(StockLedger stockLedger)
    {
        return new StockLedgerDto
        {
            Id = stockLedger.Id,
            ProductId = stockLedger.ProductId,
            UnitId = stockLedger.UnitId,
            MovementType = stockLedger.MovementType,
            Qty = stockLedger.Qty,
            Balance = stockLedger.Balance,
            Location = stockLedger.Location,
            ReferenceType = stockLedger.ReferenceType,
            ReferenceId = stockLedger.ReferenceId,
            CreatedAt = stockLedger.CreatedAt,
            Note = stockLedger.Note,
            ProductName = stockLedger.Product?.Name,
            UnitName = stockLedger.Unit?.Unit?.Name
        };
    }
}
