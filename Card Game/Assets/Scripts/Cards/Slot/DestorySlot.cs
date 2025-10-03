using UnityEngine;

public class DestroySlot : CardSlot
{
    [Header("销毁槽设置")]
    public GameObject destroyEffect;

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // 销毁特效
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // 销毁卡牌
        DestroyCard(cardView);
    }

    private void DestroyCard(CardView cardView)
    {
        Debug.Log($"销毁卡牌: {cardView.GetCardData().cardName}");
        Destroy(cardView.gameObject);
        CurrentCardView = null;
    }
}