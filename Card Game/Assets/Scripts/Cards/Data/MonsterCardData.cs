using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Card Game/Monster Card")]
public class MonsterCardData : CardData
{
    [Header("怪物属性")]
    public int health = 3;
    public int attack = 1;

    // 🎯 移除对 MonsterStateManager 的引用
    // 我们不再需要这些复杂的方法，因为血量现在由 MonsterHealthController 管理

    private void OnValidate()
    {
        cardType = CardType.Monster;
    }

    // 🎯 可选：添加一个简单的方法来获取基础血量（用于显示等）
    public int GetBaseHealth()
    {
        return health;
    }

    // 🎯 可选：添加一个简单的方法来获取基础攻击力
    public int GetBaseAttack()
    {
        return attack;
    }
}