using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Card Game/Food Card")]
public class FoodCardData : CardData
{
    [Header("Food Stats")]
    //增加的血量
    public int healAmount = 2;
    
    //保证类型只能是Food
    private void OnValidate()
    {
        cardType = CardType.Food;
    }
}