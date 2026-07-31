namespace Wms.Application.DTOs.Customers;

public record CustomerDto(
    Guid Id,
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? ShippingAddress,
    string? BillingAddress
);

public record CreateCustomerDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? ShippingAddress,
    string? BillingAddress
);

public record UpdateCustomerDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string? ShippingAddress,
    string? BillingAddress
);