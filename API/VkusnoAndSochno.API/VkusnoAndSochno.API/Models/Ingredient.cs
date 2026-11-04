using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Ingredient
{
    public int IdIngredient { get; set; }

    public string IngredientName { get; set; } = null!;

    public string UnitOfMeasure { get; set; } = null!;

    public decimal CostPrice { get; set; }

    public decimal MinRemaining { get; set; }

    public virtual ICollection<OperationItem> OperationItems { get; set; } = new List<OperationItem>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
