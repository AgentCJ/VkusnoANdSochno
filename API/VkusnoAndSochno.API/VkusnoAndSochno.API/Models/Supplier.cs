using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Supplier
{
    public int IdSupplier { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
}
