using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestSceneManager : MonoBehaviour
{
    [Header("测试参数")]
    public Transform cardContainer;
    public int testCardCount = 4;
    [SerializeField] private int _maxWaves = 10;

    [Header("UI设置")]
    public Button nextWaveButton;

    [Header("Boss卡设置")]
    public CardData bossCard;

    private CardArrangement arrangement;
    public int currentWave = 0;
    private bool waveInProgress = false;

    public int maxWaves
    {
        get { return _maxWaves; }
        set { _maxWaves = value; }
    }

    private void Start()
    {
        _maxWaves = 10;
        Debug.Log($"设置最大波数为: {_maxWaves}");

        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        // 标记为更新槽
        arrangement.isUpdateSlot = true;

        // 设置按钮事件
        if (nextWaveButton != null)
        {
            nextWaveButton.onClick.AddListener(StartNextWave);
        }

        // 开始检查更新槽状态
        StartCoroutine(CheckUpdateSlotStatus());

        Debug.Log("等待更新槽清空后才能开始下一波...");

        // 新增：自动开始第一波
        StartCoroutine(AutoStartFirstWave());
    }

    // 新增：自动开始第一波的协程
    private IEnumerator AutoStartFirstWave()
    {
        // 等待一帧确保所有组件初始化完成
        yield return new WaitForEndOfFrame();

        // 检查是否应该自动开始第一波
        var configManager = WaveConfigManager.Instance;
        bool shouldAutoStart = configManager == null || configManager.autoStartFirstWave;

        if (shouldAutoStart && currentWave == 0 && IsUpdateSlotEmpty() && !waveInProgress)
        {
            Debug.Log("自动开始第一波...");
            StartNextWave();
        }
    }

    // 定期检查更新槽状态
    private IEnumerator CheckUpdateSlotStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // 每0.5秒检查一次

            UpdateButtonState();
        }
    }

    // 手动开始下一波
    public void StartNextWave()
    {
        // 只有在更新槽为空且没有波次进行中且未达到最大波数时才能开始
        if (IsUpdateSlotEmpty() && currentWave < _maxWaves && !waveInProgress)
        {
            StartCoroutine(SetupTestScene());
        }
        else
        {
            if (!IsUpdateSlotEmpty())
            {
                Debug.Log("更新槽还未清空，无法开始下一波");
            }
            else if (waveInProgress)
            {
                Debug.Log("波次正在进行中，请等待");
            }
            else if (currentWave >= _maxWaves)
            {
                Debug.Log("已达到最大波数");
            }
        }
    }

    private IEnumerator SetupTestScene()
    {
        waveInProgress = true;
        UpdateButtonState();

        yield return new WaitForSeconds(0.5f);

        // 保存当前的自动排列设置并暂时禁用
        bool wasAutoArrange = arrangement.autoArrange;
        arrangement.autoArrange = false;

        // 清理旧卡牌
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        arrangement.ClearAllCards();
        yield return null;

        Debug.Log($"开始生成第 {currentWave + 1} 波测试卡牌...");

        // 新增：获取波次配置
        WaveConfig currentConfig = null;
        var configManager = WaveConfigManager.Instance;
        if (configManager != null && configManager.HasWaveConfig(currentWave + 1))
        {
            currentConfig = configManager.GetWaveConfig(currentWave + 1);
            Debug.Log($"使用波次 {currentWave + 1} 的配置");
        }

        // 确定要生成的卡牌数量和类型
        int cardsToGenerate = currentConfig != null ? currentConfig.cardCount : testCardCount;
        bool isLastWave = (currentWave + 1 == _maxWaves);
        bool hasBossCard = bossCard != null;

        // 新增：检查配置中的BOSS卡
        bool spawnBossThisWave = currentConfig != null ? currentConfig.spawnBossThisWave : false;
        CardData bossCardToUse = currentConfig != null && currentConfig.bossCard != null ? currentConfig.bossCard : bossCard;

        int cardsGenerated = 0;
        for (int i = 0; i < cardsToGenerate; i++)
        {
            CardData cardToCreate = null;

            // 优先使用配置中的特定卡牌
            if (currentConfig != null && currentConfig.specificCards.Count > i)
            {
                cardToCreate = currentConfig.specificCards[i];
                Debug.Log($"使用配置的特定卡牌: {cardToCreate.cardName}");
            }
            // 然后是BOSS卡逻辑
            else if ((isLastWave && hasBossCard && i == 0) || (spawnBossThisWave && i == 0 && bossCardToUse != null))
            {
                cardToCreate = bossCardToUse;
                Debug.Log("生成BOSS卡!");
            }
            // 然后是配置中的卡牌类型
            else if (currentConfig != null && currentConfig.cardTypesToSpawn.Count > 0)
            {
                CardType typeToSpawn = currentConfig.cardTypesToSpawn[i % currentConfig.cardTypesToSpawn.Count];
                var cardsOfType = CardManager.Instance.GetCardsByType(typeToSpawn);
                if (cardsOfType.Count > 0)
                {
                    cardToCreate = cardsOfType[Random.Range(0, cardsOfType.Count)];
                    Debug.Log($"生成类型 {typeToSpawn} 的卡牌: {cardToCreate.cardName}");
                }
            }
            // 最后是随机卡牌
            else if (CardManager.Instance != null && CardManager.Instance.allCards.Count > 0)
            {
                cardToCreate = CardManager.Instance.GetRandomCard();
            }
            else
            {
                continue;
            }

            if (cardToCreate != null)
            {
                GameObject cardObject = CardManager.Instance.CreateCard(cardToCreate, cardContainer);
                if (cardObject != null)
                {
                    RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                    arrangement.AddCard(cardRT);
                    cardsGenerated++;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        // 所有卡牌生成完成后，一次性排列
        arrangement.ArrangeCardsImmediately();

        // 恢复原来的自动排列设置
        arrangement.autoArrange = wasAutoArrange;

        currentWave++;

        Debug.Log($"完成了第 {currentWave} 波，生成了 {cardsGenerated} 张测试卡牌");

        waveInProgress = false;
        UpdateButtonState();

        if (currentWave >= _maxWaves)
        {
            Debug.Log($"已达到最大波数上限: {_maxWaves}");
        }
    }

    // 检查更新槽是否为空
    private bool IsUpdateSlotEmpty()
    {
        if (arrangement != null)
        {
            return arrangement.IsContainerActuallyEmpty(); // 使用新的更准确的方法
        }

        // 备用检查方法
        return cardContainer.childCount == 0;
    }

    // 更新按钮状态
    private void UpdateButtonState()
    {
        if (nextWaveButton != null)
        {
            bool isSlotEmpty = IsUpdateSlotEmpty();
            bool canStartNextWave = isSlotEmpty && currentWave < _maxWaves && !waveInProgress;

            nextWaveButton.interactable = canStartNextWave;

            // 更新按钮文本
            Text buttonText = nextWaveButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (waveInProgress)
                {
                    buttonText.text = "生成中...";
                }
                else if (!isSlotEmpty)
                {
                    buttonText.text = "清空更新槽";
                    // 可以添加提示颜色
                    buttonText.color = Color.gray;
                }
                else if (currentWave < _maxWaves)
                {
                    buttonText.text = $"下一波 ({currentWave + 1}/{_maxWaves})";
                    buttonText.color = Color.white;
                }
                else
                {
                    buttonText.text = "已完成";
                    buttonText.color = Color.gray;
                }
            }

            // 添加提示文本
            if (!isSlotEmpty && !waveInProgress)
            {
                int remainingCards = arrangement.GetCardsInContainerCount();
            }
        }
    }

    [ContextMenu("手动触发下一波")]
    public void ManualTriggerNextWave()
    {
        StartNextWave();
    }

    [ContextMenu("重置波数")]
    public void ResetWaves()
    {
        currentWave = 0;
        waveInProgress = false;
        StopAllCoroutines();

        // 清理所有卡牌
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
        arrangement.ClearAllCards();

        // 重新开始状态检查
        StartCoroutine(CheckUpdateSlotStatus());

        UpdateButtonState();
        Debug.Log("波数计数器已重置");

        // 重置后自动开始第一波
        StartCoroutine(AutoStartFirstWave());
    }

    [ContextMenu("强制设置波数为10")]
    public void ForceSetWavesTo10()
    {
        _maxWaves = 10;
        Debug.Log($"已强制设置最大波数为: {_maxWaves}");
        UpdateButtonState();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}