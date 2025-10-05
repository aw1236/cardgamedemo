using UnityEngine;

public class DestroySlot : CardSlot
{
    [Header("销毁特效")]
    public GameObject destroyEffect;

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // 获取卡牌数据
        CardData cardData = cardView.GetCardData();

        // 检查卡牌类型，只允许销毁Material、Food、Weapon和Armor类型的卡牌
        if (cardData.cardType == CardType.Material ||
            cardData.cardType == CardType.Food ||
            cardData.cardType == CardType.Weapon ||
            cardData.cardType == CardType.Armor)
        {
            // 播放销毁特效
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            // 销毁卡牌
            DestroyCard(cardView);
        }
        else
        {
            // 对于不允许销毁的卡牌类型，输出日志并弹回原位
            Debug.Log($"该类型卡牌不能被销毁: {cardData.cardName} (类型: {cardData.cardType})");

            // 使用CardDragHandler的ReturnToOriginalPosition方法弹回卡牌
            CardDragHandler dragHandler = cardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            else
            {
                // 备用方案：如果找不到拖拽组件，使用基类的ForceRemoveCard
                ForceRemoveCard();
            }

            CurrentCardView = null; // 清空当前卡牌引用
        }
    }

    private void DestroyCard(CardView cardView)
    {
        Debug.Log($"销毁卡牌: {cardView.GetCardData().cardName}");
        Destroy(cardView.gameObject);
        CurrentCardView = null;
    }
}