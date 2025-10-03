using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq; // 引入 Linq 以使用 FirstOrDefault

public class MainCharacterSlot : MonoBehaviour, IDropHandler
{
    [Header("主角设置")]
    // 运行时的主角数据，从 ScriptableObject 复制而来
    public MainCharacterCardData mainCharacterData;

    [Header("UI引用")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI weaponStatusText;
    public TextMeshProUGUI armorStatusText;
    public TextMeshProUGUI combatLogText;

    // --- 日志管理 ---
    private List<string> combatLogs = new List<string>();
    [Header("日志设置")]
    public int maxLogLines = 1; // 战斗日志最多显示 1行
    // ----------------------------

    private void Start()
    {
        // 1. 创建主角数据的运行时副本 (防止修改原始 ScriptableObject)
        if (mainCharacterData != null)
        {
            MainCharacterCardData runtimeData = ScriptableObject.CreateInstance<MainCharacterCardData>();
            CopyCardData(mainCharacterData, runtimeData);
            runtimeData.health = runtimeData.maxHealth; // 重置满血
            mainCharacterData = runtimeData;
        }

        // 2. 查找 UI 元素并初始化显示
        FindUIElements();
        UpdateMainCharacterDisplay();
        AddCombatLog($"主角初始化完成-血量：{mainCharacterData.health}");
    }

    // 复制 ScriptableObject 数据到运行时实例
    private void CopyCardData(MainCharacterCardData source, MainCharacterCardData target)
    {
        target.cardName = source.cardName;
        target.cardType = source.cardType;
        target.icon = source.icon;
        target.description = source.description;
        target.maxHealth = source.maxHealth;
        target.health = source.health;
        target.baseAttack = source.baseAttack;

        // 确保装备为空
        target.equippedWeapon = null;
        target.equippedArmor = null;
    }

    // 尝试在运行时查找 UI 元素，以防 Inspector 中忘记设置
    private void FindUIElements()
    {
        if (healthText == null) healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        if (weaponStatusText == null) weaponStatusText = GameObject.Find("WeaponStatus")?.GetComponent<TextMeshProUGUI>();
        if (armorStatusText == null) armorStatusText = GameObject.Find("ArmorStatus")?.GetComponent<TextMeshProUGUI>();
        // 查找战斗日志 TextMeshPro 组件
        if (combatLogText == null) combatLogText = GameObject.Find("CombatLog")?.GetComponent<TextMeshProUGUI>();
    }

    // 核心：处理拖拽放置 (IDropHandler 接口实现)
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
            Debug.LogError("卡牌没有CardView组件，无法获取数据");
            if (dragHandler != null) dragHandler.ReturnToOriginalPosition();
            return;
        }

        CardData cardData = cardView.GetCardData();
        if (cardData == null)
        {
            Debug.LogError("卡牌数据为空");
            if (dragHandler != null) dragHandler.ReturnToOriginalPosition();
            return;
        }

        Debug.Log($"🃏 处理卡牌: {cardData.cardName} (类型: {cardData.cardType})");

        // 根据卡牌类型处理
        if (cardData is MonsterCardData monsterData)
        {
            // 怪物卡触发战斗
            Debug.Log($"⚔️ 触发战斗: {monsterData.cardName}");
            HandleMonsterDrop(monsterData, draggedObject);
        }
        else if (cardData is WeaponCardData weaponData)
        {
            // 武器卡装备
            Debug.Log($"🛡️ 装备武器: {weaponData.cardName}");
            HandleWeaponDrop(weaponData, draggedObject, cardView);
        }
        else if (cardData is ArmorCardData armorData)
        {
            // 盔甲卡装备
            Debug.Log($"🛡️ 装备盔甲: {armorData.cardName}");
            HandleArmorDrop(armorData, draggedObject, cardView);
        }
        else
        {
            // 其他卡牌（材料、食物）放置失败，返回原位
            Debug.LogWarning($"主角槽不支持的卡牌类型: {cardData.cardType}，返回原位");
            if (dragHandler != null) dragHandler.ReturnToOriginalPosition();
        }
    }

    // 处理怪物卡放置：触发战斗
    private void HandleMonsterDrop(MonsterCardData monsterData, GameObject monsterObject)
    {
        if (mainCharacterData == null)
        {
            Debug.LogError("主角数据未设置");
            return;
        }

        // 检查 CombatManager 实例
        if (CombatManager.Instance == null)
        {
            AddCombatLog("错误：战斗管理器未就绪！");
            Debug.LogError("❌ CombatManager 实例为空！");
            return;
        }

        // 执行战斗
        CombatManager.Instance.PerformCombat(monsterData, mainCharacterData, mainCharacterData.equippedWeapon, mainCharacterData.equippedArmor);

        // 更新显示
        UpdateMainCharacterDisplay();

        // 检查怪物死亡
        if (monsterData.health <= 0)
        {
            AddCombatLog($"击败了 {monsterData.cardName}！");
            Destroy(monsterObject);
            // 销毁后，从卡牌管理器中移除引用（如果需要）
        }
        else
        {
            AddCombatLog($"{monsterData.cardName} 存活 (HP: {monsterData.health})");
        }

        // 检查主角死亡
        if (mainCharacterData.health <= 0)
        {
            AddCombatLog("主角被击败！游戏结束！");
            Debug.Log("🎮 主角死亡！触发游戏结束逻辑...");
            // TODO: 触发 Game Over 流程
        }
        else
        {
            // 战斗扣血日志
            AddCombatLog($"主角 HP 剩余: {mainCharacterData.health}");
        }

        // 检查装备耐久度 (CombatManager 应该处理耐久度扣减)
        UpdateWeaponStatusAfterCombat(mainCharacterData.equippedWeapon, mainCharacterData.equippedArmor);
    }

    // 战斗后检查装备状态 (例如：耐久度是否归零)
    private void UpdateWeaponStatusAfterCombat(WeaponCardData weapon, ArmorCardData armor)
    {
        // 检查武器是否损坏
        if (weapon != null && weapon.durability <= 0)
        {
            AddCombatLog($"武器 {weapon.cardName} 损坏！");
            mainCharacterData.equippedWeapon = null;
            // TODO: 销毁场景中的武器卡牌对象
        }
        // 检查盔甲是否损坏
        if (armor != null && armor.durability <= 0)
        {
            AddCombatLog($"盔甲 {armor.cardName} 破碎！");
            mainCharacterData.equippedArmor = null;
            // TODO: 销毁场景中的盔甲卡牌对象
        }
        UpdateMainCharacterDisplay();
    }

    // 处理武器卡放置：装备武器
    private void HandleWeaponDrop(WeaponCardData weaponData, GameObject weaponObject, CardView cardView)
    {
        // 移除旧装备（如果存在）
        if (mainCharacterData.equippedWeapon != null)
        {
            // TODO: 把旧武器卡牌放回背包或销毁（这里暂时只销毁旧的场景对象）
            Debug.Log($"替换旧武器: {mainCharacterData.equippedWeapon.cardName}");
        }

        // 装备新武器
        mainCharacterData.equippedWeapon = weaponData;

        // 放置到主角槽位，并调整位置
        weaponObject.transform.SetParent(transform);
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localScale = Vector3.one;

        // 确保新装备的卡牌不能再次被拖拽
        CardDragHandler dragHandler = weaponObject.GetComponent<CardDragHandler>();
        if (dragHandler != null) Destroy(dragHandler);

        AddCombatLog($" {weaponData.cardName} (+{weaponData.attack} 攻击)");
        UpdateMainCharacterDisplay();
    }

    // 处理盔甲卡放置：装备盔甲
    private void HandleArmorDrop(ArmorCardData armorData, GameObject armorObject, CardView cardView)
    {
        // 移除旧装备（如果存在）
        if (mainCharacterData.equippedArmor != null)
        {
            // TODO: 把旧盔甲卡牌放回背包或销毁
            Debug.Log($"替换旧盔甲: {mainCharacterData.equippedArmor.cardName}");
        }

        // 装备新盔甲
        mainCharacterData.equippedArmor = armorData;

        // 放置到槽位，并调整位置
        armorObject.transform.SetParent(transform);
        armorObject.transform.localPosition = Vector3.zero;
        armorObject.transform.localScale = Vector3.one;

        // 确保新装备的卡牌不能再次被拖拽
        CardDragHandler dragHandler = armorObject.GetComponent<CardDragHandler>();
        if (dragHandler != null) Destroy(dragHandler);

        AddCombatLog($"{armorData.cardName} (+{armorData.cardType} 护甲)");
        UpdateMainCharacterDisplay();
    }

    // 更新主角 UI 显示
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
            WeaponCardData weapon = mainCharacterData.equippedWeapon;
            if (weapon != null)
            {
                weaponStatusText.text = $"武器: {weapon.cardName} ({weapon.durability})";
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
            ArmorCardData armor = mainCharacterData.equippedArmor;
            if (armor != null)
            {
                armorStatusText.text = $"盔甲: {armor.cardName} ({armor.durability})";
                armorStatusText.color = Color.white;
            }
            else
            {
                armorStatusText.text = "盔甲: 无";
                armorStatusText.color = Color.gray;
            }
        }
        // Debug.Log($"📱 UI更新完成 - 血量: {mainCharacterData.health}/{mainCharacterData.maxHealth}");
    }

    // 更新日志逻辑：底部追加，限制行数
    public void AddCombatLog(string logMessage)
    {
        // 1. 将新消息添加到列表末尾 (底部追加)
        combatLogs.Add(logMessage);

        // 2. 检查并移除超出的旧消息
        if (combatLogs.Count > maxLogLines)
        {
            // 如果超出了最大行数，移除列表最前面（最旧）的日志
            combatLogs.RemoveAt(0);
        }

        if (combatLogText != null)
        {
            // 3. 使用 Join 函数将所有消息连接起来，用换行符分隔
            combatLogText.text = string.Join("\n", combatLogs);
        }

        Debug.Log($"📝 战斗日志: {logMessage}");
    }
}
