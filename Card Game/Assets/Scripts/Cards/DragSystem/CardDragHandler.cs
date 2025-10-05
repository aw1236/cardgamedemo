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

    // ����������OriginalParent���ԣ�����CardSlot����
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
        // ����Ƿ�Ӹ��²��ϳ���������������ϳ������������������Ƶ����²�
        CardArrangement sourceArrangement = GetComponentInParent<CardArrangement>();
        if (sourceArrangement != null && sourceArrangement.isUpdateSlot)
        {
            Debug.Log("�Ӹ��²��ϳ�����");
        }

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        OriginalParent = originalParent; // ���ù�������

        CardSlot currentSlot = originalParent.GetComponent<CardSlot>();
        if (currentSlot != null)
        {
            Debug.Log($"�ӿ��� {currentSlot.slotType} ���ϳ�����");
            // �޸���ʹ��SafeRemoveCard������RemoveCard�����⸸�ӹ�ϵ����
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

        // ������Ŀ��
        CardSlot targetSlot = null;
        CardArrangement targetArrangement = null;

        // ����Ƿ�������˿�����
        if (eventData.pointerEnter != null)
        {
            targetSlot = eventData.pointerEnter.GetComponent<CardSlot>();
            targetArrangement = eventData.pointerEnter.GetComponent<CardArrangement>();

            // ���Ŀ���и�����Ҳ��鸸��������Ƕ�������
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

        // ��Ҫ�����Ŀ���Ǹ��²ۣ��ܾ����ò�����
        if (targetArrangement != null && targetArrangement.isUpdateSlot)
        {
            Debug.Log("�޷������Ʒ��õ����²���");
            shouldReturn = true;
        }
        // ���Ŀ�꿨���Ǹ��²����ͣ�Ҳ�ܾ�
        else if (targetSlot != null)
        {
            // ���Ŀ�꿨���Ƿ��Ǹ��²۵�һ����
            CardArrangement slotArrangement = targetSlot.GetComponentInParent<CardArrangement>();
            if (slotArrangement != null && slotArrangement.isUpdateSlot)
            {
                Debug.Log("�޷������Ʒ��õ����²۵Ŀ�����");
                shouldReturn = true;
            }
        }
        // ����Ƿ��ڿ��������ڵ�û����ЧĿ��
        else if (DropZoneManager.Instance != null && !DropZoneManager.Instance.IsInAnySlotZone(eventData.position))
        {
            shouldReturn = true;
            Debug.Log("����û�з����ڿ��������ѷ���ԭλ��");
        }
        // û�гɹ����õ����忨��
        else if (transform.parent == canvas.transform && targetSlot == null)
        {
            shouldReturn = true;
        }

        // �޸���ȷ��������������ȷ�ĸ���
        if (shouldReturn)
        {
            ReturnToOriginalPosition();
        }
        else if (targetSlot == null)
        {
            // ���û��Ŀ�꿨�۵�Ҳ��Ӧ�÷��أ�ȷ�������и���
            ReturnToOriginalPosition();
        }

        CurrentlyDraggedCard = null;
    }

    public void ReturnToOriginalContainer()
    {
        // �޸������ԭʼ�����Ƿ���Ȼ��Ч
        if (originalParent == null)
        {
            Debug.LogWarning("ԭʼ���������٣��޷���������");
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

        // �޸���֪ͨԭʼ��������ӵ�����ſ���
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
        OriginalParent = newParent; // ���¹�������
        originalPosition = newPosition;

        CardArrangement newArrangement = newParent.GetComponent<CardArrangement>();
        if (newArrangement != null)
        {
            newArrangement.AddCard(rectTransform);
        }
    }

    public void ReturnToOriginalPosition()
    {
        // �޸�����Ӱ�ȫ���
        if (originalParent == null)
        {
            Debug.LogWarning("ԭʼ���������٣��޷�����ԭλ��");
            // ���û��ԭʼ�����������ҵ����ʵĸ���������
            if (canvas != null)
            {
                transform.SetParent(canvas.transform);
            }
            return;
        }

        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;

        if (arrangement != null)
        {
            arrangement.AddCard(rectTransform);
        }

        // �޸���ȷ��ԭʼ����֪�������ѷ���
        CardSlot originalSlot = originalParent.GetComponent<CardSlot>();
        if (originalSlot != null)
        {
            CardView cardView = GetComponent<CardView>();
            if (cardView != null && originalSlot.CurrentCardView == null)
            {
                originalSlot.CurrentCardView = cardView;
            }
        }
    }

    // ������ǿ�Ʒ���ԭλ�ã������쳣�����
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