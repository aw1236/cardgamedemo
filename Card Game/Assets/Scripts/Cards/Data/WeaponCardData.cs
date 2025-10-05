using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card Game/Weapon Card")]
public class WeaponCardData : CardData
{
    

    //保证类型只能是Weapon
    private void OnValidate()
    {
        cardType = CardType.Weapon;
    }

    [Header("装备属性")]
    public  int attack; // 攻击力
    public  int defense; // 防御力
    public  int durability; // 当前耐久度
    public  int maxDurability; // 🎯 新增：最大耐久度
}
