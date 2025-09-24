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
        return customers.Where(c => c.IsActive).Select(Map);
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
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Email = customerDto.Email,
            PhoneNumber = customerDto.PhoneNumber,
            Address = customerDto.Address,
            IsActive = customerDto.IsActive,
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

        existingCustomer.FirstName = customerDto.FirstName;
        existingCustomer.LastName = customerDto.LastName;
        existingCustomer.Email = customerDto.Email;
        existingCustomer.PhoneNumber = customerDto.PhoneNumber;
        existingCustomer.Address = customerDto.Address;
        existingCustomer.IsActive = customerDto.IsActive;
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

        await _unitOfWork.Customers.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
    {
        var allCustomers = await _unitOfWork.Customers.GetAllAsync();
        
        if (string.IsNullOrEmpty(searchTerm))
        {
            return allCustomers.Where(c => c.IsActive).Select(Map);
        }

        var searchTermLower = searchTerm.ToLower();
        var filteredCustomers = allCustomers.Where(c => 
            c.IsActive && 
            (c.FirstName.ToLower().Contains(searchTermLower) ||
             c.LastName.ToLower().Contains(searchTermLower) ||
             c.Email.ToLower().Contains(searchTermLower) ||
             c.PhoneNumber.Contains(searchTerm) ||
             c.Address.ToLower().Contains(searchTermLower) ||
             c.FullName.ToLower().Contains(searchTermLower))
        );

        return filteredCustomers.Select(Map);
    }

    private static CustomerDto Map(Customer c) => new()
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        PhoneNumber = c.PhoneNumber,
        Address = c.Address,
        IsActive = c.IsActive,
        FullName = c.FullName
    };
}