using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Card Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("波次基本信息")]
    public int waveNumber;
    public string waveName;

    [Header("卡牌生成设置")]
    public int cardCount = 4;
    public bool useRandomCards = true;

    [Header("特定卡牌设置")]
    public List<CardData> specificCards = new List<CardData>();

    [Header("按类型生成的卡牌")]
    public List<CardType> cardTypesToSpawn = new List<CardType>();

    [Header("Boss卡设置")]
    public CardData bossCard;
    public bool spawnBossThisWave = false;
}