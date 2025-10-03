using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Card Game/Material Card")]
public class MaterialCardData : CardData
{
    [Header("Material Info")]
    //�Ƿ��ܱ��ϳɣ�����
    public bool isCraftingMaterial = true;

    //��֤����ֻ����Material
    private void OnValidate()
    {
        cardType = CardType.Material;
    }
}