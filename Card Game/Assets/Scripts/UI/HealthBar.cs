using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("UI引用")]
    public TextMeshProUGUI healthText;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"血量: {currentHealth}/{maxHealth}";

            // 根据血量改变颜色
            if (currentHealth <= maxHealth * 0.3f)
                healthText.color = Color.red;
            else if (currentHealth <= maxHealth * 0.6f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.green;
        }
    }
}
