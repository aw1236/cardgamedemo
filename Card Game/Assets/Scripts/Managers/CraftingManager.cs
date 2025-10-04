
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("�ϳ��䷽")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

    // �ϳɲ�����
    [Header("�ϳɲ�����")]
    public CardSlot craftingSlot1;
    public CardSlot craftingSlot2;

    // ��������ֹ�ظ��ϳɵı��
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

    // ��������鲢ִ�кϳ�
    public void CheckForCrafting(CardSlot triggeredSlot)
    {
        if (isProcessingCrafting) return;

        // ȷ�������۶��п���
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

        // �ȴ�һ֡��ȷ��������ק�������
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
            Debug.Log($"��ʱ�ϳ�: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

            // ִ�кϳ�
            yield return StartCoroutine(PerformInstantCrafting(recipe));
        }
        else
        {
            Debug.Log($"û��ƥ���䷽: {card1.cardName} + {card2.cardName}");

            // ��ƥ�䣬�����ڶ��ſ���
            if (triggeredSlot != null)
            {
                triggeredSlot.ForceRemoveCard();
            }
            else
            {
                // �����֪�����ĸ��۴����ģ�����������Ŀ���
                craftingSlot2.ForceRemoveCard();
            }
        }

        isProcessingCrafting = false;
    }

    // �޸ģ���ʱ�ϳɣ���������ڵ�һ������
    private IEnumerator PerformInstantCrafting(CraftingRecipe recipe)
    {
        // ��¼��һ���ϳɲ۵�λ��
        Transform firstSlotTransform = craftingSlot1.transform;

        // �������ſ���
        craftingSlot1.ClearSlot();
        craftingSlot2.ClearSlot();

        // �ȴ�һ֡��ȷ�����Ʊ����
        yield return null;

        // �ڵ�һ���ϳɲ۴����¿���
        GameObject newCard = CardManager.Instance.CreateCard(recipe.result, firstSlotTransform);

        // ���ÿ���λ�ú�����
        SetupNewCardInSlot(newCard.GetComponent<RectTransform>());

        // ���¿�������Ϊ��һ���ϳɲ۵ĵ�ǰ����
        CardView newCardView = newCard.GetComponent<CardView>();
        if (newCardView != null)
        {
            craftingSlot1.CurrentCardView = newCardView;
        }

        // ���źϳ���Ч
        yield return StartCoroutine(PlayCraftingEffects(newCard));
    }

    // �������ڲ�λ�������¿���
    private void SetupNewCardInSlot(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        // ȷ���ߴ���ȷ
        StartCoroutine(EnsureNewCardSize(cardRT));
    }

    // �������ϳ���Ч����ѡ��
    private IEnumerator PlayCraftingEffects(GameObject newCard)
    {
        // ���������Ӻϳ���Ч���������Ŷ���
        if (newCard != null)
        {
            RectTransform cardRT = newCard.GetComponent<RectTransform>();
            Vector3 originalScale = cardRT.localScale;

            // ��С���Ķ���
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

    // ����ԭ�еķ�������������;
    public GameObject PerformCrafting(CardData card1, CardData card2, Transform parent)
    {
        CraftingRecipe recipe = FindMatchingRecipe(card1, card2);

        if (recipe != null)
        {
            Debug.Log($"�ϳɳɹ�: {card1.cardName} + {card2.cardName} = {recipe.result.cardName}");

            GameObject newCard = CardManager.Instance.CreateCard(recipe.result, parent);
            SetupNewCard(newCard.GetComponent<RectTransform>());

            return newCard;
        }
        else
        {
            Debug.Log($"û���ҵ��䷽: {card1.cardName} + {card2.cardName}");
            return null;
        }
    }

    private void SetupNewCard(RectTransform cardRT)
    {
        if (cardRT == null) return;

        cardRT.anchoredPosition = Vector2.zero;
        cardRT.localScale = Vector3.one;
        cardRT.sizeDelta = new Vector2(200, 300);

        // ȷ���ߴ���ȷ
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
