namespace Wms.Application.DTOs.Warehouses;

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Location,
    int TotalCapacityUnits,
    bool IsActive
);

public record CreateWarehouseDto(
    string Code,
    string Name,
    string Location,
    int TotalCapacityUnits
);

public record UpdateWarehouseDto(
    string Name,
    string Location,
    int TotalCapacityUnits,
    bool IsActive
);