using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CraftingZone : MonoBehaviour, IDropHandler
{
    [Header("�ϳ�����")]
    public bool allowAnyCardCrafting = true;

    private CardView firstCard = null;
    private CardView secondCard = null;

    public void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();

            if (firstCard == null)
            {
                // ��һ�ſ��� - �������
                firstCard = cardView;
                PlaceCardInZone(draggedCard.transform, new Vector2(-100, 0));
                Debug.Log($"��һ�ſ��Ʒ���: {firstCard.GetCardData().cardName}");
            }
            else if (secondCard == null)
            {
                // �ڶ��ſ��� - �����Ҳ�
                secondCard = cardView;
                PlaceCardInZone(draggedCard.transform, new Vector2(100, 0));
                Debug.Log($"�ڶ��ſ��Ʒ���: {secondCard.GetCardData().cardName}");

                // ���ſ��ƶ������ˣ����Ժϳ�
                StartCoroutine(AttemptCraftingAfterDelay());
            }
            else
            {
                // �Ѿ������ſ��ƣ��ܾ��µķ���
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    private IEnumerator AttemptCraftingAfterDelay()
    {
        // �ȴ�һ֡�õڶ��ſ�����ȫ����
        yield return new WaitForEndOfFrame();

        if (firstCard == null || secondCard == null) yield break;

        CardData card1Data = firstCard.GetCardData();
        CardData card2Data = secondCard.GetCardData();

        // �޸ģ���ȡ�ϳɽ�����ڱ��ش���������
        GameObject newCard = CraftingManager.Instance.PerformCrafting(card1Data, card2Data, transform);

        if (newCard != null)
        {
            // �ϳɳɹ�������ԭ����
            Destroy(firstCard.gameObject);
            Destroy(secondCard.gameObject);

            // ��������
            firstCard = null;
            secondCard = null;

            Debug.Log("�ϳɳɹ���ԭ���������٣��¿���������");
        }
        else
        {
            // �ϳ�ʧ�ܣ��ڶ��ſ��ƻص�ԭλ��
            CardDragHandler dragHandler = secondCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            secondCard = null;
            Debug.Log("�ϳ�ʧ�ܣ��ڶ��ſ����ѷ���");
        }
    }

    private void PlaceCardInZone(Transform cardTransform, Vector2 position)
    {
        RectTransform cardRect = cardTransform as RectTransform;
        Vector2 originalSize = cardRect.sizeDelta;

        cardTransform.SetParent(transform);
        cardTransform.localPosition = position;
        cardTransform.localScale = Vector3.one;

        // ǿ�Ʊ��ֳߴ�
        if (cardRect != null)
        {
            cardRect.sizeDelta = originalSize;
        }

        StartCoroutine(EnsureCardSize(cardRect, originalSize));
    }

    public void ClearCraftingZone()
    {
        if (firstCard != null)
        {
            CardDragHandler dragHandler = firstCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            firstCard = null;
        }
        if (secondCard != null)
        {
            CardDragHandler dragHandler = secondCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            secondCard = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) // �Ҽ����
        {
            ClearCraftingZone();
        }
    }

    private IEnumerator EnsureCardSize(RectTransform cardRect, Vector2 targetSize)
    {
        yield return new WaitForEndOfFrame();
        if (cardRect != null)
        {
            cardRect.sizeDelta = targetSize;
            cardRect.localScale = Vector3.one;
        }
    }
}