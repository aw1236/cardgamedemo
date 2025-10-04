using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CardArrangement : MonoBehaviour
{
    [Header("��������")]
    public float cardSpacing = 220f;      // ���Ƽ��
    public float arrangementSpeed = 8f;   // ���ж����ٶ�
    public bool autoArrange = false;      // �Ƿ��Զ�����
    [SerializeField] private int maxSlotCount = 4; // ���Ʋ�λ����

    private List<RectTransform> cards = new List<RectTransform>();
    private bool needsArrangement = false;
    private List<Coroutine> activeCoroutines = new List<Coroutine>(); // ���ٻ�Ծ�Ķ���Э��

    // ���������ٿ����Ƿ��������Ƴ������������ƶ�λ�ã�
    private List<RectTransform> pendingRemovalCards = new List<RectTransform>();

    // ��ӿ��Ƶ�����ϵͳ
    public void AddCard(RectTransform card)
    {
        if (!cards.Contains(card))
        {
            cards.Add(card);
            needsArrangement = autoArrange;

            // ��������ڴ��Ƴ��б��У����Ƴ�
            pendingRemovalCards.Remove(card);

            RegisterCard(card);
        }
    }

    // ������ϵͳ�Ƴ�����
    public void RemoveCard(RectTransform card)
    {
        cards.Remove(card);
        needsArrangement = autoArrange;

        UnregisterCard(card);
    }

    // ��������ǿ���Ϊ���Ƴ�״̬�����������ƶ��������Ƴ���
    public void MarkCardForRemoval(RectTransform card)
    {
        if (!pendingRemovalCards.Contains(card))
        {
            pendingRemovalCards.Add(card);
        }
    }

    // ����������Ƿ����п��ƶ��������Ƴ�
    public bool AreAllCardsActuallyRemoved()
    {
        // ���������ٵĿ�������
        cards.RemoveAll(card => card == null);
        pendingRemovalCards.RemoveAll(card => card == null);

        return cards.Count == 0 && pendingRemovalCards.Count == 0;
    }

    // ������������������е�ǰ�ж����ſ���
    public int GetCardsInContainerCount()
    {
        // ���������ٵĿ�������
        cards.RemoveAll(card => card == null || !card.gameObject.activeInHierarchy);

        // ֻ������Ȼ�ڵ�ǰ�����еĿ���
        int count = 0;
        foreach (RectTransform card in cards)
        {
            if (card != null && card.parent == this.transform)
            {
                count++;
            }
        }
        return count;
    }

    // ������������������Ƿ�Ϊ��
    public bool IsContainerEmpty()
    {
        return GetCardsInContainerCount() == 0;
    }

    // �����������п���
    public void ArrangeCardsImmediately()
    {
        StopAllArrangementCoroutines();

        if (cards.Count == 0) return;

        cards.RemoveAll(card => card == null);

        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                Vector2 targetPosition = new Vector2(startX + (i * cardSpacing), 0);
                cards[i].anchoredPosition = targetPosition;
            }
        }
    }

    // �ֶ��������У���Ҫʱ���ã�
    public void ManualArrange()
    {
        ArrangeCardsImmediately();
    }

    private void Update()
    {
        if (needsArrangement && autoArrange)
        {
            ArrangeCardsWithAnimation();
            needsArrangement = false;
        }
    }

    // ������������
    private void ArrangeCardsWithAnimation()
    {
        StopAllArrangementCoroutines();

        cards.RemoveAll(card => card == null);

        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                Vector2 targetPosition = new Vector2(startX + (i * cardSpacing), 0);
                Coroutine coroutine = StartCoroutine(MoveCardToPosition(cards[i], targetPosition));
                activeCoroutines.Add(coroutine);
            }
        }
    }

    // �����ƶ�Э��
    private IEnumerator MoveCardToPosition(RectTransform card, Vector2 targetPosition)
    {
        if (card == null) yield break;

        Vector2 startPosition = card.anchoredPosition;
        float t = 0f;

        while (t < 1f && card != null)
        {
            t += Time.deltaTime * arrangementSpeed;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (card != null)
        {
            card.anchoredPosition = targetPosition;
        }
    }

    // ֹͣ�������ж���Э��
    private void StopAllArrangementCoroutines()
    {
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    // ========== ����״̬���ٹ��� ==========

    /// <summary>
    /// ��ȡ��ǰ��Ծ�Ŀ�������
    /// </summary>
    public int GetActiveCardCount()
    {
        cards.RemoveAll(card => card == null || !card.gameObject.activeInHierarchy);
        pendingRemovalCards.RemoveAll(card => card == null);
        return cards.Count;
    }

    /// <summary>
    /// ������в�λ�Ƿ�Ϊ�� - �޸����
    /// </summary>
    public bool AreAllSlotsEmpty()
    {
        // �޸ģ�ʹ�� IsContainerEmpty() ������ AreAllCardsActuallyRemoved()
        return IsContainerEmpty();
    }

    /// <summary>
    /// ����Ƿ��пղ�λ
    /// </summary>
    public bool HasEmptySlots()
    {
        return GetActiveCardCount() < maxSlotCount;
    }

    /// <summary>
    /// ��ȡ�ղ�λ����
    /// </summary>
    public int GetEmptySlotCount()
    {
        return maxSlotCount - GetActiveCardCount();
    }

    /// <summary>
    /// ��ӿ��Ƶ������б�
    /// </summary>
    private void RegisterCard(RectTransform card)
    {
        var cardEventHandler = card.gameObject.GetComponent<CardLifecycleHandler>();
        if (cardEventHandler == null)
        {
            cardEventHandler = card.gameObject.AddComponent<CardLifecycleHandler>();
        }
        cardEventHandler.OnCardDestroyed += () => OnCardDestroyed(card);
    }

    /// <summary>
    /// �Ӹ����б����Ƴ�����
    /// </summary>
    private void UnregisterCard(RectTransform card)
    {
        cards.RemoveAll(c => c == null || c == card);
        pendingRemovalCards.RemoveAll(c => c == null || c == card);
    }

    /// <summary>
    /// ��������ʱ�Ļص�
    /// </summary>
    private void OnCardDestroyed(RectTransform card)
    {
        UnregisterCard(card);
        needsArrangement = autoArrange;
    }

    /// <summary>
    /// ������п��Ƹ��٣��������ó�����
    /// </summary>
    public void ClearAllCards()
    {
        StopAllArrangementCoroutines();
        cards.Clear();
        pendingRemovalCards.Clear();
    }
}

// �����ࣺ���������������¼�
public class CardLifecycleHandler : MonoBehaviour
{
    public System.Action OnCardDestroyed;

    private void OnDestroy()
    {
        OnCardDestroyed?.Invoke();
    }
}