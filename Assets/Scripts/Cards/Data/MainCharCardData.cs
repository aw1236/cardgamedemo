using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Card Game/Character Card")]
public class MainCharacterCardData : CardData
{
    [Header("��������")]
    public int maxHealth = 12;
    public int health = 12;
    public int baseAttack = 1;

    [Header("װ������")]
    public WeaponCardData equippedWeapon;
    public ArmorCardData equippedArmor;

    private void OnValidate()
    {
        cardType = CardType.MainCharacter;
    }

    // ��ȡ�ܹ�����
    public int GetTotalAttack()
    {
        return baseAttack + (equippedWeapon != null ? equippedWeapon.attack : 0);
    }

    // ��ȡ�ܷ�����
    public int GetTotalDefense()
    {
        return equippedArmor != null ? equippedArmor.defense : 0;
    }
}