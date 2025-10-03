using UnityEngine;

public class DestroySlot : CardSlot
{
    [Header("���ٲ�����")]
    public GameObject destroyEffect;

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // ������Ч
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // ���ٿ���
        DestroyCard(cardView);
    }

    private void DestroyCard(CardView cardView)
    {
        Debug.Log($"���ٿ���: {cardView.GetCardData().cardName}");
        Destroy(cardView.gameObject);
        CurrentCardView = null;
    }
}