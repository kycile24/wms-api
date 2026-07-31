using Wms.Application.DTOs.Customers;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}