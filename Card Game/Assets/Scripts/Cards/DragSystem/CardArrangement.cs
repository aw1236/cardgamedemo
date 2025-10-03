using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CardArrangement : MonoBehaviour
{
    [Header("排列设置")]
    public float cardSpacing = 220f;      // 卡牌间距
    public float arrangementSpeed = 8f;   // 排列动画速度
    public bool autoArrange = false;      // ← 新增：是否自动排列

    private List<RectTransform> cards = new List<RectTransform>();
    private bool needsArrangement = false;

    // 添加卡牌到排列系统
    public void AddCard(RectTransform card)
    {
        if (!cards.Contains(card))
        {
            cards.Add(card);
            needsArrangement = autoArrange; // ← 只有启用自动排列时才重新排列
        }
    }

    // 从排列系统移除卡牌
    public void RemoveCard(RectTransform card)
    {
        cards.Remove(card);
        needsArrangement = autoArrange; // ← 只有启用自动排列时才重新排列
    }

    // 立即排列所有卡牌
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

    // 手动触发排列（需要时调用）
    public void ManualArrange()
    {
        ArrangeCardsImmediately();
    }

    private void Update()
    {
        if (needsArrangement && autoArrange) // ← 只有启用自动排列时才执行
        {
            ArrangeCardsWithAnimation();
            needsArrangement = false;
        }
    }

    // 带动画的排列
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

    // 卡牌移动协程
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