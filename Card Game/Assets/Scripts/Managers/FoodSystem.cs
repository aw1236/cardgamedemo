using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    public static FoodSystem Instance { get; private set; }

    [Header("ʳ��ʹ��Ч��")]
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
    /// ʹ��ʳ�￨Ƭ
    /// </summary>
    public bool UseFoodCard(FoodCardData foodCard, MainCharacterCardData targetCharacter)
    {
        if (foodCard == null)
        {
            Debug.LogError("? ʳ�￨Ƭ����Ϊ�գ�");
            return false;
        }

        if (targetCharacter == null)
        {
            Debug.LogError("? Ŀ���ɫΪ�գ�");
            return false;
        }

        // ����ɫ�Ƿ���Ҫ����
        if (targetCharacter.health >= targetCharacter.maxHealth)
        {
            Debug.Log("?? ��ɫ����ֵ�������޷�ʹ��ʳ��");
            ShowFloatingText("����ֵ����!", targetCharacter);
            return false;
        }

        // ִ������
        int previousHealth = targetCharacter.health;
        foodCard.UseFood(targetCharacter);

        // ����Ч��
        PlayFoodEffects(targetCharacter, foodCard.healAmount);

        // ��ʾ�����ı�
        ShowHealText(targetCharacter, foodCard.healAmount, previousHealth);

        Debug.Log($"? �ɹ�ʹ��ʳ��: {foodCard.cardName}");
        return true;
    }

    /// <summary>
    /// ����ʳ��ʹ��Ч��
    /// </summary>
    private void PlayFoodEffects(MainCharacterCardData targetCharacter, int healAmount)
    {
        // ������Ч
        if (eatSound != null)
        {
            AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);
        }

        // ��������Ч��
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
    /// ��ʾ�����ı�
    /// </summary>
    private void ShowHealText(MainCharacterCardData targetCharacter, int healAmount, int previousHealth)
    {
        string healText = $"+{healAmount} HP";
        ShowFloatingText(healText, targetCharacter);

        Debug.Log($"?? �������: {previousHealth} -> {targetCharacter.health}");
    }

    /// <summary>
    /// ��ʾ��������
    /// </summary>
    private void ShowFloatingText(string text, MainCharacterCardData targetCharacter)
    {
        // ������Լ�����ĸ�������ϵͳ
        Debug.Log($"?? {text}");
    }

    /// <summary>
    /// ����Ƿ����ʳ�ã�����ֵδ����
    /// </summary>
    public bool CanEatFood(MainCharacterCardData targetCharacter)
    {
        return targetCharacter != null && targetCharacter.health < targetCharacter.maxHealth;
    }

    /// <summary>
    /// ��ȡ��������
    /// </summary>
    public string GetHealDescription(FoodCardData foodCard, MainCharacterCardData targetCharacter)
    {
        if (foodCard == null || targetCharacter == null) return "";

        int missingHealth = targetCharacter.maxHealth - targetCharacter.health;
        int actualHeal = Mathf.Min(foodCard.healAmount, missingHealth);

        return $"�ָ� {actualHeal} ������ֵ ({targetCharacter.health}/{targetCharacter.maxHealth} �� {targetCharacter.health + actualHeal}/{targetCharacter.maxHealth})";
    }
}