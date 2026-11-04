using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class VActiveOrder
{
    public int? IdOrder { get; set; }

    public int? IdBranch { get; set; }

    public string? BranchName { get; set; }

    public DateTime? OrderTime { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? EmployeeName { get; set; }
}
