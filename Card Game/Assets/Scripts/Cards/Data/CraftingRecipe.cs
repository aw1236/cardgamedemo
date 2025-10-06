using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "合成设置")]
public class CraftingRecipe : ScriptableObject
{
    [Header("合成材料")]
    public CardData ingredient1;
    public CardData ingredient2;

    [Header("合成产物")]
    public CardData result;

    [Header("是否有序合成")]
    public bool requiresSpecificOrder = false;

    [TextArea]
    public string description;

    public bool Matches(CardData card1, CardData card2)
    {
        if (card1 == null || card2 == null) return false;

        if (requiresSpecificOrder)
        {
            return (IsSameCard(card1, ingredient1) && IsSameCard(card2, ingredient2));
        }
        else
        {
            return (IsSameCard(card1, ingredient1) && IsSameCard(card2, ingredient2)) ||
                   (IsSameCard(card1, ingredient2) && IsSameCard(card2, ingredient1));
        }
    }

    /// <summary>
    /// 🎯 修复：使用卡牌名称而不是对象引用来判断是否相同卡牌
    /// </summary>
    private bool IsSameCard(CardData cardA, CardData cardB)
    {
        if (cardA == null || cardB == null) return false;

        // 首先检查对象引用是否相同（原始情况）
        if (cardA == cardB) return true;

        // 如果对象引用不同，检查卡牌名称是否相同
        return cardA.cardName == cardB.cardName;
    }
}