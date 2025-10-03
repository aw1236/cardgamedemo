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
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // 重要：如果从卡槽中拖出，通知卡槽移除引用
        CardSlot currentSlot = originalParent.GetComponent<CardSlot>();
        if (currentSlot != null)
        {
            Debug.Log($"从卡槽 {currentSlot.slotType} 中拖出卡牌");
            currentSlot.RemoveCard();  // ← 确保卡槽知道卡牌被移走了
        }

        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);

        CurrentlyDraggedCard = this;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // 直接跟随鼠标
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

        // 检查是否在卡槽区域内
        bool shouldReturn = false;

        if (DropZoneManager.Instance != null)
        {
            if (!DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
            {
                // 不在任何卡槽区域内
                shouldReturn = true;
                Debug.Log("卡牌没有放置在卡槽区域，已返回原位置");
            }
        }

        // 没有成功放置到具体卡槽，或者不在卡槽区域内
        if (shouldReturn || transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }

        CurrentlyDraggedCard = null;
    }
    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    canvasGroup.alpha = 1f;
    //    canvasGroup.blocksRaycasts = true;

    //    // 暂时注释掉区域检查，让任何地方都能放置

    //    if (DropZoneManager.Instance != null && 
    //        !DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
    //    {
    //        ReturnToOriginalPosition();
    //        return;
    //    }


    //    // 如果没有成功放置到具体卡槽，回到原位置
    //    if (transform.parent == canvas.transform)
    //    {
    //        ReturnToOriginalPosition();
    //    }

    //    CurrentlyDraggedCard = null;
    //}

    // 回到原容器并重新排列
    public void ReturnToOriginalContainer()
    {
        transform.SetParent(originalParent);

        // 重新加入排列系统
        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
        else
        {
            // 如果没有排列系统，回到原位置
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // 放置到新位置（用于后续的卡槽系统）
    public void PlaceInNewSlot(Transform newParent, Vector2 newPosition)
    {
        transform.SetParent(newParent);
        rectTransform.anchoredPosition = newPosition;

        // 更新原始位置
        originalParent = newParent;
        originalPosition = newPosition;

        // 如果新父级有排列系统，加入它
        CardArrangement newArrangement = newParent.GetComponent<CardArrangement>();
        if (newArrangement != null)
        {
            newArrangement.AddCard(rectTransform);
        }
    }
    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;

        // 重新加入排列系统
        CardArrangement arrangement = originalParent.GetComponent<CardArrangement>();
        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
    }
}