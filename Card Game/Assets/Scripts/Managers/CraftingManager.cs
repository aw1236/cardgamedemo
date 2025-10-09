
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("Basic Recipes")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    [Header("Extended Recipes (Durability Support)")]
    public List<CraftingRecipeExtended> extendedRecipes = new List<CraftingRecipeExtended>();

    [Header("Crafting Slots")]
    public CardSlot craftingSlot1;
    public CardSlot craftingSlot2;

    private bool isProcessingCrafting = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CraftingRecipe FindMatchingRecipe(CardData card1, CardData card2)
    {
        // First check extended recipes
        foreach (CraftingRecipeExtended extendedRecipe in extendedRecipes)
        {
            if (extendedRecipe.Matches(card1, card2))
            {
                return extendedRecipe;
            }
        }

        // Then check basic recipes
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.Matches(card1, card2))
            {
                return recipe;
            }
        }
        return null;
    }

    public void CheckForCrafting(CardSlot triggeredSlot)
    {
        if (isProcessingCrafting)
        {
            Debug.LogWarning("Crafting is already in process, skipping...");
            return;
        }

        if (craftingSlot1 == null || craftingSlot2 == null ||
            craftingSlot1.GetCardData() == null || craftingSlot2.GetCardData() == null)
        {
            Debug.LogWarning("Crafting slots are not properly set up");
            return;
        }

        StartCoroutine(ProcessCraftingCheck(triggeredSlot));
    }

    private IEnumerator ProcessCraftingCheck(CardSlot triggeredSlot)
    {
        isProcessingCrafting = true;

        try
        {
            yield return null;

            CardData card1 = craftingSlot1.GetCardData();
            CardData card2 = craftingSlot2.GetCardData();

            if (card1 == null || card2 == null)
            {
                Debug.LogWarning("One of the crafting slots is empty");
                yield break;
            }

            CraftingRecipe recipe = FindMatchingRecipe(card1, card2);

            if (recipe != null)
            {
                Debug.Log($"Instant Crafting: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

                // Handle crafting with extended behavior
                yield return StartCoroutine(PerformCraftingWithBehavior(recipe, card1, card2, triggeredSlot));
            }
            else
            {
                Debug.Log($"No matching recipe: {card1.cardName} + {card2.cardName}");

                // FIX: Restore the second card return mechanism
                if (triggeredSlot != null)
                {
                    // Return the card that was just placed
                    triggeredSlot.ForceRemoveCard();
                }
                else
                {
                    // If we don't know which slot was triggered, return the second card
                    craftingSlot2.ForceRemoveCard();
                }
            }
        }
        finally
        {
            // FIX: Ensure this flag is always reset to prevent deadlock
            isProcessingCrafting = false;
        }
    }

    /// <summary>
    /// Perform crafting with extended behavior support
    /// </summary>
    private IEnumerator PerformCraftingWithBehavior(CraftingRecipe recipe, CardData card1, CardData card2, CardSlot triggeredSlot)
    {
        CraftingRecipeExtended extendedRecipe = recipe as CraftingRecipeExtended;

        if (extendedRecipe != null)
        {
            yield return StartCoroutine(HandleExtendedRecipe(extendedRecipe, card1, card2, triggeredSlot));
        }
        else
        {
            // Normal crafting behavior
            yield return StartCoroutine(PerformNormalCrafting(recipe));
        }
    }

    /// <summary>
    /// Handle extended recipe with special behaviors
    /// </summary>
    private IEnumerator HandleExtendedRecipe(CraftingRecipeExtended recipe, CardData card1, CardData card2, CardSlot triggeredSlot)
    {
        Debug.Log($"Handling extended recipe: {recipe.ingredient1Behavior} + {recipe.ingredient2Behavior}");

        // FIX: Store references BEFORE any clearing
        CardView cardView1 = craftingSlot1.CurrentCardView;
        CardView cardView2 = craftingSlot2.CurrentCardView;

        if (cardView1 == null || cardView2 == null)
        {
            Debug.LogError("Card views are null, cannot proceed with extended crafting");
            yield break;
        }

        // FIX: Determine which slot should get the result
        // For durability recipes, put result in the slot of the consumed ingredient
        Transform resultSlotTransform = craftingSlot1.transform;
        CardSlot resultSlot = craftingSlot1;

        if (recipe.ingredient1Behavior == IngredientBehavior.ReduceDurability &&
            recipe.ingredient2Behavior == IngredientBehavior.Consume)
        {
            // If ingredient1 is durability and ingredient2 is consume, put result in slot2
            resultSlotTransform = craftingSlot2.transform;
            resultSlot = craftingSlot2;
        }

        // Handle ingredient behaviors
        bool destroyCard1 = true;
        bool destroyCard2 = true;

        // Process ingredient 1
        if (recipe.ingredient1Behavior == IngredientBehavior.ReduceDurability)
        {
            destroyCard1 = ReduceCardDurability(cardView1, recipe.durabilityCost);
            if (!destroyCard1)
            {
                Debug.Log($"Keeping {card1.cardName} with reduced durability");
            }
        }
        else if (recipe.ingredient1Behavior == IngredientBehavior.Keep)
        {
            destroyCard1 = false;
        }

        // Process ingredient 2
        if (recipe.ingredient2Behavior == IngredientBehavior.ReduceDurability)
        {
            destroyCard2 = ReduceCardDurability(cardView2, recipe.durabilityCost);
            if (!destroyCard2)
            {
                Debug.Log($"Keeping {card2.cardName} with reduced durability");
            }
        }
        else if (recipe.ingredient2Behavior == IngredientBehavior.Keep)
        {
            destroyCard2 = false;
        }

        // FIX: Clear slots only after processing durability
        if (destroyCard1)
        {
            craftingSlot1.ClearSlot();
        }
        else
        {
            // If we're keeping card1, make sure it stays in its slot
            cardView1.transform.SetParent(craftingSlot1.transform);
            cardView1.transform.localPosition = Vector3.zero;
            craftingSlot1.CurrentCardView = cardView1;
        }

        if (destroyCard2)
        {
            craftingSlot2.ClearSlot();
        }
        else
        {
            // If we're keeping card2, make sure it stays in its slot
            cardView2.transform.SetParent(craftingSlot2.transform);
            cardView2.transform.localPosition = Vector3.zero;
            craftingSlot2.CurrentCardView = cardView2;
        }

        yield return null;

        // Create the result card in the appropriate slot
        if (resultSlot.CurrentCardView == null) // Only create if the slot is empty
        {
            GameObject newCard = CardManager.Instance.CreateCard(recipe.result, resultSlotTransform);
            SetupNewCardInSlot(newCard.GetComponent<RectTransform>());

            CardView newCardView = newCard.GetComponent<CardView>();
            if (newCardView != null)
            {
                resultSlot.CurrentCardView = newCardView;
            }

            // Play crafting effects
            yield return StartCoroutine(PlayCraftingEffects(newCard));
        }
        else
        {
            Debug.LogWarning($"Result slot is not empty, cannot place {recipe.result.cardName}");
        }
    }

    /// <summary>
    /// Reduce card durability and return whether the card should be destroyed
    /// </summary>
    private bool ReduceCardDurability(CardView cardView, int durabilityCost)
    {
        CardData cardData = cardView.GetCardData();

        // 创建 CardData 的临时副本来修改，而不是直接修改预设体
        CardData tempCardData = null;

        if (cardData is WeaponCardData weaponData)
        {
            // 创建武器数据的副本
            WeaponCardData tempWeaponData = ScriptableObject.CreateInstance<WeaponCardData>();
            tempWeaponData.cardName = weaponData.cardName;
            tempWeaponData.cardType = weaponData.cardType;
            tempWeaponData.icon = weaponData.icon;
            tempWeaponData.description = weaponData.description;
            tempWeaponData.backgroundColor = weaponData.backgroundColor;
            tempWeaponData.cardBackgroundPrefab = weaponData.cardBackgroundPrefab;
            tempWeaponData.attack = weaponData.attack;
            tempWeaponData.defense = weaponData.defense;
            tempWeaponData.durability = Mathf.Max(0, weaponData.durability - durabilityCost);
            tempWeaponData.maxDurability = weaponData.maxDurability;

            tempCardData = tempWeaponData;
            Debug.Log($"Reduced {tempWeaponData.cardName} durability to {tempWeaponData.durability} (from {weaponData.durability})");

            // 更新卡牌显示使用临时数据
            cardView.Setup(tempCardData);

            // 检查如果耐久度为零则销毁卡牌
            if (tempWeaponData.durability <= 0)
            {
                Debug.Log($"{tempWeaponData.cardName} broke due to zero durability!");
                return true;
            }
            return false;
        }
        else if (cardData is ArmorCardData armorData)
        {
            // 创建防具数据的副本
            ArmorCardData tempArmorData = ScriptableObject.CreateInstance<ArmorCardData>();
            tempArmorData.cardName = armorData.cardName;
            tempArmorData.cardType = armorData.cardType;
            tempArmorData.icon = armorData.icon;
            tempArmorData.description = armorData.description;
            tempArmorData.backgroundColor = armorData.backgroundColor;
            tempArmorData.cardBackgroundPrefab = armorData.cardBackgroundPrefab;
            tempArmorData.attack = armorData.attack;
            tempArmorData.defense = armorData.defense;
            tempArmorData.durability = Mathf.Max(0, armorData.durability - durabilityCost);
            tempArmorData.maxDurability = armorData.MaxDurability;

            tempCardData = tempArmorData;
            Debug.Log($"Reduced {tempArmorData.cardName} durability to {tempArmorData.durability} (from {armorData.durability})");

            // 更新卡牌显示使用临时数据
            cardView.Setup(tempCardData);

            // 检查如果耐久度为零则销毁卡牌
            if (tempArmorData.durability <= 0)
            {
                Debug.Log($"{tempArmorData.cardName} broke due to zero durability!");
                return true;
            }
            return false;
        }

        // 如果不是耐久度卡牌，直接消耗
        Debug.LogWarning($"{cardData.cardName} is not a durability card, consuming it");
        return true;
    }

    /// <summary>
    /// Normal crafting - consume both ingredients
    /// </summary>
    private IEnumerator PerformNormalCrafting(CraftingRecipe recipe)
    {
        Transform resultSlotTransform = craftingSlot1.transform;

        // Consume both cards
        craftingSlot1.ClearSlot();
        craftingSlot2.ClearSlot();

        yield return null;

        // Create result card in first slot
        GameObject newCard = CardManager.Instance.CreateCard(recipe.result, resultSlotTransform);
        SetupNewCardInSlot(newCard.GetComponent<RectTransform>());

        CardView newCardView = newCard.GetComponent<CardView>();
        if (newCardView != null)
        {
            craftingSlot1.CurrentCardView = newCardView;
        }

        yield return StartCoroutine(PlayCraftingEffects(newCard));
    }

    /// <summary>
    /// Setup new card in slot
    /// </summary>
    private void SetupNewCardInSlot(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        StartCoroutine(EnsureNewCardSize(cardRT));
    }

    /// <summary>
    /// Play crafting effects
    /// </summary>
    private IEnumerator PlayCraftingEffects(GameObject newCard)
    {
        if (newCard != null)
        {
            RectTransform cardRT = newCard.GetComponent<RectTransform>();
            Vector3 originalScale = cardRT.localScale;

            cardRT.localScale = Vector3.zero;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cardRT.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
                yield return null;
            }

            cardRT.localScale = originalScale;
        }
    }

    private IEnumerator EnsureNewCardSize(RectTransform cardRect)
    {
        yield return new WaitForEndOfFrame();

        if (cardRect != null)
        {
            cardRect.sizeDelta = new Vector2(200, 300);
            cardRect.localScale = Vector3.one;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    // Keep the original method for backward compatibility
    public GameObject PerformCrafting(CardData card1, CardData card2, Transform parent)
    {
        CraftingRecipe recipe = FindMatchingRecipe(card1, card2);

        if (recipe != null)
        {
            Debug.Log($"Crafting successful: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

            GameObject newCard = CardManager.Instance.CreateCard(recipe.result, parent);
            SetupNewCard(newCard.GetComponent<RectTransform>());

            return newCard;
        }
        else
        {
            Debug.Log($"No recipe found: {card1.cardName} + {card2.cardName}");
            return null;
        }
    }

    private void SetupNewCard(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        StartCoroutine(EnsureNewCardSize(cardRT));
    }

    /// <summary>
    /// FIX: Public method to reset crafting state in case of deadlock
    /// </summary>
    public void ResetCraftingState()
    {
        isProcessingCrafting = false;
        Debug.Log("Crafting state has been reset");
    }
}
