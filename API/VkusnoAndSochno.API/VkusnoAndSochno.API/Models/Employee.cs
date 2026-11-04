using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }

    public int IdBranch { get; set; }

    public int IdPosition { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public DateOnly HireDate { get; set; }

    public virtual Branch IdBranchNavigation { get; set; } = null!;

    public virtual Position IdPositionNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShiftEmployee> ShiftEmployees { get; set; } = new List<ShiftEmployee>();

    public virtual User? User { get; set; }

    public virtual ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
}
