using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    [Header("UI References")]
    public Transform cardBackgroundParent;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    private CardData _cardData;
    private GameObject _currentBackgroundInstance;

    public void Setup(CardData data)
    {
        _cardData = data;
        UpdateView();

        // 🎯 新增：立即更新背景
        UpdateBackground();
    }

    /// <summary>
    /// Set card data and update view
    /// </summary>
    public void SetCardData(CardData newData)
    {
        _cardData = newData;
        UpdateView();

        // 🎯 新增：立即更新背景
        UpdateBackground();
    }

    private void UpdateView()
    {
        if (_cardData == null) return;

        // 🎯 关键修复：确保背景更新被调用
        UpdateBackground();

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
        // 移除旧的背景实例
        if (_currentBackgroundInstance != null)
        {
            Destroy(_currentBackgroundInstance);
            _currentBackgroundInstance = null;
        }

        if (_cardData == null) return;

        // 简化的检查
        if (_cardData.cardBackgroundPrefab == null)
        {
            CreateDefaultBackground();
            return;
        }

        if (cardBackgroundParent == null) return;

        // 实例化新背景（移除try-catch和详细日志）
        GameObject newBackground = Instantiate(_cardData.cardBackgroundPrefab, cardBackgroundParent);
        _currentBackgroundInstance = newBackground;

        // 设置UI布局
        RectTransform rt = newBackground.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
            newBackground.transform.SetSiblingIndex(0);
        }
    }

    /// <summary>
    /// 🎯 新增：创建默认背景
    /// </summary>
    private void CreateDefaultBackground()
    {
        if (cardBackgroundParent == null) return;

        // 创建默认背景对象
        GameObject defaultBg = new GameObject("DefaultBackground");
        defaultBg.transform.SetParent(cardBackgroundParent);
        _currentBackgroundInstance = defaultBg;

        // 添加 RectTransform
        RectTransform rt = defaultBg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        // 添加 Image 组件
        Image bgImage = defaultBg.AddComponent<Image>();

        // 根据卡牌类型设置不同颜色
        switch (_cardData.cardType)
        {
            case CardType.Weapon:
                bgImage.color = new Color(0.8f, 0.2f, 0.2f, 0.7f); // 红色
                break;
            case CardType.Armor:
                bgImage.color = new Color(0.2f, 0.2f, 0.8f, 0.7f); // 蓝色
                break;
            case CardType.Monster:
                bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.7f); // 灰色
                break;
            default:
                bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 默认灰色
                break;
        }

        defaultBg.transform.SetSiblingIndex(0);
        Debug.Log($"创建默认背景完成: {_cardData.cardName}");
    }

    private string GetStatsText()
    {
        if (_cardData == null) return "";

        switch (_cardData)
        {
            case MonsterCardData monster:
                // 🎯 关键修复：获取怪物的当前血量而不是基础血量
                int currentHealth = monster.health;

                // 尝试从MonsterHealthController获取实时血量
                MonsterHealthController healthController = GetComponent<MonsterHealthController>();
                if (healthController != null)
                {
                    currentHealth = healthController.GetCurrentHealth();
                }

                return $"攻击力: {monster.attack}\n血量: {currentHealth}";

            case WeaponCardData weapon:
                return $"攻击力: {weapon.attack}\n耐久: {weapon.durability}/{weapon.maxDurability}";

            case ArmorCardData armor:
                return $"防御力: {armor.defense}\n耐久: {armor.durability}/{armor.MaxDurability}";

            case FoodCardData food:
                return string.Format(food.useDescription, food.healAmount);

            case MaterialCardData material:
                return "可合成道具";

            default:
                return "";
        }
    }
    public CardData GetCardData() => _cardData;

    public void RefreshDisplay()
    {
        Debug.Log("RefreshDisplay 被调用");
        UpdateView();
    }

    // 🎯 新增：专门用于更新背景的方法
    public void RefreshBackground()
    {
        Debug.Log("RefreshBackground 被调用");
        UpdateBackground();
    }
}