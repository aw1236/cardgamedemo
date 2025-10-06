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
        _cardData
= data;
        UpdateView();
    }

    /// <summary>
    /// Set card data and update view
    /// </summary>
    public void SetCardData(CardData newData)
    {
        _cardData
= newData;
        UpdateView();
    }

    private void UpdateView()
    {
        if (_cardData == null) return;

        cardBackground
.color = _cardData.backgroundColor;
        iconImage
.sprite = _cardData.icon;
        nameText
.text = _cardData.cardName;
        descriptionText
.text = _cardData.description;

        statsText
.text = GetStatsText();
    }

    private string GetStatsText()
    {
        switch (_cardData)
        {
            case MonsterCardData monster:
                return $"Attack: {monster.attack}\nHealth: {monster.health}";

            case WeaponCardData weapon:
                return $"Attack: {weapon.attack}\nDurability: {weapon.durability}/{weapon.maxDurability}";

            case ArmorCardData armor:
                return $"Defense: {armor.defense}\nDurability: {armor.durability}/{armor.maxDurability}";

            case FoodCardData food:
                return $"HealAmount: {food.healAmount}";

            case MaterialCardData material:
                return "isCraftingMaterial";

            default:
                return "";
        }
    }

    public CardData GetCardData() => _cardData;

    /// <summary>
    /// NEW: Update card display without changing card data
    /// Useful for durability updates
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateView();
    }
}