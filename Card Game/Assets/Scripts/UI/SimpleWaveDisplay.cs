using UnityEngine;
using TMPro;

public class SimpleWaveDisplay : MonoBehaviour
{
    [Header("波次文本显示")]
    public TextMeshProUGUI waveText;

    [Header("颜色设置")]
    public Color normalWaveColor = Color.white;
    public Color bossWaveColor = Color.red;
    public Color completedColor = Color.green;

    private TestSceneManager waveManager;

    private void Start()
    {
        // 查找场景中的TestSceneManager
        waveManager = FindObjectOfType<TestSceneManager>();
        if (waveManager == null)
        {
            Debug.LogError("未找到TestSceneManager! 请确保场景中有TestSceneManager组件。");
            return;
        }

        UpdateWaveDisplay();
    }

    private void Update()
    {
        UpdateWaveDisplay();
    }

    private void UpdateWaveDisplay()
    {
        if (waveManager == null)
        {
            // 尝试重新查找
            waveManager = FindObjectOfType<TestSceneManager>();
            if (waveManager == null) return;
        }

        // 更新波次文本
        if (waveText != null)
        {
            int currentWave = waveManager.currentWave;
            int maxWaves = waveManager.maxWaves;

            waveText.text = $"波次: {currentWave}/{maxWaves}";

            // 根据波次状态改变颜色
            if (currentWave > maxWaves)
            {
                waveText.color = completedColor;
                waveText.text = "已完成所有波次";
            }
            else if (IsBossWave(currentWave))
            {
                waveText.color = bossWaveColor;
            }
            else
            {
                waveText.color = normalWaveColor;
            }
        }
    }

    private bool IsBossWave(int waveNumber)
    {
        var configManager = WaveConfigManager.Instance;
        if (configManager != null && configManager.HasWaveConfig(waveNumber))
        {
            var config = configManager.GetWaveConfig(waveNumber);
            return config.spawnBossThisWave && config.bossCard != null;
        }
        return false;
    }
}