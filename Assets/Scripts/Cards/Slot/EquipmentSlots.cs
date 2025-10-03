using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : CardSlot
{
    [Header("װ��������")]
    public GameObject equipEffect;

    // װ���������滻���¿��ƽ���ʱ���ɿ��Ʊ��滻
    public override void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();
            CardData cardData = cardView.GetCardData();

            if (CanAcceptCard(cardData))
            {
                // �����λ���п��ƣ����Ƴ�
                if (IsFull())
                {
                    ForceRemoveCard();
                }

                PlaceCard(draggedCard.transform, cardView);
                Debug.Log($"װ���� {cardData.cardName} �� {slotType} ��λ");
            }
            else
            {
                Debug.Log($"�޷�װ�� {cardData.cardName} �� {slotType} ��λ");
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // װ����Ч
        if (equipEffect != null)
        {
            Instantiate(equipEffect, transform.position, Quaternion.identity, transform);
        }

        // Ӧ��װ��Ч��
        ApplyEquipmentEffect(cardView.GetCardData());
    }

    protected override void OnCardRemoved(CardView cardView)
    {
        base.OnCardRemoved(cardView);

        // �Ƴ�װ��Ч��
        RemoveEquipmentEffect(cardView.GetCardData());
    }

    private void ApplyEquipmentEffect(CardData cardData)
    {
        if (cardData is WeaponCardData weaponData)
        {
            Debug.Log($"װ������: {weaponData.cardName}, ������: {weaponData.attack}");
        }
        else if (cardData is ArmorCardData armorData)
        {
            Debug.Log($"װ������: {armorData.cardName}, ������: {armorData.defense}");
        }
    }

    private void RemoveEquipmentEffect(CardData cardData)
    {
        Debug.Log($"�Ƴ�װ��: {cardData.cardName}");
    }
}