using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : CardSlot
{
    [Header("装备槽设置")]
    public GameObject equipEffect;

    // 装备槽允许替换：新卡牌进入时，旧卡牌被替换
    public override void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();
            CardData cardData = cardView.GetCardData();

            if (CanAcceptCard(cardData))
            {
                // 如果槽位已有卡牌，先移除
                if (IsFull())
                {
                    ForceRemoveCard();
                }

                PlaceCard(draggedCard.transform, cardView);
                Debug.Log($"装备了 {cardData.cardName} 到 {slotType} 槽位");
            }
            else
            {
                Debug.Log($"无法装备 {cardData.cardName} 到 {slotType} 槽位");
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // 装备特效
        if (equipEffect != null)
        {
            Instantiate(equipEffect, transform.position, Quaternion.identity, transform);
        }

        // 应用装备效果
        ApplyEquipmentEffect(cardView.GetCardData());
    }

    protected override void OnCardRemoved(CardView cardView)
    {
        base.OnCardRemoved(cardView);

        // 移除装备效果
        RemoveEquipmentEffect(cardView.GetCardData());
    }

    private void ApplyEquipmentEffect(CardData cardData)
    {
        if (cardData is WeaponCardData weaponData)
        {
            Debug.Log($"装备武器: {weaponData.cardName}, 攻击力: {weaponData.attack}");
        }
        else if (cardData is ArmorCardData armorData)
        {
            Debug.Log($"装备盔甲: {armorData.cardName}, 防御力: {armorData.defense}");
        }
    }

    private void RemoveEquipmentEffect(CardData cardData)
    {
        Debug.Log($"移除装备: {cardData.cardName}");
    }
}