using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡槽设置")]
    public SlotType slotType;
    public bool canAcceptAnyCard = false;
    public int maxCards = 1;  // ← 新增：最大卡牌数量

    public CardView CurrentCardView { get; protected set; }

    // 检查卡槽是否已满
    public bool IsFull()
    {
        return CurrentCardView != null;
    }

    // 检查是否可以接受卡牌
    public virtual bool CanAcceptCard(CardData cardData)
    {
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
                return cardData is WeaponCardData;
            case SlotType.Armor:
                return cardData is ArmorCardData;
            case SlotType.MainCharacter:
                return cardData.cardType == CardType.MainCharacter;
            case SlotType.Backpack:
                return cardData.cardType != CardType.Monster &&
                       cardData.cardType != CardType.MainCharacter;
            case SlotType.Destroy:
                return true;
            case SlotType.Refresh:
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
            ForceRemoveCard();
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

    private System.Collections.IEnumerator CheckFinalSize(RectTransform cardRect)
    {
        yield return new WaitForEndOfFrame();
        
    }

    public virtual void RemoveCard()
    {
        if (CurrentCardView != null)
        {
            OnCardRemoved(CurrentCardView);
            CurrentCardView = null;  // ← 确保这里正确清除引用
        }
    }

    // 同时修改 ForceRemoveCard 方法
    public virtual void ForceRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // 回到原容器或销毁
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            RemoveCard();  // ← 这会调用上面的 RemoveCard 方法
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

    
   
}