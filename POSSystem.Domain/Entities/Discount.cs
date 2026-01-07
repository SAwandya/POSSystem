using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;

namespace POSSystem.Domain.Entities;

public class Discount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;
}
