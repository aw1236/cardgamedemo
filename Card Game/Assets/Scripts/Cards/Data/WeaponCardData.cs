using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card Game/Weapon Card")]
public class WeaponCardData : CardData
{
    [Header("Weapon Stats")]
    //攻击力
    public int attack = 1;
    //耐久
    public int durability = 5;

    //保证类型只能是Weapon
    private void OnValidate()
    {
        cardType = CardType.Weapon;
    }
}
