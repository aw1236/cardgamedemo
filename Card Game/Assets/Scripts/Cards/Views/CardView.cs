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
        Debug.Log($"🎨 UpdateBackground 被调用: {_cardData?.cardName}");

        // 1. 销毁旧的背景实例（如果有的话）
        if (_currentBackgroundInstance != null)
        {
            Destroy(_currentBackgroundInstance);
            _currentBackgroundInstance = null;
            Debug.Log("🗑️ 销毁旧背景实例");
        }

        // 2. 检查必要组件
        if (_cardData == null)
        {
            Debug.LogError("❌ CardData 为空");
            return;
        }

        // 🎯 更详细的调试信息
        Debug.Log($"🔍 检查卡牌数据: {_cardData.cardName}, Type: {_cardData.GetType()}");
        Debug.Log($"🔍 cardBackgroundPrefab: {_cardData.cardBackgroundPrefab}");
        Debug.Log($"🔍 cardBackgroundParent: {cardBackgroundParent}");

        if (_cardData.cardBackgroundPrefab == null)
        {
            Debug.LogError($"❌ {_cardData.cardName} 的 cardBackgroundPrefab 为空");

            // 🎯 临时创建默认背景
            CreateDefaultBackground();
            return;
        }

        if (cardBackgroundParent == null)
        {
            Debug.LogError("❌ cardBackgroundParent 引用为空");
            return;
        }

        // 3. 实例化新的背景预制体
        Debug.Log($"🔄 创建背景: {_cardData.cardName}");

        try
        {
            GameObject newBackground = Instantiate(_cardData.cardBackgroundPrefab, cardBackgroundParent);
            _currentBackgroundInstance = newBackground;

            // 设置 UI 布局
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

            Debug.Log("✅ 背景创建成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 背景实例化失败: {e.Message}");
            CreateDefaultBackground();
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
        Debug.Log($"✅ 创建默认背景完成: {_cardData.cardName}");
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
                return $"防御力: {armor.defense}\n耐久: {armor.durability}/{armor.maxDurability}";

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
        Debug.Log("🔄 RefreshDisplay 被调用");
        UpdateView();
    }

    // 🎯 新增：专门用于更新背景的方法
    public void RefreshBackground()
    {
        Debug.Log("🎨 RefreshBackground 被调用");
        UpdateBackground();
    }
}