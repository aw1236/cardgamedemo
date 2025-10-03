using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("��ק����")]
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

        // ��Ҫ������ӿ������ϳ���֪ͨ�����Ƴ�����
        CardSlot currentSlot = originalParent.GetComponent<CardSlot>();
        if (currentSlot != null)
        {
            Debug.Log($"�ӿ��� {currentSlot.slotType} ���ϳ�����");
            currentSlot.RemoveCard();  // �� ȷ������֪�����Ʊ�������
        }

        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);

        CurrentlyDraggedCard = this;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // ֱ�Ӹ������
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

        // ����Ƿ��ڿ���������
        bool shouldReturn = false;

        if (DropZoneManager.Instance != null)
        {
            if (!DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
            {
                // �����κο���������
                shouldReturn = true;
                Debug.Log("����û�з����ڿ��������ѷ���ԭλ��");
            }
        }

        // û�гɹ����õ����忨�ۣ����߲��ڿ���������
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

    //    // ��ʱע�͵������飬���κεط����ܷ���

    //    if (DropZoneManager.Instance != null && 
    //        !DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
    //    {
    //        ReturnToOriginalPosition();
    //        return;
    //    }


    //    // ���û�гɹ����õ����忨�ۣ��ص�ԭλ��
    //    if (transform.parent == canvas.transform)
    //    {
    //        ReturnToOriginalPosition();
    //    }

    //    CurrentlyDraggedCard = null;
    //}

    // �ص�ԭ��������������
    public void ReturnToOriginalContainer()
    {
        transform.SetParent(originalParent);

        // ���¼�������ϵͳ
        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
        else
        {
            // ���û������ϵͳ���ص�ԭλ��
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // ���õ���λ�ã����ں����Ŀ���ϵͳ��
    public void PlaceInNewSlot(Transform newParent, Vector2 newPosition)
    {
        transform.SetParent(newParent);
        rectTransform.anchoredPosition = newPosition;

        // ����ԭʼλ��
        originalParent = newParent;
        originalPosition = newPosition;

        // ����¸���������ϵͳ��������
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

        // ���¼�������ϵͳ
        CardArrangement arrangement = originalParent.GetComponent<CardArrangement>();
        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }
    }
}