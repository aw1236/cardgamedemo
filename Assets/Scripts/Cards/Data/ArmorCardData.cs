using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Card Game/Armor Card")]
public class ArmorCardData : CardData
{
    [Header("Armor Stats")]
    //������
    public int defense = 1;
    //�;�
    public int durability = 5;

    //��֤����ֻ����Armor
    private void OnValidate()
    {
        cardType = CardType.Armor;
    }
}
