using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class Recipe
{
    public int IdRecipe { get; set; }

    public int IdDish { get; set; }

    public string? Description { get; set; }

    public virtual Dish IdDishNavigation { get; set; } = null!;

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
