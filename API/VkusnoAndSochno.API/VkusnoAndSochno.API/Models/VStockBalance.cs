using System;
using System.Collections.Generic;

namespace VkusnoAndSochno.API.Models;

public partial class VStockBalance
{
    public int? IdBranch { get; set; }

    public string? BranchName { get; set; }

    public int? IdIngredient { get; set; }

    public string? IngredientName { get; set; }

    public string? UnitOfMeasure { get; set; }

    public decimal? MinRemaining { get; set; }

    public decimal? Balance { get; set; }
}
