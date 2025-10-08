using UnityEngine;
using UnityEngine.SceneManagement;
using static EquipmentSlot;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("战斗设置")]
    public GameObject combatEffect;  // 战斗特效
    public AudioClip combatSound;    // 战斗音效

    [Header("游戏结束设置")]
    public float gameOverRestartDelay = 3f; // 游戏结束后的重启延迟

    // 🎯 新增：最终BOSS名称
    private const string FINAL_BOSS_NAME = "狗熊王";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log("=== 场景检查开始 ===");

        // 检查所有场景
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"场景 [{i}]: {sceneName}");

            // 特别标记结束场景
            if (sceneName == "game win scene" || sceneName == "game lose scene")
            {
                Debug.Log($"⭐ 找到结束场景: {sceneName} 在索引 {i}");
            }
        }

        Debug.Log("=== 场景检查结束 ===");
    }
    private void CheckScenesInBuild()
    {
        Debug.Log("🔍 检查构建设置中的场景:");

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  场景 [{i}]: {sceneName}");
        }

        // 检查特定场景
        CheckSceneExists("game win scene");
        CheckSceneExists("game lose scene");
    }

    private void CheckSceneExists(string sceneName)
    {
        bool exists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string nameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (nameInBuild == sceneName)
            {
                exists = true;
                break;
            }
        }
        Debug.Log($"场景 '{sceneName}' {(exists ? "✅ 存在" : "❌ 不存在")}");
    }

    /// <summary>
    /// 执行战斗逻辑
    /// </summary>
    public void PerformCombat(MonsterCardData monster, MainCharacterCardData mainChar, GameObject monsterObject = null, MonsterHealthController healthController = null)
    {
        Debug.Log("CombatManager.PerformCombat 被调用！");

        if (monster == null)
        {
            Debug.LogError(" 怪物数据为空！");
            return;
        }

        if (mainChar == null)
        {
            Debug.LogError("主角数据为空！");
            return;
        }

        // 🎯 获取怪物当前血量
        int monsterCurrentHealth;
        if (healthController != null)
        {
            monsterCurrentHealth = healthController.GetCurrentHealth();
        }
        else
        {
            // 备用方案：使用怪物数据的基础血量
            monsterCurrentHealth = monster.health;
        }

        Debug.Log($"参数检查通过");
        Debug.Log($"怪物: {monster.cardName} (HP:{monsterCurrentHealth}, ATK:{monster.attack})");
        Debug.Log($"主角: {mainChar.cardName} (HP:{mainChar.health})");

        // 在 PerformCombat 方法中，确保使用主角数据中的装备
        WeaponCardData currentWeapon = mainChar.equippedWeapon;
        ArmorCardData currentArmor = mainChar.equippedArmor;

        // 计算伤害
        int monsterAttack = monster.attack;
        int characterDefense = currentArmor != null ? currentArmor.defense : 0;
        int actualDamage = Mathf.Max(monsterAttack - characterDefense, 0);

        Debug.Log($"伤害计算: 怪物攻击{monsterAttack} - 主角防御{characterDefense} = 实际伤害{actualDamage}");

        // 应用伤害到主角
        int previousHealth = mainChar.health;
        mainChar.health -= actualDamage;
        mainChar.health = Mathf.Max(mainChar.health, 0);

        Debug.Log($"主角血量: {previousHealth} -> {mainChar.health}");

        // 消耗装备耐久
        if (currentWeapon != null)
        {
            int previousDurability = currentWeapon.durability;
            currentWeapon.durability--;
            currentWeapon.durability = Mathf.Max(currentWeapon.durability, 0);
            Debug.Log($"武器耐久: {previousDurability} -> {currentWeapon.durability}");

            // 🎯 新增：立即更新武器UI显示
            UpdateWeaponUI(currentWeapon);

            // 🎯 检查武器是否损坏
            if (currentWeapon.durability <= 0)
            {
                Debug.Log($"武器 {currentWeapon.cardName} 已损坏！");
                OnWeaponBreak(currentWeapon);
            }
        }

        if (currentArmor != null)
        {
            int previousDurability = currentArmor.durability;
            currentArmor.durability--;
            currentArmor.durability = Mathf.Max(currentArmor.durability, 0);
            Debug.Log($" 盔甲耐久: {previousDurability} -> {currentArmor.durability}");

            // 🎯 新增：立即更新盔甲UI显示
            UpdateArmorUI(currentArmor);

            // 🎯 检查盔甲是否损坏
            if (currentArmor.durability <= 0)
            {
                Debug.Log($"盔甲 {currentArmor.cardName} 已损坏！");
                OnArmorBreak(currentArmor);
            }
        }

        // 怪物承受主角攻击
        int characterAttack = currentWeapon != null ? currentWeapon.attack : mainChar.baseAttack;
        int previousMonsterHealth = monsterCurrentHealth;
        int newMonsterHealth = previousMonsterHealth - characterAttack;
        newMonsterHealth = Mathf.Max(newMonsterHealth, 0);

        // 🎯 设置怪物新血量
        if (healthController != null)
        {
            healthController.SetHealth(newMonsterHealth);

            // 🎯 新增：确保怪物UI更新
            healthController.ForceRefreshUI();
            Debug.Log($"强制刷新怪物UI: {newMonsterHealth} HP");
        }

        Debug.Log($" 怪物血量: {previousMonsterHealth} -> {newMonsterHealth} (受到{characterAttack}伤害)");

        Debug.Log("战斗计算完成！");

        // 播放战斗效果
        PlayCombatEffects();

        // 更新UI
        UpdateCombatUI(mainChar, actualDamage);

        // 🎯 修改：检查游戏结束或胜利
        CheckGameResult(mainChar, monster, newMonsterHealth);
    }

    /// <summary>
    /// 🎯 新增：检查游戏结果（胜利或失败）
    /// </summary>
    private void CheckGameResult(MainCharacterCardData mainChar, MonsterCardData monster, int monsterHealthAfter)
    {
        // 检查游戏失败（主角死亡）
        if (mainChar.health <= 0)
        {
            Debug.Log("游戏结束！主角死亡");
            ShowGameOver(false); // 失败
            return;
        }

        // 🎯 新增：检查游戏胜利（击败狗熊王且主角存活）
        if (monster.cardName == FINAL_BOSS_NAME && monsterHealthAfter <= 0 && mainChar.health > 0)
        {
            Debug.Log($"击败最终BOSS {FINAL_BOSS_NAME}！游戏胜利！");
            ShowGameOver(true); // 胜利
            return;
        }
    }

    /// <summary>
    /// 🎯 新增：更新武器UI显示
    /// </summary>
    private void UpdateWeaponUI(WeaponCardData weaponData)
    {
        // 查找所有装备槽
        EquipmentSlot[] equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.equipmentType == EquipmentType.Weapon && slot.CurrentCardView != null)
            {
                CardData slotCardData = slot.CurrentCardView.GetCardData();
                if (slotCardData == weaponData) // 引用比较
                {
                    Debug.Log($"更新武器UI显示: {weaponData.cardName} (耐久:{weaponData.durability})");
                    slot.CurrentCardView.RefreshDisplay();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 🎯 新增：更新盔甲UI显示
    /// </summary>
    private void UpdateArmorUI(ArmorCardData armorData)
    {
        // 查找所有装备槽
        EquipmentSlot[] equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.equipmentType == EquipmentType.Armor && slot.CurrentCardView != null)
            {
                CardData slotCardData = slot.CurrentCardView.GetCardData();
                if (slotCardData == armorData) // 引用比较
                {
                    Debug.Log($"更新盔甲UI显示: {armorData.cardName} (耐久:{armorData.durability})");
                    slot.CurrentCardView.RefreshDisplay();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 播放战斗特效和音效
    /// </summary>
    private void PlayCombatEffects()
    {
        // 战斗特效
        if (combatEffect != null)
        {
            Instantiate(combatEffect, Vector3.zero, Quaternion.identity);
        }

        // 战斗音效
        if (combatSound != null)
        {
            AudioSource.PlayClipAtPoint(combatSound, Camera.main.transform.position);
        }
    }

    /// <summary>
    /// 武器损坏处理
    /// </summary>
    private void OnWeaponBreak(WeaponCardData weapon)
    {
        // 查找主角槽并移除损坏的武器
        MainCharacterSlot mainCharSlot = FindObjectOfType<MainCharacterSlot>();
        if (mainCharSlot != null && mainCharSlot.mainCharacterData != null)
        {
            if (mainCharSlot.mainCharacterData.equippedWeapon == weapon)
            {
                mainCharSlot.mainCharacterData.equippedWeapon = null;
                Debug.Log("已自动卸下损坏的武器");

                // 🎯 新增：查找并销毁武器槽中的卡牌
                DestroyWeaponCardInSlot(weapon);

                // 更新显示
                mainCharSlot.UpdateMainCharacterDisplay();
                mainCharSlot.AddCombatLog($"{weapon.cardName} 已损坏！");
            }
        }
    }

    /// <summary>
    /// 盔甲损坏处理
    /// </summary>
    private void OnArmorBreak(ArmorCardData armor)
    {
        // 查找主角槽并移除损坏的盔甲
        MainCharacterSlot mainCharSlot = FindObjectOfType<MainCharacterSlot>();
        if (mainCharSlot != null && mainCharSlot.mainCharacterData != null)
        {
            if (mainCharSlot.mainCharacterData.equippedArmor == armor)
            {
                mainCharSlot.mainCharacterData.equippedArmor = null;
                Debug.Log("已自动卸下损坏的盔甲");

                // 🎯 新增：查找并销毁盔甲槽中的卡牌
                DestroyArmorCardInSlot(armor);

                // 更新显示
                mainCharSlot.UpdateMainCharacterDisplay();
                mainCharSlot.AddCombatLog($"{armor.cardName} 已损坏！");
            }
        }
    }

    /// <summary>
    /// 🎯 新增：销毁武器槽中的卡牌
    /// </summary>
    private void DestroyWeaponCardInSlot(WeaponCardData weaponData)
    {
        // 查找所有装备槽
        EquipmentSlot[] equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.equipmentType == EquipmentType.Weapon && slot.CurrentCardView != null)
            {
                CardData slotCardData = slot.CurrentCardView.GetCardData();
                if (slotCardData == weaponData) // 引用比较
                {
                    Debug.Log($"销毁损坏的武器卡牌: {weaponData.cardName}");
                    slot.ForceRemoveAndDestroy(); // 调用新增的销毁方法
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 🎯 新增：销毁盔甲槽中的卡牌
    /// </summary>
    private void DestroyArmorCardInSlot(ArmorCardData armorData)
    {
        // 查找所有装备槽
        EquipmentSlot[] equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.equipmentType == EquipmentType.Armor && slot.CurrentCardView != null)
            {
                CardData slotCardData = slot.CurrentCardView.GetCardData();
                if (slotCardData == armorData) // 引用比较
                {
                    Debug.Log($"销毁损坏的盔甲卡牌: {armorData.cardName}");
                    slot.ForceRemoveAndDestroy(); // 调用新增的销毁方法
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 更新战斗后的UI显示
    /// </summary>
    private void UpdateCombatUI(MainCharacterCardData mainChar, int damageTaken)
    {
        // 更新主角显示
        MainCharacterSlot mainCharSlot = FindObjectOfType<MainCharacterSlot>();
        if (mainCharSlot != null)
        {
            mainCharSlot.UpdateMainCharacterDisplay();
        }

        Debug.Log("战斗UI已更新");
    }

    /// <summary>
    /// 🎯 修改：显示游戏结束（现在支持胜利和失败）
    /// </summary>
    private void ShowGameOver(bool isWin)
    {
        Debug.Log($"显示游戏结束界面: {(isWin ? "胜利" : "失败")}");

        // 🎯 方法1：使用场景索引
        if (isWin)
        {
            SceneManager.LoadScene(6); // game win scene 的索引
        }
        else
        {
            SceneManager.LoadScene(7); // game lose scene 的索引
        }
    }

    /// <summary>
    /// 🎯 移除旧的 CheckGameOver 方法，因为已经被 CheckGameResult 替代
    /// </summary>
    // private void CheckGameOver(MainCharacterCardData mainChar)
    // {
    //     // 这个方法已经被 CheckGameResult 替代
    // }

    /// <summary>
    /// 🎯 移除旧的 ShowGameOver 方法，因为已经被新的重载版本替代
    /// </summary>
    // private void ShowGameOver()
    // {
    //     // 这个方法已经被新的 ShowGameOver(bool isWin) 替代
    // }

    /// <summary>
    /// 🎯 移除旧的 RestartGame 方法，因为现在直接跳转到结束场景
    /// </summary>
    // private void RestartGame()
    // {
    //     // 这个方法不再需要，因为直接跳转到结束场景
    // }

    /// <summary>
    /// 预检查是否可以战斗
    /// </summary>
    public bool CanFight(CardData card1, CardData card2)
    {
        if (card1 == null || card2 == null) return false;

        bool hasMonster = card1 is MonsterCardData || card2 is MonsterCardData;
        bool hasMainChar = card1.cardType == CardType.MainCharacter ||
                          card2.cardType == CardType.MainCharacter;

        return hasMonster && hasMainChar;
    }

    /// <summary>
    /// 获取战斗结果描述
    /// </summary>
    public string GetCombatResultDescription(MonsterCardData monster, MainCharacterCardData mainChar)
    {
        if (monster.health <= 0)
        {
            return $"击败了 {monster.cardName}！";
        }
        else if (mainChar.health <= 0)
        {
            return "主角被击败了...";
        }
        else
        {
            return $"战斗继续...";
        }
    }

    /// <summary>
    /// 治疗主角
    /// </summary>
    public void HealMainCharacter(MainCharacterCardData mainChar, int healAmount)
    {
        if (mainChar == null) return;

        int previousHealth = mainChar.health;
        mainChar.health += healAmount;
        mainChar.health = Mathf.Min(mainChar.health, mainChar.maxHealth); // 不超过最大血量

        Debug.Log($"治疗主角: HP {previousHealth} -> {mainChar.health} (+{healAmount})");

        // 更新UI
        UpdateCombatUI(mainChar, 0);
    }

    /// <summary>
    /// 重置战斗管理器（用于新游戏）
    /// </summary>
    public void ResetCombatManager()
    {
        Debug.Log("战斗管理器已重置");
    }

}