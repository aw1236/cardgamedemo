using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("UI����")]
    public TextMeshProUGUI healthText;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Ѫ��: {currentHealth}/{maxHealth}";

            // ����Ѫ���ı���ɫ
            if (currentHealth <= maxHealth * 0.3f)
                healthText.color = Color.red;
            else if (currentHealth <= maxHealth * 0.6f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.green;
        }
    }
}
