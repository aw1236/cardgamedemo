using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Card Game/Monster Card")]
public class MonsterCardData : CardData
{
    [Header("Monster Stats")]
    //������
    public int attack = 1;
    //Ѫ��
    public int health = 3;


    //��֤����ֻ����Monster
    private void OnValidate()
    {
        cardType = CardType.Monster;
    }
}