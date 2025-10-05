using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestSceneManager : MonoBehaviour
{
    [Header("���Բ���")]
    public Transform cardContainer;
    public int testCardCount = 4;
    [SerializeField] private int _maxWaves = 10;

    [Header("UI����")]
    public Button nextWaveButton;

    [Header("Boss������")]
    public CardData bossCard;

    private CardArrangement arrangement;
    private int currentWave = 0;
    private bool waveInProgress = false;

    public int maxWaves
    {
        get { return _maxWaves; }
        set { _maxWaves = value; }
    }

    private void Start()
    {
        _maxWaves = 10;
        Debug.Log($"���������Ϊ: {_maxWaves}");

        arrangement = cardContainer.GetComponent<CardArrangement>();
        if (arrangement == null)
            arrangement = cardContainer.gameObject.AddComponent<CardArrangement>();

        // ���Ϊ���²�
        arrangement.isUpdateSlot = true;

        // ���ð�ť�¼�
        if (nextWaveButton != null)
        {
            nextWaveButton.onClick.AddListener(StartNextWave);
        }

        // ��ʼ�����²�״̬
        StartCoroutine(CheckUpdateSlotStatus());

        Debug.Log("�ȴ����²���պ���ܿ�ʼ��һ��...");
    }

    // ���ڼ����²�״̬
    private IEnumerator CheckUpdateSlotStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // ÿ0.5����һ��

            UpdateButtonState();
        }
    }

    // �ֶ���ʼ��һ��
    public void StartNextWave()
    {
        // ֻ���ڸ��²�Ϊ����û�в��ν�������δ�ﵽ�����ʱ���ܿ�ʼ
        if (IsUpdateSlotEmpty() && currentWave < _maxWaves && !waveInProgress)
        {
            StartCoroutine(SetupTestScene());
        }
        else
        {
            if (!IsUpdateSlotEmpty())
            {
                Debug.Log("���²ۻ�δ��գ��޷���ʼ��һ��");
            }
            else if (waveInProgress)
            {
                Debug.Log("�������ڽ����У���ȴ�");
            }
            else if (currentWave >= _maxWaves)
            {
                Debug.Log("�Ѵﵽ�����");
            }
        }
    }

    private IEnumerator SetupTestScene()
    {
        waveInProgress = true;
        UpdateButtonState();

        yield return new WaitForSeconds(0.5f);

        // ���浱ǰ���Զ��������ò���ʱ����
        bool wasAutoArrange = arrangement.autoArrange;
        arrangement.autoArrange = false;

        // ����ɿ���
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        arrangement.ClearAllCards();
        yield return null;

        Debug.Log($"��ʼ���ɵ� {currentWave + 1} �����Կ���...");

        bool isLastWave = (currentWave + 1 == _maxWaves);
        bool hasBossCard = bossCard != null;

        int cardsGenerated = 0;
        for (int i = 0; i < testCardCount; i++)
        {
            CardData cardToCreate;

            if (isLastWave && hasBossCard && i == 0)
            {
                cardToCreate = bossCard;
                Debug.Log("����BOSS��!");
            }
            else if (CardManager.Instance != null && CardManager.Instance.allCards.Count > 0)
            {
                cardToCreate = CardManager.Instance.GetRandomCard();
            }
            else
            {
                continue;
            }

            GameObject cardObject = CardManager.Instance.CreateCard(cardToCreate, cardContainer);
            if (cardObject != null)
            {
                RectTransform cardRT = cardObject.GetComponent<RectTransform>();
                arrangement.AddCard(cardRT);
                cardsGenerated++;
            }

            yield return new WaitForSeconds(0.1f);
        }

        // ���п���������ɺ�һ��������
        arrangement.ArrangeCardsImmediately();

        // �ָ�ԭ�����Զ���������
        arrangement.autoArrange = wasAutoArrange;

        currentWave++;

        Debug.Log($"����˵� {currentWave} ���������� {cardsGenerated} �Ų��Կ���");

        waveInProgress = false;
        UpdateButtonState();

        if (currentWave >= _maxWaves)
        {
            Debug.Log($"�Ѵﵽ���������: {_maxWaves}");
        }
    }

    // �����²��Ƿ�Ϊ��
    private bool IsUpdateSlotEmpty()
    {
        if (arrangement != null)
        {
            return arrangement.IsContainerActuallyEmpty(); // ʹ���µĸ�׼ȷ�ķ���
        }

        // ���ü�鷽��
        return cardContainer.childCount == 0;
    }

    // ���°�ť״̬
    private void UpdateButtonState()
    {
        if (nextWaveButton != null)
        {
            bool isSlotEmpty = IsUpdateSlotEmpty();
            bool canStartNextWave = isSlotEmpty && currentWave < _maxWaves && !waveInProgress;

            nextWaveButton.interactable = canStartNextWave;

            // ���°�ť�ı�
            Text buttonText = nextWaveButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (waveInProgress)
                {
                    buttonText.text = "������...";
                }
                else if (!isSlotEmpty)
                {
                    buttonText.text = "��ո��²�";
                    // ���������ʾ��ɫ
                    buttonText.color = Color.gray;
                }
                else if (currentWave < _maxWaves)
                {
                    buttonText.text = $"��һ�� ({currentWave + 1}/{_maxWaves})";
                    buttonText.color = Color.white;
                }
                else
                {
                    buttonText.text = "�����";
                    buttonText.color = Color.gray;
                }
            }

            // �����ʾ�ı�
            if (!isSlotEmpty && !waveInProgress)
            {
                int remainingCards = arrangement.GetCardsInContainerCount();
                Debug.Log($"���²��л��� {remainingCards} �ſ��ƣ�����պ��ٿ�ʼ��һ��");
            }
        }
    }

    [ContextMenu("�ֶ�������һ��")]
    public void ManualTriggerNextWave()
    {
        StartNextWave();
    }

    [ContextMenu("���ò���")]
    public void ResetWaves()
    {
        currentWave = 0;
        waveInProgress = false;
        StopAllCoroutines();

        // �������п���
        foreach (Transform child in cardContainer)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
        arrangement.ClearAllCards();

        // ���¿�ʼ״̬���
        StartCoroutine(CheckUpdateSlotStatus());

        UpdateButtonState();
        Debug.Log("����������������");
    }

    [ContextMenu("ǿ�����ò���Ϊ10")]
    public void ForceSetWavesTo10()
    {
        _maxWaves = 10;
        Debug.Log($"��ǿ�����������Ϊ: {_maxWaves}");
        UpdateButtonState();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}