using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Card Game/Food Card")]
public class FoodCardData : CardData
{
    [Header("Food Stats")]
    //���ӵ�Ѫ��
    public int healAmount = 2;
    
    //��֤����ֻ����Food
    private void OnValidate()
    {
        cardType = CardType.Food;
    }
}