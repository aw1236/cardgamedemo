using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [Header("UI References")]
    public Image cardBackground;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    private CardData _cardData;

    public void Setup(CardData data)
    {
        _cardData = data;
        UpdateView();
    }

    private void UpdateView()
    {
        if (_cardData == null) return;

        // 基础信息
        cardBackground.color = _cardData.backgroundColor;
        iconImage.sprite = _cardData.icon;
        nameText.text = _cardData.cardName;
        descriptionText.text = _cardData.description;

        // 根据卡牌类型显示不同属性
        statsText.text = GetStatsText();
    }

    private string GetStatsText()
    {
        switch (_cardData)
        {
            case MonsterCardData monster:
                return $"Attack: {monster.attack}\nHealth: {monster.health}";

            case WeaponCardData weapon:
                return $"Attack: {weapon.attack}\nDurability: {weapon.durability}";

            case ArmorCardData armor:
                return $"Defense: {armor.defense}\nDurability: {armor.durability}";

            case FoodCardData food:
                return $"HealAmount: {food.healAmount}";

            case MaterialCardData material:
                return "isCraftingMaterial";

            default:
                return "";
        }
    }

    public CardData GetCardData() => _cardData;
}