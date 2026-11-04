using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Branch
{
    public int IdBranch { get; set; }

    public string BranchName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
}
