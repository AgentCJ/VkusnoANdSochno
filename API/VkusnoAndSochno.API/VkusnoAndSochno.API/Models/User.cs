using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class User
{
    public int IdUser { get; set; }

    public int IdEmployee { get; set; }

    public int IdRole { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
