using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Card Game/Material Card")]
public class MaterialCardData : CardData
{
    [Header("Material Info")]
    //是否能被合成？？？
    public bool isCraftingMaterial = true;

    //保证类型只能是Material
    private void OnValidate()
    {
        cardType = CardType.Material;
    }
}