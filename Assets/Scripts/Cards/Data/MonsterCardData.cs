using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Card Game/Monster Card")]
public class MonsterCardData : CardData
{
    [Header("Monster Stats")]
    //攻击力
    public int attack = 1;
    //血量
    public int health = 3;


    //保证类型只能是Monster
    private void OnValidate()
    {
        cardType = CardType.Monster;
    }
}