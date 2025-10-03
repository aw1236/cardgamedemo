using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("��������")]
    public SlotType slotType;
    public bool canAcceptAnyCard = false;
    public int maxCards = 1;  // �� ���������������

    public CardView CurrentCardView { get; protected set; }

    // ��鿨���Ƿ�����
    public bool IsFull()
    {
        return CurrentCardView != null;
    }

    // ����Ƿ���Խ��ܿ���
    public virtual bool CanAcceptCard(CardData cardData)
    {
        // ��鿨���Ƿ�����
        if (IsFull())
        {
            Debug.Log($"{slotType}���������޷����ø��࿨��");
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
                Debug.Log($"���Ʒ��õ� {slotType} ��λ");
            }
            else
            {
                Debug.Log($"�޷����ÿ��Ƶ� {slotType} ��λ");
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    public virtual void PlaceCard(Transform cardTransform, CardView cardView)
    {
        // �Ƴ���ǰ���ƣ�����У�
        if (CurrentCardView != null)
        {
            ForceRemoveCard();
        }

        RectTransform cardRect = cardTransform as RectTransform;

        
       
        // ��¼ԭʼ�ߴ�
        Vector2 originalSize = cardRect.sizeDelta;

        // �����¿���
        cardTransform.SetParent(transform);

       

        cardTransform.localPosition = Vector3.zero;
        cardTransform.localScale = Vector3.one;

      

        // ǿ�����óߴ�
        cardRect.sizeDelta = originalSize;
       
        CurrentCardView = cardView;

        // �������ú��Ч��
        OnCardPlaced(cardView);

        // �ӳټ�����ճߴ�
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
            CurrentCardView = null;  // �� ȷ��������ȷ�������
        }
    }

    // ͬʱ�޸� ForceRemoveCard ����
    public virtual void ForceRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // �ص�ԭ����������
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            RemoveCard();  // �� ����������� RemoveCard ����
        }
    }

    protected virtual void OnCardPlaced(CardView cardView)
    {
        Debug.Log($"���� {cardView.GetCardData().cardName} ���õ� {slotType} ��λ");
    }

    protected virtual void OnCardRemoved(CardView cardView)
    {
        Debug.Log($"���� {cardView.GetCardData().cardName} �� {slotType} ��λ�Ƴ�");
    }

    
   
}