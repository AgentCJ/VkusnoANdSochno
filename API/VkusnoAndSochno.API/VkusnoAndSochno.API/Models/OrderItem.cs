using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class OrderItem
{
    public int IdOrder { get; set; }

    public int IdDish { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtSale { get; set; }

    public virtual Dish IdDishNavigation { get; set; } = null!;

    public virtual Order IdOrderNavigation { get; set; } = null!;
}
