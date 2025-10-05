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
    public Color backgroundColor = Color.white;

   



}
