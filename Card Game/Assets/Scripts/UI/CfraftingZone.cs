using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CraftingZone : MonoBehaviour, IDropHandler
{
    [Header("合成区域")]
    public bool allowAnyCardCrafting = true;

    private CardView firstCard = null;
    private CardView secondCard = null;

    public void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();

            if (firstCard == null)
            {
                // 第一张卡牌 - 放在左侧
                firstCard = cardView;
                PlaceCardInZone(draggedCard.transform, new Vector2(-100, 0));
                Debug.Log($"第一张卡牌放置: {firstCard.GetCardData().cardName}");
            }
            else if (secondCard == null)
            {
                // 第二张卡牌 - 放在右侧
                secondCard = cardView;
                PlaceCardInZone(draggedCard.transform, new Vector2(100, 0));
                Debug.Log($"第二张卡牌放置: {secondCard.GetCardData().cardName}");

                // 两张卡牌都放置了，尝试合成
                StartCoroutine(AttemptCraftingAfterDelay());
            }
            else
            {
                // 已经有两张卡牌，拒绝新的放置
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    private IEnumerator AttemptCraftingAfterDelay()
    {
        // 等待一帧让第二张卡牌完全放置
        yield return new WaitForEndOfFrame();

        if (firstCard == null || secondCard == null) yield break;

        CardData card1Data = firstCard.GetCardData();
        CardData card2Data = secondCard.GetCardData();

        // 修改：获取合成结果，在本地处理卡牌销毁
        GameObject newCard = CraftingManager.Instance.PerformCrafting(card1Data, card2Data, transform);

        if (newCard != null)
        {
            // 合成成功，销毁原卡牌
            Destroy(firstCard.gameObject);
            Destroy(secondCard.gameObject);

            // 重置引用
            firstCard = null;
            secondCard = null;

            Debug.Log("合成成功！原卡牌已销毁，新卡牌已生成");
        }
        else
        {
            // 合成失败，第二张卡牌回到原位置
            CardDragHandler dragHandler = secondCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            secondCard = null;
            Debug.Log("合成失败，第二张卡牌已返回");
        }
    }

    private void PlaceCardInZone(Transform cardTransform, Vector2 position)
    {
        RectTransform cardRect = cardTransform as RectTransform;
        Vector2 originalSize = cardRect.sizeDelta;

        cardTransform.SetParent(transform);
        cardTransform.localPosition = position;
        cardTransform.localScale = Vector3.one;

        // 强制保持尺寸
        if (cardRect != null)
        {
            cardRect.sizeDelta = originalSize;
        }

        StartCoroutine(EnsureCardSize(cardRect, originalSize));
    }

    public void ClearCraftingZone()
    {
        if (firstCard != null)
        {
            CardDragHandler dragHandler = firstCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            firstCard = null;
        }
        if (secondCard != null)
        {
            CardDragHandler dragHandler = secondCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            secondCard = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 右键清空
        {
            ClearCraftingZone();
        }
    }

    private IEnumerator EnsureCardSize(RectTransform cardRect, Vector2 targetSize)
    {
        yield return new WaitForEndOfFrame();
        if (cardRect != null)
        {
            cardRect.sizeDelta = targetSize;
            cardRect.localScale = Vector3.one;
        }
    }
}