using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "卡牌游戏/合成配方")]
public class CraftingRecipe : ScriptableObject
{
    [Header("合成材料")]
    public CardData ingredient1;
    public CardData ingredient2;

    [Header("合成结果")]
    public CardData result;

    [Header("合成条件")]
    public bool requiresSpecificOrder = false;

    [TextArea]
    public string description;

    public bool Matches(CardData card1, CardData card2)
    {
        if (requiresSpecificOrder)
        {
            return (card1 == ingredient1 && card2 == ingredient2);
        }
        else
        {
            return (card1 == ingredient1 && card2 == ingredient2) ||
                   (card1 == ingredient2 && card2 == ingredient1);
        }
    }
}