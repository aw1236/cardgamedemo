using UnityEngine;
using TMPro;

public class SimpleWaveDisplay : MonoBehaviour
{
    [Header("�����ı���ʾ")]
    public TextMeshProUGUI waveText;

    [Header("��ɫ����")]
    public Color normalWaveColor = Color.white;
    public Color bossWaveColor = Color.red;
    public Color completedColor = Color.green;

    private TestSceneManager waveManager;

    private void Start()
    {
        // ���ҳ����е�TestSceneManager
        waveManager = FindObjectOfType<TestSceneManager>();
        if (waveManager == null)
        {
            Debug.LogError("δ�ҵ�TestSceneManager! ��ȷ����������TestSceneManager�����");
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
            // �������²���
            waveManager = FindObjectOfType<TestSceneManager>();
            if (waveManager == null) return;
        }

        // ���²����ı�
        if (waveText != null)
        {
            int currentWave = waveManager.currentWave;
            int maxWaves = waveManager.maxWaves;

            waveText.text = $"����: {currentWave}/{maxWaves}";

            // ���ݲ���״̬�ı���ɫ
            if (currentWave > maxWaves)
            {
                waveText.color = completedColor;
                waveText.text = "��������в���";
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