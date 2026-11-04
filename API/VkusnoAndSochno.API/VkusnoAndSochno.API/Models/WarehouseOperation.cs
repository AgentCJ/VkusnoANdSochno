using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class WarehouseOperation
{
    public int IdOperation { get; set; }

    public int IdEmployee { get; set; }

    public int IdBranch { get; set; }

    public int? IdSupplier { get; set; }

    public int? IdOrder { get; set; }

    public DateTime OperationDate { get; set; }

    public string OperationType { get; set; } = null!;

    public string? Comment { get; set; }

    public virtual Branch IdBranchNavigation { get; set; } = null!;

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual Order? IdOrderNavigation { get; set; }

    public virtual Supplier? IdSupplierNavigation { get; set; }

    public virtual ICollection<OperationItem> OperationItems { get; set; } = new List<OperationItem>();
}
