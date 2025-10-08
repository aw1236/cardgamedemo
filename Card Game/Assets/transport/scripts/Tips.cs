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

    // 这个方法将被按钮的点击事件调用
    public void OnClickjack()
    {
        NextTip();
    }
}