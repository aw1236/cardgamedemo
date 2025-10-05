using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡槽设置")]
    public SlotType slotType;
    public bool canAcceptAnyCard = false;
    public int maxCards = 1;

    public CardView CurrentCardView { get; set; } // 改为public set，方便CraftingManager设置

    // 检查卡槽是否已满
    public bool IsFull()
    {
        return CurrentCardView != null;
    }

    // 检查是否可以接受卡牌
    public virtual bool CanAcceptCard(CardData cardData)
    {
        // 新增：检查这个卡槽是否是更新槽的一部分
        CardArrangement parentArrangement = GetComponentInParent<CardArrangement>();
        if (parentArrangement != null && parentArrangement.isUpdateSlot)
        {
            Debug.Log($"更新槽不能接受外来卡牌");
            return false;
        }

        // 检查卡槽是否已满
        if (IsFull())
        {
            Debug.Log($"{slotType}槽已满，无法放置更多卡牌");
            return false;
        }

        if (canAcceptAnyCard) return true;

        switch (slotType)
        {
            case SlotType.Weapon:
                return cardData.cardType == CardType.Weapon;
            case SlotType.Armor:
                return cardData.cardType == CardType.Armor;
            case SlotType.MainCharacter:
                return cardData.cardType == CardType.MainCharacter;
            case SlotType.Backpack:
                return cardData.cardType != CardType.Monster &&
                       cardData.cardType != CardType.MainCharacter;
            case SlotType.Destroy:
                return true;
            case SlotType.Refresh:
                return true;
            case SlotType.Crafting:
                return true;
            default:
                return false;
        }
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();
            CardData cardData = cardView.GetCardData();

            if (CanAcceptCard(cardData))
            {
                PlaceCard(draggedCard.transform, cardView);
                Debug.Log($"卡牌放置到 {slotType} 槽位");

                // 新增：如果是合成槽，通知合成管理器检查合成
                if (slotType == SlotType.Crafting && CraftingManager.Instance != null)
                {
                    CraftingManager.Instance.CheckForCrafting(this);
                }
            }
            else
            {
                Debug.Log($"无法放置卡牌到 {slotType} 槽位");
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    public virtual void PlaceCard(Transform cardTransform, CardView cardView)
    {
        // 移除当前卡牌（如果有）
        if (CurrentCardView != null)
        {
            RemoveCard();
        }

        RectTransform cardRect = cardTransform as RectTransform;

        // 记录原始尺寸
        Vector2 originalSize = cardRect.sizeDelta;

        // 放置新卡牌
        cardTransform.SetParent(transform);

        cardTransform.localPosition = Vector3.zero;
        cardTransform.localScale = Vector3.one;

        // 强制设置尺寸
        cardRect.sizeDelta = originalSize;

        CurrentCardView = cardView;

        // 触发放置后的效果
        OnCardPlaced(cardView);

        // 延迟检查最终尺寸
        StartCoroutine(CheckFinalSize(cardRect));
    }

    private IEnumerator CheckFinalSize(RectTransform cardRect)
    {
        yield return new WaitForEndOfFrame();
    }

    public virtual void RemoveCard()
    {
        if (CurrentCardView != null)
        {
            OnCardRemoved(CurrentCardView);
            CurrentCardView = null;
        }
    }

    // 强制移除卡牌（返回原位置）
    public virtual void ForceRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // 回到原容器
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            RemoveCard();
        }
    }

    // 新增：获取卡牌数据
    public CardData GetCardData()
    {
        return CurrentCardView?.GetCardData();
    }

    // 新增：清空槽位（不返回原位置）
    public void ClearSlot()
    {
        if (CurrentCardView != null)
        {
            Destroy(CurrentCardView.gameObject);
            CurrentCardView = null;
        }
    }

    protected virtual void OnCardPlaced(CardView cardView)
    {
        Debug.Log($"卡牌 {cardView.GetCardData().cardName} 放置到 {slotType} 槽位");
    }

    protected virtual void OnCardRemoved(CardView cardView)
    {
        Debug.Log($"卡牌 {cardView.GetCardData().cardName} 从 {slotType} 槽位移除");
    }

    // 在CardSlot类中添加这个方法
    public void SafeRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // 移除父子关系但保持引用
            if (CurrentCardView.transform.parent == transform)
            {
                CurrentCardView.transform.SetParent(null);
            }

            // 清理CardDragHandler的引用
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null && dragHandler.OriginalParent == transform)
            {
                dragHandler.OriginalParent = null;
            }

            // 触发移除事件但不销毁卡牌
            OnCardRemoved(CurrentCardView);
            CurrentCardView = null;
        }
    }
}
