
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Extended Recipe", menuName = "Card Game/Extended Crafting Recipe")]
public class CraftingRecipeExtended : CraftingRecipe
{
    [Header("Extended Crafting Behavior")]
    public IngredientBehavior ingredient1Behavior = IngredientBehavior.Consume;
    public IngredientBehavior ingredient2Behavior = IngredientBehavior.Consume;

    [Header("Durability Settings")]
    public bool useDurability = false;

    [Header("Durability Cost")]
    public int durabilityCost = 1;

    [Header("Extended Description")]
    [TextArea]
    public string extendedDescription;
}

/// <summary>
/// Behavior of ingredients in extended crafting
/// </summary>
public enum IngredientBehavior
{
    Consume,        // Completely consume the ingredient
    Keep,           // Keep the ingredient (return to inventory)
    ReduceDurability // Reduce durability of the ingredient
}
