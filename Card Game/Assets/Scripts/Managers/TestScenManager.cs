using UnityEngine;
using System.Collections;

public class TestSceneManager : MonoBehaviour
{
    [Header("测试设置")]
    public Transform cardContainer;
    public int testCardCount = 4;

    private CardArrangement arrangement;

    private void Start()
    {
        // 获取或添加排列组件
        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        StartCoroutine(SetupTestScene());
    }

    private IEnumerator SetupTestScene()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("开始生成测试卡牌...");

        for (int i = 0; i < testCardCount; i++)
        {
            if (CardManager.Instance.allCards.Count > 0)
            {
                CardData randomCard = CardManager.Instance.GetRandomCard();
                GameObject cardObject = CardManager.Instance.CreateCard(randomCard, cardContainer);

                // 立即将卡牌添加到排列系统
                RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                arrangement.AddCard(cardRT);

                yield return new WaitForSeconds(0.1f);
            }
        }

        // 立即排列所有卡牌
        arrangement.ArrangeCardsImmediately();

        Debug.Log($"生成了 {testCardCount} 张测试卡牌");
    }
}