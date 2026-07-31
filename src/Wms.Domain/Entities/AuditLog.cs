using Wms.Domain.Common;

namespace Wms.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ChangesJson { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}