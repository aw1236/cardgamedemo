using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("所有合成配方")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

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
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.Matches(card1, card2))
            {
                return recipe;
            }
        }
        return null;
    }

    // 修改：只返回合成结果，不处理卡牌销毁
    public GameObject PerformCrafting(CardData card1, CardData card2, Transform parent)
    {
        CraftingRecipe recipe = FindMatchingRecipe(card1, card2);

        if (recipe != null)
        {
            Debug.Log($"合成成功: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

            GameObject newCard = CardManager.Instance.CreateCard(recipe.result, parent);
            SetupNewCard(newCard.GetComponent<RectTransform>());

            return newCard;
        }
        else
        {
            Debug.Log($"没有找到配方: {card1.cardName} + {card2.cardName}");
            return null;
        }
    }

    private void SetupNewCard(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        // 延迟确保尺寸
        StartCoroutine(EnsureNewCardSize(cardRT));
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
}