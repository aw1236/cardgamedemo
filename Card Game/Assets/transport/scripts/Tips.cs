using UnityEngine;
using TMPro;

public class Tips : MonoBehaviour
{
    public string[] tips;
    private int currentTipIndex = 0;
    private TextMeshProUGUI tipText;

    void Start()
    {
        tipText = GetComponent<TextMeshProUGUI>();
        ShowCurrentTip();
    }

    void ShowCurrentTip()
    {
        tipText.text = tips[currentTipIndex];
    }

    void NextTip()
    {
        currentTipIndex = (currentTipIndex + 1) % tips.Length;
        ShowCurrentTip();
    }

    // �������������ť�ĵ���¼�����
    public void OnClickjack()
    {
        NextTip();
    }
}