using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        return customers.Select(Map);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        return customers.Where(c => c.Status == "Active").Select(Map);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer != null ? Map(customer) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
    {
        var customer = new Customer
        {
            CustomerFullName = customerDto.CustomerFullName,
            BusinessFullName = customerDto.BusinessFullName,
            IsBusiness = customerDto.IsBusiness,
            BusinessTypeId = customerDto.BusinessTypeId,
            CustomerGroupId = customerDto.CustomerGroupId,
            CustomerBalanceAmount = customerDto.CustomerBalanceAmount,
            LicenseNo = customerDto.LicenseNo,
            TrnNo = customerDto.TrnNo,
            MobileNo = customerDto.MobileNo,
            HomePhone = customerDto.HomePhone,
            OfficePhone = customerDto.OfficePhone,
            ContactMobileNo = customerDto.ContactMobileNo,
            OfficialEmail = customerDto.OfficialEmail,
            CreditAllowed = customerDto.CreditAllowed,
            CreditAmountMax = customerDto.CreditAmountMax,
            CreditDays = customerDto.CreditDays,
            CreditReference1Name = customerDto.CreditReference1Name,
            CreditReference2Name = customerDto.CreditReference2Name,
            KeyContactName = customerDto.KeyContactName,
            KeyContactMobile = customerDto.KeyContactMobile,
            KeyContactEmail = customerDto.KeyContactEmail,
            FinancePersonName = customerDto.FinancePersonName,
            FinancePersonMobile = customerDto.FinancePersonMobile,
            FinancePersonEmail = customerDto.FinancePersonEmail,
            PostDatedChequesAllowed = customerDto.PostDatedChequesAllowed,
            ShopId = customerDto.ShopId,
            Status = customerDto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return Map(customer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(CustomerDto customerDto)
    {
        var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customerDto.Id);
        if (existingCustomer == null)
        {
            throw new ArgumentException($"Customer with ID {customerDto.Id} not found.");
        }

        existingCustomer.CustomerFullName = customerDto.CustomerFullName;
        existingCustomer.BusinessFullName = customerDto.BusinessFullName;
        existingCustomer.IsBusiness = customerDto.IsBusiness;
        existingCustomer.BusinessTypeId = customerDto.BusinessTypeId;
        existingCustomer.CustomerGroupId = customerDto.CustomerGroupId;
        existingCustomer.CustomerBalanceAmount = customerDto.CustomerBalanceAmount;
        existingCustomer.LicenseNo = customerDto.LicenseNo;
        existingCustomer.TrnNo = customerDto.TrnNo;
        existingCustomer.MobileNo = customerDto.MobileNo;
        existingCustomer.HomePhone = customerDto.HomePhone;
        existingCustomer.OfficePhone = customerDto.OfficePhone;
        existingCustomer.ContactMobileNo = customerDto.ContactMobileNo;
        existingCustomer.OfficialEmail = customerDto.OfficialEmail;
        existingCustomer.CreditAllowed = customerDto.CreditAllowed;
        existingCustomer.CreditAmountMax = customerDto.CreditAmountMax;
        existingCustomer.CreditDays = customerDto.CreditDays;
        existingCustomer.CreditReference1Name = customerDto.CreditReference1Name;
        existingCustomer.CreditReference2Name = customerDto.CreditReference2Name;
        existingCustomer.KeyContactName = customerDto.KeyContactName;
        existingCustomer.KeyContactMobile = customerDto.KeyContactMobile;
        existingCustomer.KeyContactEmail = customerDto.KeyContactEmail;
        existingCustomer.FinancePersonName = customerDto.FinancePersonName;
        existingCustomer.FinancePersonMobile = customerDto.FinancePersonMobile;
        existingCustomer.FinancePersonEmail = customerDto.FinancePersonEmail;
        existingCustomer.PostDatedChequesAllowed = customerDto.PostDatedChequesAllowed;
        existingCustomer.ShopId = customerDto.ShopId;
        existingCustomer.Status = customerDto.Status;
        existingCustomer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(existingCustomer);
        await _unitOfWork.SaveChangesAsync();

        return Map(existingCustomer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {id} not found.");
        }

        customer.Status = "Deleted";
        customer.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
    {
        var allCustomers = await _unitOfWork.Customers.GetAllAsync();
        
        if (string.IsNullOrEmpty(searchTerm))
        {
            return allCustomers.Where(c => c.Status == "Active").Select(Map);
        }

        var searchTermLower = searchTerm.ToLower();
        var filteredCustomers = allCustomers.Where(c => 
            c.Status == "Active" && 
            ((!string.IsNullOrEmpty(c.CustomerFullName) && c.CustomerFullName.ToLower().Contains(searchTermLower)) ||
             (!string.IsNullOrEmpty(c.BusinessFullName) && c.BusinessFullName.ToLower().Contains(searchTermLower)) ||
             (!string.IsNullOrEmpty(c.OfficialEmail) && c.OfficialEmail.ToLower().Contains(searchTermLower)) ||
             (!string.IsNullOrEmpty(c.MobileNo) && c.MobileNo.Contains(searchTerm)) ||
             (!string.IsNullOrEmpty(c.ContactMobileNo) && c.ContactMobileNo.Contains(searchTerm)) ||
             (!string.IsNullOrEmpty(c.KeyContactName) && c.KeyContactName.ToLower().Contains(searchTermLower)) ||
             c.DisplayName.ToLower().Contains(searchTermLower))
        );

        return filteredCustomers.Select(Map);
    }

    private static CustomerDto Map(Customer c) => new()
    {
        Id = c.Id,
        CustomerFullName = c.CustomerFullName,
        BusinessFullName = c.BusinessFullName,
        IsBusiness = c.IsBusiness,
        BusinessTypeId = c.BusinessTypeId,
        CustomerGroupId = c.CustomerGroupId,
        CustomerBalanceAmount = c.CustomerBalanceAmount,
        LicenseNo = c.LicenseNo,
        TrnNo = c.TrnNo,
        MobileNo = c.MobileNo,
        HomePhone = c.HomePhone,
        OfficePhone = c.OfficePhone,
        ContactMobileNo = c.ContactMobileNo,
        OfficialEmail = c.OfficialEmail,
        CreditAllowed = c.CreditAllowed,
        CreditAmountMax = c.CreditAmountMax,
        CreditDays = c.CreditDays,
        CreditReference1Name = c.CreditReference1Name,
        CreditReference2Name = c.CreditReference2Name,
        KeyContactName = c.KeyContactName,
        KeyContactMobile = c.KeyContactMobile,
        KeyContactEmail = c.KeyContactEmail,
        FinancePersonName = c.FinancePersonName,
        FinancePersonMobile = c.FinancePersonMobile,
        FinancePersonEmail = c.FinancePersonEmail,
        PostDatedChequesAllowed = c.PostDatedChequesAllowed,
        ShopId = c.ShopId,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}