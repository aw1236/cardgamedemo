
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("合成配方")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    // 合成槽引用
    [Header("合成槽设置")]
    public CardSlot craftingSlot1;
    public CardSlot craftingSlot2;

    // 新增：防止重复合成的标记
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
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.Matches(card1, card2))
            {
                return recipe;
            }
        }
        return null;
    }

    // 新增：检查并执行合成
    public void CheckForCrafting(CardSlot triggeredSlot)
    {
        if (isProcessingCrafting) return;

        // 确保两个槽都有卡牌
        if (craftingSlot1 == null || craftingSlot2 == null ||
            craftingSlot1.GetCardData() == null || craftingSlot2.GetCardData() == null)
        {
            return;
        }

        StartCoroutine(ProcessCraftingCheck(triggeredSlot));
    }

    private IEnumerator ProcessCraftingCheck(CardSlot triggeredSlot)
    {
        isProcessingCrafting = true;

        // 等待一帧，确保所有拖拽操作完成
        yield return null;

        CardData card1 = craftingSlot1.GetCardData();
        CardData card2 = craftingSlot2.GetCardData();

        if (card1 == null || card2 == null)
        {
            isProcessingCrafting = false;
            yield break;
        }

        CraftingRecipe recipe = FindMatchingRecipe(card1, card2);

        if (recipe != null)
        {
            Debug.Log($"即时合成: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

            // 执行合成
            yield return StartCoroutine(PerformInstantCrafting(recipe));
        }
        else
        {
            Debug.Log($"没有匹配配方: {card1.cardName} + {card2.cardName}");

            // 不匹配，弹出第二张卡牌
            if (triggeredSlot != null)
            {
                triggeredSlot.ForceRemoveCard();
            }
            else
            {
                // 如果不知道是哪个槽触发的，弹出最后放入的卡牌
                craftingSlot2.ForceRemoveCard();
            }
        }

        isProcessingCrafting = false;
    }

    // 修改：即时合成，产物出现在第一个格子
    private IEnumerator PerformInstantCrafting(CraftingRecipe recipe)
    {
        // 记录第一个合成槽的位置
        Transform firstSlotTransform = craftingSlot1.transform;

        // 消耗两张卡牌
        craftingSlot1.ClearSlot();
        craftingSlot2.ClearSlot();

        // 等待一帧，确保卡牌被清除
        yield return null;

        // 在第一个合成槽创建新卡牌
        GameObject newCard = CardManager.Instance.CreateCard(recipe.result, firstSlotTransform);

        // 设置卡牌位置和属性
        SetupNewCardInSlot(newCard.GetComponent<RectTransform>());

        // 将新卡牌设置为第一个合成槽的当前卡牌
        CardView newCardView = newCard.GetComponent<CardView>();
        if (newCardView != null)
        {
            craftingSlot1.CurrentCardView = newCardView;
        }

        // 播放合成特效
        yield return StartCoroutine(PlayCraftingEffects(newCard));
    }

    // 新增：在槽位中设置新卡牌
    private void SetupNewCardInSlot(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        // 确保尺寸正确
        StartCoroutine(EnsureNewCardSize(cardRT));
    }

    // 新增：合成特效（可选）
    private IEnumerator PlayCraftingEffects(GameObject newCard)
    {
        // 这里可以添加合成特效，比如缩放动画
        if (newCard != null)
        {
            RectTransform cardRT = newCard.GetComponent<RectTransform>();
            Vector3 originalScale = cardRT.localScale;

            // 从小变大的动画
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

    // 保留原有的方法用于其他用途
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

        // 确保尺寸正确
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
