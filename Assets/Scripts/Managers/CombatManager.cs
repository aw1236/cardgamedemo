using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("战斗设置")]
    public GameObject combatEffect;  // 战斗特效
    public AudioClip combatSound;    // 战斗音效

    [Header("游戏结束设置")]
    public float gameOverRestartDelay = 3f; // 游戏结束后的重启延迟

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

    /// <summary>
    /// 执行战斗逻辑
    /// </summary>
    public void PerformCombat(MonsterCardData monster, MainCharacterCardData mainChar,
                         WeaponCardData weapon, ArmorCardData armor)
    {
        Debug.Log("🔍 CombatManager.PerformCombat 被调用！");

        if (monster == null)
        {
            Debug.LogError("❌ 怪物数据为空！");
            return;
        }

        if (mainChar == null)
        {
            Debug.LogError("❌ 主角数据为空！");
            return;
        }

        Debug.Log($"✅ 参数检查通过");
        Debug.Log($"📊 怪物: {monster.cardName} (HP:{monster.health}, ATK:{monster.attack})");
        Debug.Log($"📊 主角: {mainChar.cardName} (HP:{mainChar.health})");

        // 计算伤害
        int monsterAttack = monster.attack;
        int characterDefense = armor != null ? armor.defense : 0;
        int actualDamage = Mathf.Max(monsterAttack - characterDefense, 1);

        Debug.Log($"🎯 伤害计算: 怪物攻击{monsterAttack} - 主角防御{characterDefense} = 实际伤害{actualDamage}");

        // 应用伤害到主角
        int previousHealth = mainChar.health;
        mainChar.health -= actualDamage;
        mainChar.health = Mathf.Max(mainChar.health, 0);

        Debug.Log($"❤️ 主角血量: {previousHealth} -> {mainChar.health}");

        // 消耗装备耐久
        if (weapon != null)
        {
            weapon.durability--;
            weapon.durability = Mathf.Max(weapon.durability, 0);
            Debug.Log($"⚔️ 武器耐久: {weapon.durability + 1} -> {weapon.durability}");
        }

        if (armor != null)
        {
            armor.durability--;
            armor.durability = Mathf.Max(armor.durability, 0);
            Debug.Log($"🛡️ 盔甲耐久: {armor.durability + 1} -> {armor.durability}");
        }

        // 怪物承受主角攻击
        int characterAttack = weapon != null ? weapon.attack : mainChar.baseAttack;
        int previousMonsterHealth = monster.health;
        monster.health -= characterAttack;
        monster.health = Mathf.Max(monster.health, 0);

        Debug.Log($"🐺 怪物血量: {previousMonsterHealth} -> {monster.health} (受到{characterAttack}伤害)");

        Debug.Log("✅ 战斗计算完成！");

        // 检查装备损坏
        CheckEquipmentBreak(weapon, armor);

        // 更新UI
        UpdateCombatUI(mainChar, actualDamage);

        // 检查游戏结束
        CheckGameOver(mainChar);
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
    /// 检查装备是否损坏
    /// </summary>
    private void CheckEquipmentBreak(WeaponCardData weapon, ArmorCardData armor)
    {
        if (weapon != null && weapon.durability <= 0)
        {
            Debug.Log($"?? 武器 {weapon.cardName} 已损坏！");
            // 触发武器损坏事件
            OnWeaponBreak(weapon);
        }

        if (armor != null && armor.durability <= 0)
        {
            Debug.Log($"?? 盔甲 {armor.cardName} 已损坏！");
            // 触发盔甲损坏事件
            OnArmorBreak(armor);
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

                // 更新显示
                mainCharSlot.UpdateMainCharacterDisplay();
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

                // 更新显示
                mainCharSlot.UpdateMainCharacterDisplay();
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
    /// 检查游戏是否结束
    /// </summary>
    private void CheckGameOver(MainCharacterCardData mainChar)
    {
        if (mainChar.health <= 0)
        {
            Debug.Log("?? 游戏结束！主角死亡");
            ShowGameOver();
        }
    }

    /// <summary>
    /// 显示游戏结束
    /// </summary>
    private void ShowGameOver()
    {
        Debug.Log("显示游戏结束界面...");

        // 这里可以显示简单的游戏结束信息
        // 在实际项目中，这里会显示UI界面

        // 临时解决方案：在Console显示信息并重启游戏
        Debug.Log("?? 游戏结束！3秒后重新开始...");

        // 延迟后重启场景
        Invoke("RestartGame", gameOverRestartDelay);
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    private void RestartGame()
    {
        Debug.Log("重新开始游戏...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 检查怪物死亡
    /// </summary>
    private void CheckMonsterDeath(MonsterCardData monster)
    {
        if (monster.health <= 0)
        {
            Debug.Log($"?? 怪物 {monster.cardName} 被击败！");

            // 这里可以添加怪物死亡后的逻辑
            // 比如：增加分数、掉落物品等
        }
    }

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


