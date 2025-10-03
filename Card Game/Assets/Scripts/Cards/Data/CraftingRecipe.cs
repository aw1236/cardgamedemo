using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "������Ϸ/�ϳ��䷽")]
public class CraftingRecipe : ScriptableObject
{
    [Header("�ϳɲ���")]
    public CardData ingredient1;
    public CardData ingredient2;

    [Header("�ϳɽ��")]
    public CardData result;

    [Header("�ϳ�����")]
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