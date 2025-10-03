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
