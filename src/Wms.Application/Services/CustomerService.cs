using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Customers;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Repository<Customer>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
            return Result<CustomerDto>.Failure(new Error("Customer.NotFound", $"Клієнта з ID '{id}' не знайдено."));

        return Result<CustomerDto>.Success(MapToDto(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Repository<Customer>()
            .Query()
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CustomerDto>>.Success(customers);
    }

    public async Task<Result<Guid>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone.Trim(),
            ShippingAddress = dto.ShippingAddress?.Trim(),
            BillingAddress = dto.BillingAddress?.Trim()
        };

        await _unitOfWork.Repository<Customer>().AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var customer = await repo.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return Result.Failure(new Error("Customer.NotFound", $"Клієнта з ID '{id}' не знайдено."));

        customer.Name = dto.Name.Trim();
        customer.ContactPerson = dto.ContactPerson.Trim();
        customer.Email = dto.Email.Trim().ToLowerInvariant();
        customer.Phone = dto.Phone.Trim();
        customer.ShippingAddress = dto.ShippingAddress?.Trim();
        customer.BillingAddress = dto.BillingAddress?.Trim();

        repo.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var customer = await repo.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return Result.Failure(new Error("Customer.NotFound", $"Клієнта з ID '{id}' не знайдено."));

        repo.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CustomerDto MapToDto(Customer c) => new(
        c.Id,
        c.Name,
        c.ContactPerson,
        c.Email,
        c.Phone,
        c.ShippingAddress,
        c.BillingAddress
    );
}