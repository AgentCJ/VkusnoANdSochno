using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class RecipeIngredient
{
    public int IdRecipe { get; set; }

    public int IdIngredient { get; set; }

    public decimal Quantity { get; set; }

    public virtual Ingredient IdIngredientNavigation { get; set; } = null!;

    public virtual Recipe IdRecipeNavigation { get; set; } = null!;
}
