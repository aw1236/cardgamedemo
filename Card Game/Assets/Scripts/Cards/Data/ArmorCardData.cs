using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Card Game/Armor Card")]
public class ArmorCardData : CardData
{
    [Header("装备属性")]
    public int  attack; // 攻击力
    public int defense; // 防御力
    public int durability; // 当前耐久度
    public int maxDurability; // 🎯 新增：最大耐久度

    //保证类型只能是Armor
    private void OnValidate()
    {
        cardType = CardType.Armor;
    }
}
