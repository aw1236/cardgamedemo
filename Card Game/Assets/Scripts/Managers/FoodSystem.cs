using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    public static FoodSystem Instance { get; private set; }

    [Header("食物使用效果")]
    public GameObject healEffectPrefab;
    public AudioClip eatSound;
    public Color healTextColor = Color.green;

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
    /// 使用食物卡片
    /// </summary>
    public bool UseFoodCard(FoodCardData foodCard, MainCharacterCardData targetCharacter)
    {
        if (foodCard == null)
        {
            Debug.LogError("? 食物卡片数据为空！");
            return false;
        }

        if (targetCharacter == null)
        {
            Debug.LogError("? 目标角色为空！");
            return false;
        }

        // 检查角色是否需要治疗
        if (targetCharacter.health >= targetCharacter.maxHealth)
        {
            Debug.Log("?? 角色生命值已满，无法使用食物");
            ShowFloatingText("生命值已满!", targetCharacter);
            return false;
        }

        // 执行治疗
        int previousHealth = targetCharacter.health;
        foodCard.UseFood(targetCharacter);

        // 播放效果
        PlayFoodEffects(targetCharacter, foodCard.healAmount);

        // 显示治疗文本
        ShowHealText(targetCharacter, foodCard.healAmount, previousHealth);

        Debug.Log($"? 成功使用食物: {foodCard.cardName}");
        return true;
    }

    /// <summary>
    /// 播放食物使用效果
    /// </summary>
    private void PlayFoodEffects(MainCharacterCardData targetCharacter, int healAmount)
    {
        // 播放音效
        if (eatSound != null)
        {
            AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);
        }

        // 播放治疗效果
        if (healEffectPrefab != null)
        {
            MainCharacterSlot mainCharSlot = FindObjectOfType<MainCharacterSlot>();
            if (mainCharSlot != null)
            {
                Instantiate(healEffectPrefab, mainCharSlot.transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// 显示治疗文本
    /// </summary>
    private void ShowHealText(MainCharacterCardData targetCharacter, int healAmount, int previousHealth)
    {
        string healText = $"+{healAmount} HP";
        ShowFloatingText(healText, targetCharacter);

        Debug.Log($"?? 治疗完成: {previousHealth} -> {targetCharacter.health}");
    }

    /// <summary>
    /// 显示浮动文字
    /// </summary>
    private void ShowFloatingText(string text, MainCharacterCardData targetCharacter)
    {
        // 这里可以集成你的浮动文字系统
        Debug.Log($"?? {text}");
    }

    /// <summary>
    /// 检查是否可以食用（生命值未满）
    /// </summary>
    public bool CanEatFood(MainCharacterCardData targetCharacter)
    {
        return targetCharacter != null && targetCharacter.health < targetCharacter.maxHealth;
    }

    /// <summary>
    /// 获取治疗描述
    /// </summary>
    public string GetHealDescription(FoodCardData foodCard, MainCharacterCardData targetCharacter)
    {
        if (foodCard == null || targetCharacter == null) return "";

        int missingHealth = targetCharacter.maxHealth - targetCharacter.health;
        int actualHeal = Mathf.Min(foodCard.healAmount, missingHealth);

        return $"恢复 {actualHeal} 点生命值 ({targetCharacter.health}/{targetCharacter.maxHealth} → {targetCharacter.health + actualHeal}/{targetCharacter.maxHealth})";
    }
}