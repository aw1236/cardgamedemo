using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public CardType cardType;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Visual")]
    // 移除或保留 'backgroundColor'，但我们主要使用预制体
    public Color backgroundColor = Color.white;

    // 【新增】引用卡牌背景的预制体
    public GameObject cardBackgroundPrefab;
}

