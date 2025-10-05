using UnityEngine;

public class DestroySlot : CardSlot
{
    [Header("������Ч")]
    public GameObject destroyEffect;

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // ��ȡ��������
        CardData cardData = cardView.GetCardData();

        // ��鿨�����ͣ�ֻ��������Material��Food��Weapon��Armor���͵Ŀ���
        if (cardData.cardType == CardType.Material ||
            cardData.cardType == CardType.Food ||
            cardData.cardType == CardType.Weapon ||
            cardData.cardType == CardType.Armor)
        {
            // ����������Ч
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            // ���ٿ���
            DestroyCard(cardView);
        }
        else
        {
            // ���ڲ��������ٵĿ������ͣ������־������ԭλ
            Debug.Log($"�����Ϳ��Ʋ��ܱ�����: {cardData.cardName} (����: {cardData.cardType})");

            // ʹ��CardDragHandler��ReturnToOriginalPosition�������ؿ���
            CardDragHandler dragHandler = cardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            else
            {
                // ���÷���������Ҳ�����ק�����ʹ�û����ForceRemoveCard
                ForceRemoveCard();
            }

            CurrentCardView = null; // ��յ�ǰ��������
        }
    }

    private void DestroyCard(CardView cardView)
    {
        Debug.Log($"���ٿ���: {cardView.GetCardData().cardName}");
        Destroy(cardView.gameObject);
        CurrentCardView = null;
    }
}