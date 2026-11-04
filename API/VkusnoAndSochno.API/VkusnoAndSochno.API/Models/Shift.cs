using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Shift
{
    public int IdShift { get; set; }

    public int IdBranch { get; set; }

    public DateOnly ShiftDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;

    public virtual Branch IdBranchNavigation { get; set; } = null!;

    public virtual ICollection<ShiftEmployee> ShiftEmployees { get; set; } = new List<ShiftEmployee>();
}
