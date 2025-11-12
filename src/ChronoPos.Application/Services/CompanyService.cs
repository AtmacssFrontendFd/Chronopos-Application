using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(IUnitOfWork unitOfWork, ICompanyRepository companyRepository)
    {
        _unitOfWork = unitOfWork;
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        var companies = await _companyRepository.GetAllAsync();
        return companies
            .Where(c => c.DeletedAt == null)
            .Select(MapToDto);
    }

    public async Task<IEnumerable<CompanyDto>> GetActiveAsync()
    {
        var companies = await _companyRepository.GetActiveCompaniesAsync();
        return companies.Select(MapToDto);
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return company != null && company.DeletedAt == null ? MapToDto(company) : null;
    }

    public async Task<CompanyDto?> GetByNameAsync(string name)
    {
        var company = await _companyRepository.GetByNameAsync(name);
        return company != null ? MapToDto(company) : null;
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto createDto, string createdBy)
    {
        // Check if company with same name already exists
        if (await _companyRepository.CompanyExistsAsync(createDto.CompanyName))
        {
            throw new InvalidOperationException($"Company with name '{createDto.CompanyName}' already exists.");
        }

        var company = new Company
        {
            CompanyName = createDto.CompanyName,
            LogoPath = createDto.LogoPath,
            LicenseNumber = createDto.LicenseNumber,
            NumberOfOwners = createDto.NumberOfOwners,
            VatTrnNumber = createDto.VatTrnNumber,
            PhoneNo = createDto.PhoneNo,
            EmailOfBusiness = createDto.EmailOfBusiness,
            Website = createDto.Website,
            KeyContactName = createDto.KeyContactName,
            KeyContactMobNo = createDto.KeyContactMobNo,
            KeyContactEmail = createDto.KeyContactEmail,
            LocationLatitude = createDto.LocationLatitude,
            LocationLongitude = createDto.LocationLongitude,
            Remarks = createDto.Remarks,
            Status = createDto.Status,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(company);
    }

    public async Task<CompanyDto> UpdateAsync(int id, UpdateCompanyDto updateDto, string updatedBy)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null || company.DeletedAt != null)
        {
            throw new ArgumentException("Company not found.");
        }

        // Check if another company with same name exists
        if (await _companyRepository.CompanyExistsAsync(updateDto.CompanyName, id))
        {
            throw new InvalidOperationException($"Another company with name '{updateDto.CompanyName}' already exists.");
        }

        company.CompanyName = updateDto.CompanyName;
        company.LogoPath = updateDto.LogoPath;
        company.LicenseNumber = updateDto.LicenseNumber;
        company.NumberOfOwners = updateDto.NumberOfOwners;
        company.VatTrnNumber = updateDto.VatTrnNumber;
        company.PhoneNo = updateDto.PhoneNo;
        company.EmailOfBusiness = updateDto.EmailOfBusiness;
        company.Website = updateDto.Website;
        company.KeyContactName = updateDto.KeyContactName;
        company.KeyContactMobNo = updateDto.KeyContactMobNo;
        company.KeyContactEmail = updateDto.KeyContactEmail;
        company.LocationLatitude = updateDto.LocationLatitude;
        company.LocationLongitude = updateDto.LocationLongitude;
        company.Remarks = updateDto.Remarks;
        company.Status = updateDto.Status;
        company.UpdatedBy = updatedBy;
        company.UpdatedAt = DateTime.UtcNow;

        await _companyRepository.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(company);
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null || company.DeletedAt != null)
        {
            return false;
        }

        // Soft delete
        company.DeletedBy = deletedBy;
        company.DeletedAt = DateTime.UtcNow;
        company.Status = false;

        await _companyRepository.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompanyExistsAsync(string name, int? excludeId = null)
    {
        return await _companyRepository.CompanyExistsAsync(name, excludeId);
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            LogoPath = company.LogoPath,
            LicenseNumber = company.LicenseNumber,
            NumberOfOwners = company.NumberOfOwners,
            VatTrnNumber = company.VatTrnNumber,
            PhoneNo = company.PhoneNo,
            EmailOfBusiness = company.EmailOfBusiness,
            Website = company.Website,
            KeyContactName = company.KeyContactName,
            KeyContactMobNo = company.KeyContactMobNo,
            KeyContactEmail = company.KeyContactEmail,
            LocationLatitude = company.LocationLatitude,
            LocationLongitude = company.LocationLongitude,
            Remarks = company.Remarks,
            Status = company.Status,
            CreatedBy = company.CreatedBy,
            CreatedAt = company.CreatedAt,
            UpdatedBy = company.UpdatedBy,
            UpdatedAt = company.UpdatedAt
        };
    }
}
