using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("拖拽设置")]
    public float dragAlpha = 0.7f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private CardArrangement arrangement;

    // 新增：公开OriginalParent属性，方便CardSlot访问
    public Transform OriginalParent { get; set; }

    public static CardDragHandler CurrentlyDraggedCard { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        arrangement = GetComponentInParent<CardArrangement>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 检查是否从更新槽拖出，如果是则允许拖出但不能拖入其他卡牌到更新槽
        CardArrangement sourceArrangement = GetComponentInParent<CardArrangement>();
        if (sourceArrangement != null && sourceArrangement.isUpdateSlot)
        {
            Debug.Log("从更新槽拖出卡牌");
        }

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        OriginalParent = originalParent; // 设置公开属性

        CardSlot currentSlot = originalParent.GetComponent<CardSlot>();
        if (currentSlot != null)
        {
            Debug.Log($"从卡槽 {currentSlot.slotType} 中拖出卡牌");
            // 修复：使用SafeRemoveCard而不是RemoveCard，避免父子关系混乱
            currentSlot.SafeRemoveCard();
        }

        if (arrangement != null)
        {
            arrangement.MarkCardForRemoval(rectTransform);
        }

        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        CurrentlyDraggedCard = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null,
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 新增：在拖拽结束时强制重置所有可能相关的卡槽状态
        CardSlot originalSlot = originalParent?.GetComponent<CardSlot>();
        if (originalSlot != null)
        {
            // 调用新增的ForceResetSlot方法
            originalSlot.ForceResetSlot();
        }

        // 检查放置目标
        CardSlot targetSlot = null;
        CardArrangement targetArrangement = null;

        // 检查是否放置在了卡槽上
        if (eventData.pointerEnter != null)
        {
            targetSlot = eventData.pointerEnter.GetComponent<CardSlot>();
            targetArrangement = eventData.pointerEnter.GetComponent<CardArrangement>();

            // 如果目标有父级，也检查父级（处理嵌套情况）
            if (targetSlot == null && eventData.pointerEnter.transform.parent != null)
            {
                targetSlot = eventData.pointerEnter.transform.parent.GetComponent<CardSlot>();
            }
            if (targetArrangement == null && eventData.pointerEnter.transform.parent != null)
            {
                targetArrangement = eventData.pointerEnter.transform.parent.GetComponent<CardArrangement>();
            }
        }

        bool shouldReturn = false;

        // 重要：如果目标是更新槽，拒绝放置并弹回
        if (targetArrangement != null && targetArrangement.isUpdateSlot)
        {
            Debug.Log("无法将卡牌放置到更新槽中");
            shouldReturn = true;
        }
        // 如果目标卡槽是更新槽类型，也拒绝
        else if (targetSlot != null)
        {
            // 检查目标卡槽是否是更新槽的一部分
            CardArrangement slotArrangement = targetSlot.GetComponentInParent<CardArrangement>();
            if (slotArrangement != null && slotArrangement.isUpdateSlot)
            {
                Debug.Log("无法将卡牌放置到更新槽的卡槽中");
                shouldReturn = true;
            }
        }
        // 检查是否在卡槽区域内但没有有效目标
        else if (DropZoneManager.Instance != null && !DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
        {
            shouldReturn = true;
            Debug.Log("卡牌没有放置在卡槽区域，已返回原位置");
        }
        // 没有成功放置到具体卡槽
        else if (transform.parent == canvas.transform && targetSlot == null)
        {
            shouldReturn = true;
        }

        // 修复：确保卡牌最终有正确的父级
        if (shouldReturn)
        {
            ReturnToOriginalPosition();
        }
        else if (targetSlot == null)
        {
            // 如果没有目标卡槽但也不应该返回，确保卡牌有父级
            ReturnToOriginalPosition();
        }

        CurrentlyDraggedCard = null;
    }

    public void ReturnToOriginalContainer()
    {
        // 修复：检查原始父级是否仍然有效
        if (originalParent == null)
        {
            Debug.LogWarning("原始父级已销毁，无法返回容器");
            return;
        }

        transform.SetParent(originalParent);
        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        // 修复：通知原始卡槽重新拥有这张卡牌
        CardSlot originalSlot = originalParent.GetComponent<CardSlot>();
        if (originalSlot != null)
        {
            CardView cardView = GetComponent<CardView>();
            if (cardView != null)
            {
                originalSlot.CurrentCardView = cardView;
            }
        }
    }

    public void PlaceInNewSlot(Transform newParent, Vector2 newPosition)
    {
        if (arrangement != null && newParent != arrangement.transform)
        {
            arrangement.RemoveCard(rectTransform);
        }

        transform.SetParent(newParent);
        rectTransform.anchoredPosition = newPosition;
        originalParent = newParent;
        OriginalParent = newParent; // 更新公开属性
        originalPosition = newPosition;

        CardArrangement newArrangement = newParent.GetComponent<CardArrangement>();
        if (newArrangement != null)
        {
            newArrangement.AddCard(rectTransform);
        }
    }

    public void ReturnToOriginalPosition()
    {
        // 修复：添加安全检查
        if (originalParent == null)
        {
            Debug.LogWarning("原始父级已销毁，无法返回原位置");
            // 如果没有原始父级，尝试找到合适的父级或销毁
            if (canvas != null)
            {
                transform.SetParent(canvas.transform);
            }
            return;
        }

        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;

        // 新增：返回时强制重置原卡槽状态
        CardSlot originalSlot = originalParent.GetComponent<CardSlot>();
        if (originalSlot != null)
        {
            // 调用新增的ForceResetSlot方法
            originalSlot.ForceResetSlot();

            CardView cardView = GetComponent<CardView>();
            if (cardView != null && originalSlot.CurrentCardView == null)
            {
                originalSlot.CurrentCardView = cardView;

                // 🎯 新增：如果是装备槽，重新应用装备效果
                EquipmentSlot equipmentSlot = originalSlot as EquipmentSlot;
                if (equipmentSlot != null)
                {
                    CardData cardData = cardView.GetCardData();
                    if (cardData != null)
                    {
                        equipmentSlot.ApplyEquipmentToMainCharacter(cardData);
                        Debug.Log($"装备卡牌返回槽位，重新应用效果: {cardData.cardName}");
                    }
                }
            }
        }

        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
    }

    // 新增：强制返回原位置（用于异常情况）
    public void ForceReturnToOriginal()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;

            CardSlot originalSlot = originalParent.GetComponent<CardSlot>();
            if (originalSlot != null)
            {
                CardView cardView = GetComponent<CardView>();
                if (cardView != null)
                {
                    originalSlot.CurrentCardView = cardView;
                }
            }
        }
    }
}