using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class OperationItem
{
    public int IdOperation { get; set; }

    public int IdIngredient { get; set; }

    public decimal Quantity { get; set; }

    public virtual Ingredient IdIngredientNavigation { get; set; } = null!;

    public virtual WarehouseOperation IdOperationNavigation { get; set; } = null!;
}
