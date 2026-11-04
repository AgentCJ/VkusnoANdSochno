using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Dish
{
    public int IdDish { get; set; }

    public string DishName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
