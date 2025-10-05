using UnityEngine;

public class MonsterHealthController : MonoBehaviour
{
    private int currentHealth;
    private bool isInitialized = false;

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
        }
        return currentHealth;
    }

    /// <summary>
    /// 设置当前血量
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Max(0, health);
        Debug.Log($"🩸 设置怪物血量: {currentHealth}");
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
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"💥 怪物受到 {damage} 伤害，剩余血量: {currentHealth}");
    }
}