using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [Header("UI References")]
    // 【注意：如果你的背景 Image 不再是固定的，请移除它】
    // public Image cardBackground; 

    // 【新增】一个用于放置动态背景预制体的父对象引用
    public Transform cardBackgroundParent;

    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    private CardData _cardData;

    // 【新增】用于跟踪当前背景实例，以便在更新时销毁旧的
    private GameObject _currentBackgroundInstance;

    public void Setup(CardData data)
    {
        _cardData = data;
        UpdateView();
    }

    /// <summary>
    /// Set card data and update view
    /// </summary>
    public void SetCardData(CardData newData)
    {
        _cardData = newData;
        UpdateView();
    }

    private void UpdateView()
    {
        if (_cardData == null) return;

        // 【关键改动】先更新背景
        UpdateBackground();

        // 如果你保留了 backgroundColor，你可以注释掉或移除以下这行
        // cardBackground.color = _cardData.backgroundColor; 

        iconImage.sprite = _cardData.icon;
        nameText.text = _cardData.cardName;
        descriptionText.text = _cardData.description;

        statsText.text = GetStatsText();
    }

    /// <summary>
    /// 销毁旧背景并实例化新卡牌类型对应的背景预制体
    /// </summary>
    private void UpdateBackground()
    {
        // 1. 销毁旧的背景实例（如果有的话）
        if (_currentBackgroundInstance != null)
        {
            // 使用 DestroyImmediate 可能会更安全，但通常 Destroy 即可，取决于你的生成时机
            Destroy(_currentBackgroundInstance);
            _currentBackgroundInstance = null;
        }

        // 2. 实例化新的背景预制体
        if (_cardData.cardBackgroundPrefab != null && cardBackgroundParent != null)
        {
            GameObject newBackground = Instantiate(_cardData.cardBackgroundPrefab, cardBackgroundParent);
            _currentBackgroundInstance = newBackground;

            // 3. 设置 UI 布局：确保它填充父容器并位于最底层
            RectTransform rt = newBackground.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 设置为全尺寸，并设置锚点
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero; // 确保没有偏移
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;

                // 确保它在渲染层级上处于最底层
                newBackground.transform.SetSiblingIndex(0);
            }
        }
    }

    private string GetStatsText()
    {
        switch (_cardData)
        {
            // ... (这部分保持不变)
            case MonsterCardData monster:
                return $"攻击力: {monster.attack}\n血量: {monster.health}";

            case WeaponCardData weapon:
                return $"攻击力: {weapon.attack}\n耐久: {weapon.durability}/{weapon.maxDurability}";

            case ArmorCardData armor:
                return $"防御力: {armor.defense}\n耐久: {armor.durability}/{armor.maxDurability}";

            case FoodCardData food:
                return string.Format(food.useDescription, food.healAmount);
            //return $"恢复: {food.healAmount}";

            case MaterialCardData material:
                return "可合成道具";

            default:
                return "";
        }
    }

    public CardData GetCardData() => _cardData;

    public void RefreshDisplay()
    {
        UpdateView();
    }
}