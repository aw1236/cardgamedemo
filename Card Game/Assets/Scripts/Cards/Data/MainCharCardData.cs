using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Card Game/Character Card")]
public class MainCharacterCardData : CardData
{
    [Header("主角属性")]
    public int maxHealth = 12;
    public int health = 12;
    public int baseAttack = 1;

    [Header("装备引用")]
    public WeaponCardData equippedWeapon;
    public ArmorCardData equippedArmor;

    private void OnValidate()
    {
        cardType = CardType.MainCharacter;
    }

    // 获取总攻击力
    public int GetTotalAttack()
    {
        return baseAttack + (equippedWeapon != null ? equippedWeapon.attack : 0);
    }

    // 获取总防御力
    public int GetTotalDefense()
    {
        return equippedArmor != null ? equippedArmor.defense : 0;
    }
}