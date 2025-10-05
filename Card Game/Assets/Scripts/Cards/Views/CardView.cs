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

    /// <summary>
    /// 🎯 设置卡牌数据（用于运行时数据替换）
    /// </summary>
    public void SetCardData(CardData newData)
    {
        _cardData = newData;
        UpdateView(); // 更新显示
    }

    private void UpdateView()
    {
        if (_cardData == null) return;

        // 基本设置
        cardBackground.color = _cardData.backgroundColor;
        iconImage.sprite = _cardData.icon;
        nameText.text = _cardData.cardName;
        descriptionText.text = _cardData.description;

        // 根据卡牌类型显示属性
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