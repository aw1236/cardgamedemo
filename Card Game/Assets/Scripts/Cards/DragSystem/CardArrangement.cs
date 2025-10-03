using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CardArrangement : MonoBehaviour
{
    [Header("��������")]
    public float cardSpacing = 220f;      // ���Ƽ��
    public float arrangementSpeed = 8f;   // ���ж����ٶ�
    public bool autoArrange = false;      // �� �������Ƿ��Զ�����

    private List<RectTransform> cards = new List<RectTransform>();
    private bool needsArrangement = false;

    // ��ӿ��Ƶ�����ϵͳ
    public void AddCard(RectTransform card)
    {
        if (!cards.Contains(card))
        {
            cards.Add(card);
            needsArrangement = autoArrange; // �� ֻ�������Զ�����ʱ����������
        }
    }

    // ������ϵͳ�Ƴ�����
    public void RemoveCard(RectTransform card)
    {
        cards.Remove(card);
        needsArrangement = autoArrange; // �� ֻ�������Զ�����ʱ����������
    }

    // �����������п���
    public void ArrangeCardsImmediately()
    {
        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Vector2 targetPosition = new Vector2(startX + (i * cardSpacing), 0);
            cards[i].anchoredPosition = targetPosition;
        }
    }

    // �ֶ��������У���Ҫʱ���ã�
    public void ManualArrange()
    {
        ArrangeCardsImmediately();
    }

    private void Update()
    {
        if (needsArrangement && autoArrange) // �� ֻ�������Զ�����ʱ��ִ��
        {
            ArrangeCardsWithAnimation();
            needsArrangement = false;
        }
    }

    // ������������
    private void ArrangeCardsWithAnimation()
    {
        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Vector2 targetPosition = new Vector2(startX + (i * cardSpacing), 0);
            StartCoroutine(MoveCardToPosition(cards[i], targetPosition));
        }
    }

    // �����ƶ�Э��
    private IEnumerator MoveCardToPosition(RectTransform card, Vector2 targetPosition)
    {
        Vector2 startPosition = card.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * arrangementSpeed;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        card.anchoredPosition = targetPosition;
    }
}