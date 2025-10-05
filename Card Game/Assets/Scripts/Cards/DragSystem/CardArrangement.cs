using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CardArrangement : MonoBehaviour
{
    [Header("排列设置")]
    public float cardSpacing = 220f;
    public float arrangementSpeed = 8f;
    public bool autoArrange = false;
    [SerializeField] private int maxSlotCount = 4;

    [Header("更新槽设置")]
    public bool isUpdateSlot = false; // 新增：标记是否为更新槽

    private List<RectTransform> cards = new List<RectTransform>();
    private bool needsArrangement = false;
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private List<RectTransform> pendingRemovalCards = new List<RectTransform>();

    // 添加卡牌到排列系统 - 修改这里
    public void AddCard(RectTransform card)
    {
        if (!cards.Contains(card))
        {
            // 如果是更新槽且已经有卡牌，拒绝添加（防止从外部拖入）
            if (isUpdateSlot && cards.Count >= maxSlotCount)
            {
                Debug
.LogWarning("更新槽已满，无法添加更多卡牌");
                return;
            }

            cards
.Add(card);
            needsArrangement
= autoArrange;
            pendingRemovalCards
.Remove(card);
            RegisterCard(card);
        }
    }

    // 新增：检查更新槽是否可以接受卡牌
    public bool CanAcceptCardToUpdateSlot()
    {
        return !isUpdateSlot || cards.Count < maxSlotCount;
    }

    // 从排列系统移除卡牌
    public void RemoveCard(RectTransform card)
    {
        cards.Remove(card);
        needsArrangement = autoArrange;

        UnregisterCard(card);
    }

    // 新增：标记卡牌为待移除状态（用于区分移动和真正移除）
    public void MarkCardForRemoval(RectTransform card)
    {
        if (!pendingRemovalCards.Contains(card))
        {
            pendingRemovalCards.Add(card);
        }
    }

    // 新增：检查是否所有卡牌都已真正移除
    public bool AreAllCardsActuallyRemoved()
    {
        // 清理已销毁的卡牌引用
        cards.RemoveAll(card => card == null);
        pendingRemovalCards.RemoveAll(card => card == null);

        return cards.Count == 0 && pendingRemovalCards.Count == 0;
    }

    // 新增：检查排列容器中当前有多少张卡牌
    public int GetCardsInContainerCount()
    {
        // 清理已销毁的卡牌引用
        cards.RemoveAll(card => card == null || !card.gameObject.activeInHierarchy);

        // 只计算仍然在当前容器中的卡牌
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

    // 新增：检查排列容器是否为空
    public bool IsContainerEmpty()
    {
        return GetCardsInContainerCount() == 0;
    }

    // 立即排列所有卡牌
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

    // 手动触发排列（需要时调用）
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

    // 带动画的排列
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

    // 卡牌移动协程
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

    // 停止所有排列动画协程
    private void StopAllArrangementCoroutines()
    {
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    // ========== 卡牌状态跟踪功能 ==========

    /// <summary>
    /// 获取当前活跃的卡牌数量
    /// </summary>
    public int GetActiveCardCount()
    {
        cards.RemoveAll(card => card == null || !card.gameObject.activeInHierarchy);
        pendingRemovalCards.RemoveAll(card => card == null);
        return cards.Count;
    }

    /// <summary>
    /// 检查所有槽位是否都为空 - 修改这里！
    /// </summary>
    public bool AreAllSlotsEmpty()
    {
        // 修改：使用 IsContainerEmpty() 而不是 AreAllCardsActuallyRemoved()
        return IsContainerEmpty();
    }

    /// <summary>
    /// 检查是否还有空槽位
    /// </summary>
    public bool HasEmptySlots()
    {
        return GetActiveCardCount() < maxSlotCount;
    }

    /// <summary>
    /// 获取空槽位数量
    /// </summary>
    public int GetEmptySlotCount()
    {
        return maxSlotCount - GetActiveCardCount();
    }

    /// <summary>
    /// 添加卡牌到跟踪列表
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
    /// 从跟踪列表中移除卡牌
    /// </summary>
    private void UnregisterCard(RectTransform card)
    {
        cards.RemoveAll(c => c == null || c == card);
        pendingRemovalCards.RemoveAll(c => c == null || c == card);
    }

    /// <summary>
    /// 卡牌销毁时的回调
    /// </summary>
    private void OnCardDestroyed(RectTransform card)
    {
        UnregisterCard(card);
        needsArrangement = autoArrange;
    }

    /// <summary>
    /// 清空所有卡牌跟踪（用于重置场景）
    /// </summary>
    public void ClearAllCards()
    {
        StopAllArrangementCoroutines();
        cards.Clear();
        pendingRemovalCards.Clear();
    }

    /// <summary>
    /// 获取更新槽中实际的卡牌数量
    /// </summary>
    public int GetActualCardCountInContainer()
    {
        // 清理无效引用
        cards.RemoveAll(card => card == null || !card.gameObject.activeInHierarchy);

        int count = 0;
        foreach (RectTransform card in cards)
        {
            // 确保卡牌确实在当前容器中
            if (card != null && card.parent == this.transform)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 检查容器是否真正为空
    /// </summary>
    public bool IsContainerActuallyEmpty()
    {
        return GetActualCardCountInContainer() == 0;
    }

}


// 辅助类：处理卡牌生命周期事件
public class CardLifecycleHandler : MonoBehaviour
{
    public System.Action OnCardDestroyed;

    private void OnDestroy()
    {
        OnCardDestroyed?.Invoke();
    }

    
}