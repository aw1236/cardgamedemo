using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Card Prefabs")]
    public GameObject cardPrefab;

    [Header("Available Cards")]
    public List<CardData> allCards = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 在 CardManager 的 Awake 或 Start 方法中添加调试
    private void Start()
    {
        Debug.Log($"CardManager 中的所有卡牌数量: {allCards.Count}");
        foreach (var card in allCards)
        {
            Debug.Log($"卡牌: {card.cardName}, 类型: {card.cardType}");
            if (card.cardName.Contains("chick")) // 根据你的蜜蜂卡实际名称调整
            {
                Debug.Log("✅ 找到蜜蜂卡！");
            }
        }
    }

    public GameObject CreateCard(CardData data, Transform parent = null)
    {
        if (cardPrefab == null || data == null)
        {
            Debug.LogError("Card prefab or data is missing!");
            return null;
        }

        GameObject cardObject = Instantiate(cardPrefab, parent);
        CardView cardView = cardObject.GetComponent<CardView>();

        if (cardView != null)
        {
            cardView.Setup(data);
        }

        return cardObject;
    }

    public CardData GetRandomCard()
    {
        if (allCards.Count == 0) return null;
        return allCards[Random.Range(0, allCards.Count)];
    }

    public List<CardData> GetCardsByType(CardType type)
    {
        return allCards.FindAll(card => card.cardType == type);
    }
}
