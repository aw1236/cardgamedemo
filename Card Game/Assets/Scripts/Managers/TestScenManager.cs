using UnityEngine;
using System.Collections;

public class TestSceneManager : MonoBehaviour
{
    [Header("测试参数")]
    public Transform cardContainer;
    public int testCardCount = 4;
    [SerializeField] private int _maxWaves = 10; // 改为序列化字段
    public float checkInterval = 1.0f; // 检查间隔

    [Header("Boss卡设置")]
    public CardData bossCard; // 拖入bearKing.asset到这里

    private CardArrangement arrangement;
    private int currentWave = 0;
    private bool isChecking = false;

    // 添加属性以确保值正确
    public int maxWaves
    {
        get { return _maxWaves; }
        set { _maxWaves = value; }
    }

    private void Start()
    {
        // 强制设置最大波数为10
        _maxWaves = 10;
        Debug.Log($"设置最大波数为: {_maxWaves}");

        // 获取或添加卡牌排列组件
        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        // 开始第一波卡牌
        StartCoroutine(SetupTestScene());
    }

    private IEnumerator SetupTestScene()
    {
        yield return new WaitForSeconds(0.5f);

        // 确保所有旧卡牌被清理
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        // 清空排列系统
        arrangement.ClearAllCards();

        // 等待一帧确保清理完成
        yield return null;

        Debug.Log($"当前波数: {currentWave}, 最大波数: {_maxWaves}");

        if (currentWave < _maxWaves)
        {
            Debug.Log($"开始生成第 {currentWave + 1} 波测试卡牌...");

            // 检查是否是最后一波并且有boss卡
            bool isLastWave = (currentWave + 1 == _maxWaves);
            bool hasBossCard = bossCard != null;

            for (int i = 0; i < testCardCount; i++)
            {
                CardData cardToCreate;

                // 如果是最后一波、有boss卡，并且是第一张卡，则创建boss卡
                if (isLastWave && hasBossCard && i == 0)
                {
                    cardToCreate = bossCard;
                    Debug.Log("生成BOSS卡!");
                }
                else if (CardManager.Instance.allCards.Count > 0)
                {
                    cardToCreate = CardManager.Instance.GetRandomCard();
                }
                else
                {
                    // 如果没有可用卡牌，跳过
                    continue;
                }

                GameObject cardObject = CardManager.Instance.CreateCard(cardToCreate, cardContainer);

                // 将卡牌添加到排列系统
                RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                arrangement.AddCard(cardRT);

                yield return new WaitForSeconds(0.1f);
            }

            // 立即排列卡牌
            arrangement.ArrangeCardsImmediately();

            currentWave++;
            Debug.Log($"完成了第 {currentWave} 波，共 {testCardCount} 张测试卡牌");

            // 开始检查是否刷新下一波
            if (currentWave < _maxWaves)
            {
                StartCoroutine(CheckForNextWave());
            }
            else
            {
                Debug.Log($"已达到最大波数上限: {_maxWaves}");
            }
        }
    }

    private IEnumerator CheckForNextWave()
    {
        if (isChecking) yield break;

        isChecking = true;
        Debug.Log("开始监测卡牌槽状态...");

        while (currentWave < _maxWaves)
        {
            yield return new WaitForSeconds(checkInterval);

            if (AreAllSlotsEmpty())
            {
                Debug.Log("所有卡牌槽为空，开始下一波刷新");
                StartCoroutine(SetupTestScene());
                break;
            }
        }

        isChecking = false;
    }

    private bool AreAllSlotsEmpty()
    {
        if (arrangement != null)
        {
            return arrangement.IsContainerEmpty();
        }

        return cardContainer.childCount == 0;
    }

    // 可选：手动触发下一波刷新（用于调试）
    [ContextMenu("手动触发下一波")]
    public void ManualTriggerNextWave()
    {
        if (currentWave < _maxWaves && !isChecking)
        {
            StartCoroutine(SetupTestScene());
        }
    }

    // 可选：重置波数计数
    [ContextMenu("重置波数")]
    public void ResetWaves()
    {
        currentWave = 0;
        isChecking = false;
        StopAllCoroutines();
        Debug.Log("波数计数器已重置");
    }

    // 添加一个方法来强制设置波数
    [ContextMenu("强制设置波数为10")]
    public void ForceSetWavesTo10()
    {
        _maxWaves = 10;
        Debug.Log($"已强制设置最大波数为: {_maxWaves}");
    }
}