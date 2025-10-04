using UnityEngine;
using System.Collections;

public class TestSceneManager : MonoBehaviour
{
    [Header("���Բ���")]
    public Transform cardContainer;
    public int testCardCount = 4;
    [SerializeField] private int _maxWaves = 10; // ��Ϊ���л��ֶ�
    public float checkInterval = 1.0f; // �����

    [Header("Boss������")]
    public CardData bossCard; // ����bearKing.asset������

    private CardArrangement arrangement;
    private int currentWave = 0;
    private bool isChecking = false;

    // ���������ȷ��ֵ��ȷ
    public int maxWaves
    {
        get { return _maxWaves; }
        set { _maxWaves = value; }
    }

    private void Start()
    {
        // ǿ�����������Ϊ10
        _maxWaves = 10;
        Debug.Log($"���������Ϊ: {_maxWaves}");

        // ��ȡ����ӿ����������
        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        // ��ʼ��һ������
        StartCoroutine(SetupTestScene());
    }

    private IEnumerator SetupTestScene()
    {
        yield return new WaitForSeconds(0.5f);

        // ȷ�����оɿ��Ʊ�����
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        // �������ϵͳ
        arrangement.ClearAllCards();

        // �ȴ�һ֡ȷ���������
        yield return null;

        Debug.Log($"��ǰ����: {currentWave}, �����: {_maxWaves}");

        if (currentWave < _maxWaves)
        {
            Debug.Log($"��ʼ���ɵ� {currentWave + 1} �����Կ���...");

            // ����Ƿ������һ��������boss��
            bool isLastWave = (currentWave + 1 == _maxWaves);
            bool hasBossCard = bossCard != null;

            for (int i = 0; i < testCardCount; i++)
            {
                CardData cardToCreate;

                // ��������һ������boss���������ǵ�һ�ſ����򴴽�boss��
                if (isLastWave && hasBossCard && i == 0)
                {
                    cardToCreate = bossCard;
                    Debug.Log("����BOSS��!");
                }
                else if (CardManager.Instance.allCards.Count > 0)
                {
                    cardToCreate = CardManager.Instance.GetRandomCard();
                }
                else
                {
                    // ���û�п��ÿ��ƣ�����
                    continue;
                }

                GameObject cardObject = CardManager.Instance.CreateCard(cardToCreate, cardContainer);

                // ��������ӵ�����ϵͳ
                RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                arrangement.AddCard(cardRT);

                yield return new WaitForSeconds(0.1f);
            }

            // �������п���
            arrangement.ArrangeCardsImmediately();

            currentWave++;
            Debug.Log($"����˵� {currentWave} ������ {testCardCount} �Ų��Կ���");

            // ��ʼ����Ƿ�ˢ����һ��
            if (currentWave < _maxWaves)
            {
                StartCoroutine(CheckForNextWave());
            }
            else
            {
                Debug.Log($"�Ѵﵽ���������: {_maxWaves}");
            }
        }
    }

    private IEnumerator CheckForNextWave()
    {
        if (isChecking) yield break;

        isChecking = true;
        Debug.Log("��ʼ��⿨�Ʋ�״̬...");

        while (currentWave < _maxWaves)
        {
            yield return new WaitForSeconds(checkInterval);

            if (AreAllSlotsEmpty())
            {
                Debug.Log("���п��Ʋ�Ϊ�գ���ʼ��һ��ˢ��");
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

    // ��ѡ���ֶ�������һ��ˢ�£����ڵ��ԣ�
    [ContextMenu("�ֶ�������һ��")]
    public void ManualTriggerNextWave()
    {
        if (currentWave < _maxWaves && !isChecking)
        {
            StartCoroutine(SetupTestScene());
        }
    }

    // ��ѡ�����ò�������
    [ContextMenu("���ò���")]
    public void ResetWaves()
    {
        currentWave = 0;
        isChecking = false;
        StopAllCoroutines();
        Debug.Log("����������������");
    }

    // ���һ��������ǿ�����ò���
    [ContextMenu("ǿ�����ò���Ϊ10")]
    public void ForceSetWavesTo10()
    {
        _maxWaves = 10;
        Debug.Log($"��ǿ�����������Ϊ: {_maxWaves}");
    }
}