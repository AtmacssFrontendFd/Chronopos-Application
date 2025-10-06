using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Customer Group operations
/// </summary>
public class CustomerGroupService : ICustomerGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerGroupRelationService _customerGroupRelationService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IDiscountService _discountService;

    public CustomerGroupService(
        IUnitOfWork unitOfWork,
        ICustomerGroupRelationService customerGroupRelationService,
        ISellingPriceTypeService sellingPriceTypeService,
        IDiscountService discountService)
    {
        _unitOfWork = unitOfWork;
        _customerGroupRelationService = customerGroupRelationService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _discountService = discountService;
    }

    public async Task<IEnumerable<CustomerGroupDto>> GetAllAsync()
    {
        var customerGroups = await _unitOfWork.CustomerGroups.GetAllAsync();
        var result = new List<CustomerGroupDto>();
        
        foreach (var cg in customerGroups.Where(cg => cg.DeletedAt == null))
        {
            result.Add(await MapAsync(cg));
        }
        
        return result;
    }

    public async Task<CustomerGroupDto?> GetByIdAsync(int id)
    {
        var customerGroup = await _unitOfWork.CustomerGroups.GetByIdAsync(id);
        return customerGroup != null && customerGroup.DeletedAt == null ? await MapAsync(customerGroup) : null;
    }

    public async Task<CustomerGroupDto> CreateAsync(CreateCustomerGroupDto createDto)
    {
        var customerGroup = new CustomerGroup
        {
            Name = createDto.Name,
            NameAr = createDto.NameAr,
            SellingPriceTypeId = createDto.SellingPriceTypeId,
            DiscountId = createDto.DiscountId,
            DiscountValue = createDto.DiscountValue,
            DiscountMaxValue = createDto.DiscountMaxValue,
            IsPercentage = createDto.IsPercentage,
            Status = createDto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CustomerGroups.AddAsync(customerGroup);
        await _unitOfWork.SaveChangesAsync();

        return await MapAsync(customerGroup);
    }

    public async Task<CustomerGroupDto> UpdateAsync(UpdateCustomerGroupDto updateDto)
    {
        var customerGroup = await _unitOfWork.CustomerGroups.GetByIdAsync(updateDto.Id);
        if (customerGroup == null || customerGroup.DeletedAt != null)
        {
            throw new InvalidOperationException($"Customer group with ID {updateDto.Id} not found.");
        }

        customerGroup.Name = updateDto.Name;
        customerGroup.NameAr = updateDto.NameAr;
        customerGroup.SellingPriceTypeId = updateDto.SellingPriceTypeId;
        customerGroup.DiscountId = updateDto.DiscountId;
        customerGroup.DiscountValue = updateDto.DiscountValue;
        customerGroup.DiscountMaxValue = updateDto.DiscountMaxValue;
        customerGroup.IsPercentage = updateDto.IsPercentage;
        customerGroup.Status = updateDto.Status;
        customerGroup.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CustomerGroups.UpdateAsync(customerGroup);
        await _unitOfWork.SaveChangesAsync();

        return await MapAsync(customerGroup);
    }

    public async Task DeleteAsync(int id)
    {
        var customerGroup = await _unitOfWork.CustomerGroups.GetByIdAsync(id);
        if (customerGroup == null || customerGroup.DeletedAt != null)
        {
            throw new InvalidOperationException($"Customer group with ID {id} not found.");
        }

        // Check if any customers are assigned to this group
        var allCustomers = await _unitOfWork.Customers.GetAllAsync();
        var customersCount = allCustomers.Count(c => c.CustomerGroupId == id && c.DeletedAt == null);
        if (customersCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete customer group. {customersCount} customers are assigned to this group.");
        }

        // Soft delete
        customerGroup.DeletedAt = DateTime.UtcNow;
        customerGroup.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CustomerGroups.UpdateAsync(customerGroup);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CustomerGroupDto>> SearchAsync(string searchTerm)
    {
        var customerGroups = await _unitOfWork.CustomerGroups.GetAllAsync();
        var filtered = customerGroups
            .Where(cg => cg.DeletedAt == null && 
                        (cg.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                         (!string.IsNullOrEmpty(cg.NameAr) && cg.NameAr.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))));
        
        var result = new List<CustomerGroupDto>();
        foreach (var cg in filtered)
        {
            result.Add(await MapAsync(cg));
        }
        
        return result;
    }

    public async Task<IEnumerable<CustomerGroupDto>> GetActiveAsync()
    {
        var customerGroups = await _unitOfWork.CustomerGroups.GetAllAsync();
        var active = customerGroups.Where(cg => cg.DeletedAt == null && cg.Status == "Active");
        
        var result = new List<CustomerGroupDto>();
        foreach (var cg in active)
        {
            result.Add(await MapAsync(cg));
        }
        
        return result;
    }

    public async Task<int> GetCountAsync()
    {
        var customerGroups = await _unitOfWork.CustomerGroups.GetAllAsync();
        return customerGroups.Count(cg => cg.DeletedAt == null);
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var customerGroups = await _unitOfWork.CustomerGroups.GetAllAsync();
        return customerGroups.Any(cg => 
            cg.DeletedAt == null && 
            cg.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
            (excludeId == null || cg.Id != excludeId));
    }

    private async Task<CustomerGroupDto> MapAsync(CustomerGroup customerGroup)
    {
        // Get customer count from relations
        var relations = await _customerGroupRelationService.GetByCustomerGroupIdAsync(customerGroup.Id);
        var customerCount = relations.Count();
        
        // Get selling price type name
        string? sellingPriceTypeName = null;
        if (customerGroup.SellingPriceTypeId.HasValue)
        {
            var priceType = await _sellingPriceTypeService.GetByIdAsync(customerGroup.SellingPriceTypeId.Value);
            sellingPriceTypeName = priceType?.TypeName;
        }
        
        // Get discount name
        string? discountName = null;
        if (customerGroup.DiscountId.HasValue)
        {
            var discount = await _discountService.GetByIdAsync(customerGroup.DiscountId.Value);
            discountName = discount?.DiscountName;
        }
        
        return new CustomerGroupDto
        {
            Id = customerGroup.Id,
            Name = customerGroup.Name,
            NameAr = customerGroup.NameAr,
            SellingPriceTypeId = customerGroup.SellingPriceTypeId,
            SellingPriceTypeName = sellingPriceTypeName,
            DiscountId = customerGroup.DiscountId,
            DiscountName = discountName,
            DiscountValue = customerGroup.DiscountValue,
            DiscountMaxValue = customerGroup.DiscountMaxValue,
            IsPercentage = customerGroup.IsPercentage,
            Status = customerGroup.Status,
            CustomerCount = customerCount,
            CreatedBy = customerGroup.CreatedBy,
            CreatedAt = customerGroup.CreatedAt,
            UpdatedBy = customerGroup.UpdatedBy,
            UpdatedAt = customerGroup.UpdatedAt,
            DeletedAt = customerGroup.DeletedAt,
            DeletedBy = customerGroup.DeletedBy
        };
    }
}