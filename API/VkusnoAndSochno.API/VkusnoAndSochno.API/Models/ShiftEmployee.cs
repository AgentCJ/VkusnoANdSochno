using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class ShiftEmployee
{
    public int IdShift { get; set; }

    public int IdEmployee { get; set; }

    public decimal? HoursWorked { get; set; }

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual Shift IdShiftNavigation { get; set; } = null!;
}
