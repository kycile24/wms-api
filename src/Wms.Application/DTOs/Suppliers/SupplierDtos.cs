namespace Wms.Application.DTOs.Suppliers;

public record SupplierDto(
    Guid Id,
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? Address,
    string? TaxId
);

public record CreateSupplierDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? Address,
    string? TaxId
);

public record UpdateSupplierDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? Address,
    string? TaxId
);