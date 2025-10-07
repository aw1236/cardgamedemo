using UnityEngine;

public class MonsterHealthController : MonoBehaviour
{
    private int currentHealth;
    private bool isInitialized = false;
    private CardView cardView;

    private void Awake()
    {
        // 获取同一GameObject上的CardView组件
        cardView = GetComponent<CardView>();
        if (cardView == null)
        {
            Debug.LogError("❌ MonsterHealthController: 找不到CardView组件");
        }
    }

    /// <summary>
    /// 初始化或获取怪物血量
    /// </summary>
    public int InitializeOrGetHealth(int baseHealth)
    {
        if (!isInitialized)
        {
            currentHealth = baseHealth;
            isInitialized = true;
            Debug.Log($"🩸 初始化怪物血量: {currentHealth}");

            // 🎯 新增：初始化后立即更新UI
            UpdateMonsterUI();
        }
        return currentHealth;
    }

    /// <summary>
    /// 设置当前血量
    /// </summary>
    public void SetHealth(int health)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, health);
        Debug.Log($"🩸 设置怪物血量: {previousHealth} -> {currentHealth}");

        // 🎯 新增：血量变化时更新UI
        UpdateMonsterUI();
    }

    /// <summary>
    /// 获取当前血量
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 减少血量
    /// </summary>
    public void TakeDamage(int damage)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"💥 怪物受到 {damage} 伤害，剩余血量: {previousHealth} -> {currentHealth}");

        // 🎯 新增：受伤时更新UI
        UpdateMonsterUI();
    }

    /// <summary>
    /// 🎯 新增：更新怪物卡牌UI显示
    /// </summary>
    private void UpdateMonsterUI()
    {
        if (cardView != null)
        {
            cardView.RefreshDisplay();
            Debug.Log($"🔄 更新怪物UI显示: {currentHealth} HP");
        }
        else
        {
            Debug.LogWarning("⚠️ 无法更新怪物UI: CardView为空");

            // 尝试重新获取CardView
            cardView = GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.RefreshDisplay();
                Debug.Log("✅ 重新获取CardView并更新UI");
            }
        }
    }

    /// <summary>
    /// 🎯 新增：强制刷新UI（用于外部调用）
    /// </summary>
    public void ForceRefreshUI()
    {
        UpdateMonsterUI();
    }
}