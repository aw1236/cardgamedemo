using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Card Game/Food Card")]
public class FoodCardData : CardData
{
    [Header("Food Stats")]
    public int healAmount = 2;

    [TextArea]
    public string useDescription = "恢复 {0} 点生命值";

    private void OnValidate()
    {
        cardType = CardType.Food;
    }

    /// <summary>
    /// 使用食物效果
    /// </summary>
    public void UseFood(MainCharacterCardData targetCharacter)
    {
        if (targetCharacter == null) return;

        CombatManager.Instance.HealMainCharacter(targetCharacter, healAmount);
        Debug.Log($"使用了 {cardName}，恢复了 {healAmount} 点生命值");
    }
}