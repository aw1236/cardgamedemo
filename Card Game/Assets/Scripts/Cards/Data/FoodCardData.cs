using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Card Game/Food Card")]
public class FoodCardData : CardData
{
    [Header("Food Stats")]
    public int healAmount = 2;

    [TextArea]
    public string useDescription = "�ָ� {0} ������ֵ";

    private void OnValidate()
    {
        cardType = CardType.Food;
    }

    /// <summary>
    /// ʹ��ʳ��Ч��
    /// </summary>
    public void UseFood(MainCharacterCardData targetCharacter)
    {
        if (targetCharacter == null) return;

        CombatManager.Instance.HealMainCharacter(targetCharacter, healAmount);
        Debug.Log($"ʹ���� {cardName}���ָ��� {healAmount} ������ֵ");
    }
}