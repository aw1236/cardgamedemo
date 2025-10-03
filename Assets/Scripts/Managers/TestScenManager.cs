using UnityEngine;
using System.Collections;

public class TestSceneManager : MonoBehaviour
{
    [Header("��������")]
    public Transform cardContainer;
    public int testCardCount = 4;

    private CardArrangement arrangement;

    private void Start()
    {
        // ��ȡ������������
        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        StartCoroutine(SetupTestScene());
    }

    private IEnumerator SetupTestScene()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("��ʼ���ɲ��Կ���...");

        for (int i = 0; i < testCardCount; i++)
        {
            if (CardManager.Instance.allCards.Count > 0)
            {
                CardData randomCard = CardManager.Instance.GetRandomCard();
                GameObject cardObject = CardManager.Instance.CreateCard(randomCard, cardContainer);

                // ������������ӵ�����ϵͳ
                RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                arrangement.AddCard(cardRT);

                yield return new WaitForSeconds(0.1f);
            }
        }

        // �����������п���
        arrangement.ArrangeCardsImmediately();

        Debug.Log($"������ {testCardCount} �Ų��Կ���");
    }
}