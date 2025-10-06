using UnityEngine;
using System.Collections.Generic;

public class WaveConfigManager : MonoBehaviour
{
    public static WaveConfigManager Instance { get; private set; }

    [Header("���������б�")]
    public List<WaveConfig> waveConfigs = new List<WaveConfig>();

    [Header("Ĭ������")]
    public int defaultCardCount = 4;
    public bool autoStartFirstWave = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public WaveConfig GetWaveConfig(int waveNumber)
    {
        return waveConfigs.Find(config => config.waveNumber == waveNumber);
    }

    public bool HasWaveConfig(int waveNumber)
    {
        return waveConfigs.Exists(config => config.waveNumber == waveNumber);
    }
}