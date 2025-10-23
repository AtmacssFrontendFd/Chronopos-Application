using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        try
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            return suppliers.Select(MapToDto);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error retrieving suppliers: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        return await GetAllAsync();
    }

    public async Task<SupplierDto?> GetByIdAsync(long id)
    {
        try
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            return supplier != null ? MapToDto(supplier) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error retrieving supplier with ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(supplierDto.CompanyName))
                throw new ArgumentException("Company name is required.");

            if (string.IsNullOrWhiteSpace(supplierDto.AddressLine1))
                throw new ArgumentException("Address line 1 is required.");

            // Check for duplicate email if provided
            if (!string.IsNullOrWhiteSpace(supplierDto.Email))
            {
                var existingSupplier = await _unitOfWork.Suppliers.GetByEmailAsync(supplierDto.Email);
                if (existingSupplier != null)
                    throw new ArgumentException("A supplier with this email already exists.");
            }

            var supplier = MapToEntity(supplierDto);
            supplier.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(supplier);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error creating supplier: {ex.Message}", ex);
        }
    }

    public async Task<SupplierDto> UpdateSupplierAsync(SupplierDto supplierDto)
    {
        try
        {
            var existingSupplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierDto.SupplierId);
            if (existingSupplier == null)
                throw new ArgumentException("Supplier not found.");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(supplierDto.CompanyName))
                throw new ArgumentException("Company name is required.");

            if (string.IsNullOrWhiteSpace(supplierDto.AddressLine1))
                throw new ArgumentException("Address line 1 is required.");

            // Check for duplicate email if provided and different from current
            if (!string.IsNullOrWhiteSpace(supplierDto.Email) && 
                supplierDto.Email != existingSupplier.Email)
            {
                var duplicateSupplier = await _unitOfWork.Suppliers.GetByEmailAsync(supplierDto.Email);
                if (duplicateSupplier != null && duplicateSupplier.SupplierId != supplierDto.SupplierId)
                    throw new ArgumentException("A supplier with this email already exists.");
            }

            // Update properties
            existingSupplier.CompanyName = supplierDto.CompanyName;
            existingSupplier.LogoPicture = supplierDto.LogoPicture;
            existingSupplier.LicenseNumber = supplierDto.LicenseNumber;
            existingSupplier.OwnerName = supplierDto.OwnerName;
            existingSupplier.OwnerMobile = supplierDto.OwnerMobile;
            existingSupplier.VatTrnNumber = supplierDto.VatTrnNumber;
            existingSupplier.Email = supplierDto.Email;
            existingSupplier.AddressLine1 = supplierDto.AddressLine1;
            existingSupplier.AddressLine2 = supplierDto.AddressLine2;
            existingSupplier.Building = supplierDto.Building;
            existingSupplier.Area = supplierDto.Area;
            existingSupplier.PoBox = supplierDto.PoBox;
            existingSupplier.City = supplierDto.City;
            existingSupplier.State = supplierDto.State;
            existingSupplier.Country = supplierDto.Country;
            existingSupplier.Website = supplierDto.Website;
            existingSupplier.KeyContactName = supplierDto.KeyContactName;
            existingSupplier.KeyContactMobile = supplierDto.KeyContactMobile;
            existingSupplier.KeyContactEmail = supplierDto.KeyContactEmail;
            existingSupplier.Mobile = supplierDto.Mobile;
            existingSupplier.LocationLatitude = supplierDto.LocationLatitude;
            existingSupplier.LocationLongitude = supplierDto.LocationLongitude;
            existingSupplier.CompanyPhoneNumber = supplierDto.CompanyPhoneNumber;
            existingSupplier.Gstin = supplierDto.Gstin;
            existingSupplier.Pan = supplierDto.Pan;
            existingSupplier.PaymentTerms = supplierDto.PaymentTerms;
            existingSupplier.OpeningBalance = supplierDto.OpeningBalance;
            existingSupplier.BalanceType = supplierDto.BalanceType;
            existingSupplier.Status = supplierDto.Status;
            existingSupplier.CreditLimit = supplierDto.CreditLimit;
            existingSupplier.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Suppliers.UpdateAsync(existingSupplier);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(existingSupplier);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error updating supplier: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteSupplierAsync(long id)
    {
        try
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier == null)
                return false;

            await _unitOfWork.Suppliers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error deleting supplier: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm)
    {
        try
        {
            var suppliers = await _unitOfWork.Suppliers.SearchSuppliersAsync(searchTerm);
            return suppliers.Select(MapToDto);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error searching suppliers: {ex.Message}", ex);
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _unitOfWork.Suppliers.GetTotalCountAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error getting supplier count: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<SupplierDto>> GetActiveAsync()
    {
        try
        {
            var suppliers = await _unitOfWork.Suppliers.GetActiveAsync();
            return suppliers.Select(MapToDto);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error retrieving active suppliers: {ex.Message}", ex);
        }
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            ShopId = supplier.ShopId,
            CompanyName = supplier.CompanyName,
            LogoPicture = supplier.LogoPicture,
            LicenseNumber = supplier.LicenseNumber,
            OwnerName = supplier.OwnerName,
            OwnerMobile = supplier.OwnerMobile,
            VatTrnNumber = supplier.VatTrnNumber,
            Email = supplier.Email,
            AddressLine1 = supplier.AddressLine1,
            AddressLine2 = supplier.AddressLine2,
            Building = supplier.Building,
            Area = supplier.Area,
            PoBox = supplier.PoBox,
            City = supplier.City,
            State = supplier.State,
            Country = supplier.Country,
            Website = supplier.Website,
            KeyContactName = supplier.KeyContactName,
            KeyContactMobile = supplier.KeyContactMobile,
            KeyContactEmail = supplier.KeyContactEmail,
            Mobile = supplier.Mobile,
            LocationLatitude = supplier.LocationLatitude,
            LocationLongitude = supplier.LocationLongitude,
            CompanyPhoneNumber = supplier.CompanyPhoneNumber,
            Gstin = supplier.Gstin,
            Pan = supplier.Pan,
            PaymentTerms = supplier.PaymentTerms,
            OpeningBalance = supplier.OpeningBalance,
            BalanceType = supplier.BalanceType,
            Status = supplier.Status,
            CreditLimit = supplier.CreditLimit,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }

    private static Supplier MapToEntity(SupplierDto dto)
    {
        return new Supplier
        {
            SupplierId = dto.SupplierId,
            ShopId = dto.ShopId,
            CompanyName = dto.CompanyName,
            LogoPicture = dto.LogoPicture,
            LicenseNumber = dto.LicenseNumber,
            OwnerName = dto.OwnerName,
            OwnerMobile = dto.OwnerMobile,
            VatTrnNumber = dto.VatTrnNumber,
            Email = dto.Email,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            Building = dto.Building,
            Area = dto.Area,
            PoBox = dto.PoBox,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            Website = dto.Website,
            KeyContactName = dto.KeyContactName,
            KeyContactMobile = dto.KeyContactMobile,
            KeyContactEmail = dto.KeyContactEmail,
            Mobile = dto.Mobile,
            LocationLatitude = dto.LocationLatitude,
            LocationLongitude = dto.LocationLongitude,
            CompanyPhoneNumber = dto.CompanyPhoneNumber,
            Gstin = dto.Gstin,
            Pan = dto.Pan,
            PaymentTerms = dto.PaymentTerms,
            OpeningBalance = dto.OpeningBalance,
            BalanceType = dto.BalanceType,
            Status = dto.Status,
            CreditLimit = dto.CreditLimit
        };
    }
}
