using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int IdEmployee { get; set; }

    public int IdBranch { get; set; }

    public DateTime OrderTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public virtual Branch IdBranchNavigation { get; set; } = null!;

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
}
