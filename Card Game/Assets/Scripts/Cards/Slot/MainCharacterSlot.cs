using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MainCharacterSlot : MonoBehaviour, IDropHandler
{
    public static MainCharacterSlot Instance { get; private set; }

    [Header("主角设置")]
    public MainCharacterCardData mainCharacterData;

    [Header("UI引用")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI weaponStatusText;
    public TextMeshProUGUI armorStatusText;
    public TextMeshProUGUI combatLogText;

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 创建主角数据的运行时副本
        if (mainCharacterData != null)
        {
            MainCharacterCardData runtimeData = ScriptableObject.CreateInstance<MainCharacterCardData>();
            CopyCardData(mainCharacterData, runtimeData);
            runtimeData.health = runtimeData.maxHealth; // 重置满血
            mainCharacterData = runtimeData;
        }

        FindUIElements();
        UpdateMainCharacterDisplay();
        AddCombatLog($"主角血量：{mainCharacterData.health}");
    }

    private void CopyCardData(MainCharacterCardData source, MainCharacterCardData target)
    {
        target.cardName = source.cardName;
        target.cardType = source.cardType;
        target.icon = source.icon;
        target.description = source.description;
        target.maxHealth = source.maxHealth;
        target.health = source.health;
        target.baseAttack = source.baseAttack;
    }

    private void FindUIElements()
    {
        if (healthText == null) healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        if (weaponStatusText == null) weaponStatusText = GameObject.Find("WeaponStatus")?.GetComponent<TextMeshProUGUI>();
        if (armorStatusText == null) armorStatusText = GameObject.Find("ArmorStatus")?.GetComponent<TextMeshProUGUI>();
        if (combatLogText == null) combatLogText = GameObject.Find("CombatLog")?.GetComponent<TextMeshProUGUI>();
    }

    // 核心：处理拖拽放置 - 只处理怪物和食物，不再处理装备
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("MainCharacterSlot: 开始处理拖拽放置");

        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null)
        {
            Debug.LogError("拖拽对象为空");
            return;
        }

        CardDragHandler dragHandler = draggedObject.GetComponent<CardDragHandler>();
        CardView cardView = draggedObject.GetComponent<CardView>();

        if (cardView == null)
        {
            Debug.LogError("卡牌没有CardView组件");
            return;
        }

        CardData cardData = cardView.GetCardData();
        if (cardData == null)
        {
            Debug.LogError("卡牌数据为空");
            return;
        }

        Debug.Log($"处理卡牌: {cardData.cardName} (类型: {cardData.GetType()})");

        // 只处理怪物和食物，不再处理装备
        if (cardData is MonsterCardData monsterData)
        {
            Debug.Log($"触发战斗: {monsterData.cardName}");
            HandleMonsterDrop(monsterData, draggedObject);
        }
        else if (cardData is FoodCardData foodData)
        {
            Debug.Log($"使用食物: {foodData.cardName}");
            HandleFoodDrop(foodData, draggedObject, cardView);
        }
        else
        {
            Debug.LogWarning($"不支持的卡牌类型或请使用装备槽装备: {cardData.cardType}");
            if (dragHandler != null) dragHandler.ReturnToOriginalPosition();
        }
    }

    private void HandleMonsterDrop(MonsterCardData monsterData, GameObject monsterObject)
    {
        if (mainCharacterData == null)
        {
            Debug.LogError("主角数据未设置");
            return;
        }

        Debug.Log($"开始战斗: {monsterData.cardName} vs {mainCharacterData.cardName}");

        // 使用怪物对象上的组件来存储临时血量
        MonsterHealthController healthController = monsterObject.GetComponent<MonsterHealthController>();
        if (healthController == null)
        {
            healthController = monsterObject.AddComponent<MonsterHealthController>();
        }

        // 初始化或获取怪物当前血量
        int monsterCurrentHealth = healthController.InitializeOrGetHealth(monsterData.health);

        Debug.Log($"当前装备 - 武器: {(mainCharacterData.equippedWeapon != null ? mainCharacterData.equippedWeapon.cardName : "无")}, 盔甲: {(mainCharacterData.equippedArmor != null ? mainCharacterData.equippedArmor.cardName : "无")}");
        Debug.Log($"战斗前状态 - 主角HP: {mainCharacterData.health}, 怪物HP: {monsterCurrentHealth}");

        // 检查 CombatManager 实例
        if (CombatManager.Instance == null)
        {
            Debug.LogError("CombatManager 实例为空！");
            return;
        }

        Debug.Log("准备调用 CombatManager.PerformCombat...");

        // 传递怪物对象引用和血量控制器
        CombatManager.Instance.PerformCombat(monsterData, mainCharacterData, monsterObject, healthController);

        Debug.Log("CombatManager.PerformCombat 调用完成");

        // 获取战斗后的怪物血量
        int monsterHealthAfter = healthController.GetCurrentHealth();

        Debug.Log($"战斗后状态 - 主角HP: {mainCharacterData.health}, 怪物HP: {monsterHealthAfter}");

        // 更新显示
        UpdateMainCharacterDisplay();

        // 添加战斗日志
        AddCombatLog($"与 {monsterData.cardName} 战斗");

        // 检查怪物死亡
        if (monsterHealthAfter <= 0)
        {
            AddCombatLog($"击败了 {monsterData.cardName}！");
            Destroy(monsterObject);
            Debug.Log($"怪物 {monsterData.cardName} 被销毁");
        }
        else
        {
            AddCombatLog($"{monsterData.cardName} 存活 (HP: {monsterHealthAfter})");
            Debug.Log($"怪物 {monsterData.cardName} 存活，血量: {monsterHealthAfter}");
        }

        // 检查主角死亡
        if (mainCharacterData.health <= 0)
        {
            AddCombatLog("主角被击败！");
            Debug.Log("主角死亡！");
        }
    }

    /// <summary>
    /// 处理食物卡放置
    /// </summary>
    private void HandleFoodDrop(FoodCardData foodData, GameObject foodObject, CardView cardView)
    {
        if (mainCharacterData == null)
        {
            Debug.LogError("主角数据未设置");
            return;
        }

        // 检查是否可以食用
        if (!FoodSystem.Instance.CanEatFood(mainCharacterData))
        {
            Debug.Log("生命值已满，无法使用食物");
            AddCombatLog($"无法使用 {foodData.cardName} (生命值已满)");
            if (foodObject.GetComponent<CardDragHandler>() != null)
            {
                foodObject.GetComponent<CardDragHandler>().ReturnToOriginalPosition();
            }
            return;
        }

        // 记录治疗前状态
        int healthBefore = mainCharacterData.health;

        // 使用食物
        bool success = FoodSystem.Instance.UseFoodCard(foodData, mainCharacterData);

        if (success)
        {
            // 添加战斗日志
            int healAmount = mainCharacterData.health - healthBefore;
            AddCombatLog($"使用了 {foodData.cardName}，恢复 {healAmount} 点生命值");

            // 销毁食物卡（一次性消耗）
            Destroy(foodObject);
            Debug.Log($"食物使用成功: {foodData.cardName}，已销毁食物卡");

            // 更新UI显示
            UpdateMainCharacterDisplay();
        }
        else
        {
            // 使用失败，返回原位置
            if (foodObject.GetComponent<CardDragHandler>() != null)
            {
                foodObject.GetComponent<CardDragHandler>().ReturnToOriginalPosition();
            }
        }
    }

    public void UpdateMainCharacterDisplay()
    {
        if (mainCharacterData == null) return;

        // 更新血量显示
        if (healthText != null)
        {
            healthText.text = $"血量: {mainCharacterData.health}/{mainCharacterData.maxHealth}";

            // 血量颜色
            float healthPercent = (float)mainCharacterData.health / mainCharacterData.maxHealth;
            if (healthPercent <= 0.3f) healthText.color = Color.red;
            else if (healthPercent <= 0.6f) healthText.color = Color.yellow;
            else healthText.color = Color.green;
        }

        // 更新武器状态
        if (weaponStatusText != null)
        {
            if (mainCharacterData.equippedWeapon != null)
            {
                int totalAttack = mainCharacterData.baseAttack + mainCharacterData.equippedWeapon.attack;
                weaponStatusText.text = $"武器: {mainCharacterData.equippedWeapon.cardName} (攻击:{totalAttack}, 耐久:{mainCharacterData.equippedWeapon.durability})";
                weaponStatusText.color = Color.red;
            }
            else
            {
                weaponStatusText.text = $"武器: 无 (攻击:{mainCharacterData.baseAttack})";
                weaponStatusText.color = Color.white;
            }
        }

        // 更新盔甲状态
        if (armorStatusText != null)
        {
            if (mainCharacterData.equippedArmor != null)
            {
                armorStatusText.text = $"盔甲: {mainCharacterData.equippedArmor.cardName} (防御:{mainCharacterData.equippedArmor.defense}, 耐久:{mainCharacterData.equippedArmor.durability})";
                armorStatusText.color = Color.blue;
            }
            else
            {
                armorStatusText.text = "盔甲: 无 (防御:0)";
                armorStatusText.color = Color.white;
            }
        }

        Debug.Log($"UI更新完成 - 血量: {mainCharacterData.health}/{mainCharacterData.maxHealth}");
    }

    public void AddCombatLog(string logMessage)
    {
        if (combatLogText != null)
        {
            // 保留：清理不可见字符
            string cleanMessage = CleanInvisibleCharacters(logMessage);

            // 🎯 修改1：新内容添加到最下方（像微信一样）
            if (string.IsNullOrEmpty(combatLogText.text))
            {
                combatLogText.text = cleanMessage;
            }
            else
            {
                combatLogText.text = $"{combatLogText.text}\n{cleanMessage}";
            }

            // 🎯 修改2：保留最下面的3行（最新的内容）
            string[] lines = combatLogText.text.Split('\n');
            if (lines.Length > 2)
            {
                combatLogText.text = string.Join("\n", lines, lines.Length - 2, 2);
            }
        }

        Debug.Log($"战斗日志: {logMessage}");
    }
    // 保留这个有用的方法
    private string CleanInvisibleCharacters(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = text.Replace("\u200B", "");
        text = text.Replace("\uFEFF", "");
        text = text.Replace("\u200E", "");
        text = text.Replace("\u200F", "");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in text)
        {
            if (!char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Trim();
    }
}