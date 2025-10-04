using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MainCharacterSlot : MonoBehaviour, IDropHandler
{
    [Header("主角设置")]
    public MainCharacterCardData mainCharacterData;

    [Header("UI引用")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI weaponStatusText;
    public TextMeshProUGUI armorStatusText;
    public TextMeshProUGUI combatLogText;

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
        AddCombatLog($"主角初始化完成-血量：{mainCharacterData.health}");
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

    // 核心：处理拖拽放置
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("🎯 MainCharacterSlot: 开始处理拖拽放置");

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

        Debug.Log($"🃏 处理卡牌: {cardData.cardName} (类型: {cardData.GetType()})");

        // 根据卡牌类型处理
        if (cardData is MonsterCardData monsterData)
        {
            Debug.Log($"⚔️ 触发战斗: {monsterData.cardName}");
            HandleMonsterDrop(monsterData, draggedObject);
        }
        else if (cardData is WeaponCardData weaponData)
        {
            Debug.Log($"🛡️ 装备武器: {weaponData.cardName}");
            HandleWeaponDrop(weaponData, draggedObject, cardView);
        }
        else if (cardData is ArmorCardData armorData)
        {
            Debug.Log($"🛡️ 装备盔甲: {armorData.cardName}");
            HandleArmorDrop(armorData, draggedObject, cardView);
        }
        else if (cardData is FoodCardData foodData) // 新增：处理食物卡
        {
            Debug.Log($"🍎 使用食物: {foodData.cardName}");
            HandleFoodDrop(foodData, draggedObject, cardView);
        }
        else
        {
            Debug.LogWarning($"不支持的卡牌类型: {cardData.cardType}");
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

        Debug.Log($"🏁 开始战斗: {monsterData.cardName} vs {mainCharacterData.cardName}");

        // 获取当前装备
        WeaponCardData weapon = mainCharacterData.equippedWeapon;
        ArmorCardData armor = mainCharacterData.equippedArmor;

        Debug.Log($"🛠️ 当前装备 - 武器: {(weapon != null ? weapon.cardName : "无")}, 盔甲: {(armor != null ? armor.cardName : "无")}");

        // 记录战斗前状态
        int playerHealthBefore = mainCharacterData.health;
        int monsterHealthBefore = monsterData.health;

        Debug.Log($"📊 战斗前状态 - 主角HP: {playerHealthBefore}, 怪物HP: {monsterHealthBefore}");

        // 检查 CombatManager 实例
        if (CombatManager.Instance == null)
        {
            Debug.LogError("❌ CombatManager 实例为空！");
            return;
        }

        Debug.Log("🔄 准备调用 CombatManager.PerformCombat...");

        // 执行战斗
        CombatManager.Instance.PerformCombat(monsterData, mainCharacterData, weapon, armor);

        Debug.Log("✅ CombatManager.PerformCombat 调用完成");

        // 检查战斗后状态
        Debug.Log($"📊 战斗后状态 - 主角HP: {mainCharacterData.health}, 怪物HP: {monsterData.health}");

        // 更新显示
        UpdateMainCharacterDisplay();

        // 添加战斗日志
        AddCombatLog($"与 {monsterData.cardName} 战斗");

        // 检查怪物死亡
        if (monsterData.health <= 0)
        {
            AddCombatLog($"🎯 击败了 {monsterData.cardName}！");
            Destroy(monsterObject);
            Debug.Log($"💀 怪物 {monsterData.cardName} 被销毁");
        }
        else
        {
            AddCombatLog($"{monsterData.cardName} 存活 (HP: {monsterData.health})");
            Debug.Log($"🐺 怪物 {monsterData.cardName} 存活，血量: {monsterData.health}");
        }

        // 检查主角死亡
        if (mainCharacterData.health <= 0)
        {
            AddCombatLog("💀 主角被击败！");
            Debug.Log("🎮 主角死亡！");
        }
    }
    private void HandleWeaponDrop(WeaponCardData weaponData, GameObject weaponObject, CardView cardView)
    {
        // 简单装备逻辑 - 直接装备
        mainCharacterData.equippedWeapon = weaponData;

        // 放置到槽位
        weaponObject.transform.SetParent(transform);
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localScale = Vector3.one;

        AddCombatLog($"⚔️ 装备了 {weaponData.cardName}");
        UpdateMainCharacterDisplay();

        Debug.Log($"✅ 武器装备成功: {weaponData.cardName}");
    }

    private void HandleArmorDrop(ArmorCardData armorData, GameObject armorObject, CardView cardView)
    {
        // 简单装备逻辑 - 直接装备
        mainCharacterData.equippedArmor = armorData;

        // 放置到槽位
        armorObject.transform.SetParent(transform);
        armorObject.transform.localPosition = Vector3.zero;
        armorObject.transform.localScale = Vector3.one;

        AddCombatLog($"🛡️ 装备了 {armorData.cardName}");
        UpdateMainCharacterDisplay();

        Debug.Log($"✅ 盔甲装备成功: {armorData.cardName}");
    }

    public void UpdateMainCharacterDisplay()
    {
        if (mainCharacterData == null) return;

        // 更新血量显示
        if (healthText != null)
        {
            healthText.text = $"HP: {mainCharacterData.health}/{mainCharacterData.maxHealth}";

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
                weaponStatusText.text = $"武器: {mainCharacterData.equippedWeapon.cardName}";
                weaponStatusText.color = Color.white;
            }
            else
            {
                weaponStatusText.text = "武器: 无";
                weaponStatusText.color = Color.gray;
            }
        }

        // 更新盔甲状态
        if (armorStatusText != null)
        {
            if (mainCharacterData.equippedArmor != null)
            {
                armorStatusText.text = $"盔甲: {mainCharacterData.equippedArmor.cardName}";
                armorStatusText.color = Color.white;
            }
            else
            {
                armorStatusText.text = "盔甲: 无";
                armorStatusText.color = Color.gray;
            }
        }

        Debug.Log($"📱 UI更新完成 - 血量: {mainCharacterData.health}/{mainCharacterData.maxHealth}");
    }

    public void AddCombatLog(string logMessage)
    {
        if (combatLogText != null)
        {
            combatLogText.text = $"{logMessage}\n{combatLogText.text}";

            // 限制行数
            string[] lines = combatLogText.text.Split('\n');
            if (lines.Length > 3)
            {
                combatLogText.text = string.Join("\n", lines, 0, 3);
            }
        }

        Debug.Log($"📝 战斗日志: {logMessage}");
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
            Debug.Log("❤️ 生命值已满，无法使用食物");
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
            AddCombatLog($"🍎 使用了 {foodData.cardName}，恢复 {healAmount} 点生命值");

            // 销毁食物卡（一次性消耗）
            Destroy(foodObject);
            Debug.Log($"✅ 食物使用成功: {foodData.cardName}，已销毁食物卡");

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
}


