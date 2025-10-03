using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card Game/Weapon Card")]
public class WeaponCardData : CardData
{
    [Header("Weapon Stats")]
    //������
    public int attack = 1;
    //�;�
    public int durability = 5;

    //��֤����ֻ����Weapon
    private void OnValidate()
    {
        cardType = CardType.Weapon;
    }
}
