using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("��������")]
    public SlotType slotType;
    public bool canAcceptAnyCard = false;
    public int maxCards = 1;

    public CardView CurrentCardView { get; set; } // ��Ϊpublic set������CraftingManager����

    // ��鿨���Ƿ�����
    public bool IsFull()
    {
        return CurrentCardView != null;
    }

    // ����Ƿ���Խ��ܿ���
    public virtual bool CanAcceptCard(CardData cardData)
    {
        // �����������������Ƿ��Ǹ��²۵�һ����
        CardArrangement parentArrangement = GetComponentInParent<CardArrangement>();
        if (parentArrangement != null && parentArrangement.isUpdateSlot)
        {
            Debug.Log($"���²۲��ܽ�����������");
            return false;
        }

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
                Debug.Log($"���Ʒ��õ� {slotType} ��λ");

                // ����������Ǻϳɲۣ�֪ͨ�ϳɹ��������ϳ�
                if (slotType == SlotType.Crafting && CraftingManager.Instance != null)
                {
                    CraftingManager.Instance.CheckForCrafting(this);
                }
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
            RemoveCard();
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

    // ǿ���Ƴ����ƣ�����ԭλ�ã�
    public virtual void ForceRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // �ص�ԭ����
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            RemoveCard();
        }
    }

    // ��������ȡ��������
    public CardData GetCardData()
    {
        return CurrentCardView?.GetCardData();
    }

    // ��������ղ�λ��������ԭλ�ã�
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
        Debug.Log($"���� {cardView.GetCardData().cardName} ���õ� {slotType} ��λ");
    }

    protected virtual void OnCardRemoved(CardView cardView)
    {
        Debug.Log($"���� {cardView.GetCardData().cardName} �� {slotType} ��λ�Ƴ�");
    }

    // ��CardSlot��������������
    public void SafeRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // �Ƴ����ӹ�ϵ����������
            if (CurrentCardView.transform.parent == transform)
            {
                CurrentCardView.transform.SetParent(null);
            }

            // ����CardDragHandler������
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null && dragHandler.OriginalParent == transform)
            {
                dragHandler.OriginalParent = null;
            }

            // �����Ƴ��¼��������ٿ���
            OnCardRemoved(CurrentCardView);
            CurrentCardView = null;
        }
    }
}
