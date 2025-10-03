using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Card Game/Armor Card")]
public class ArmorCardData : CardData
{
    [Header("Armor Stats")]
    //防御力
    public int defense = 1;
    //耐久
    public int durability = 5;

    //保证类型只能是Armor
    private void OnValidate()
    {
        cardType = CardType.Armor;
    }
}
