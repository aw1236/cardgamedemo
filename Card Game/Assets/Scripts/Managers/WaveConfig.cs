using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Card Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("���λ�����Ϣ")]
    public int waveNumber;
    public string waveName;

    [Header("������������")]
    public int cardCount = 4;
    public bool useRandomCards = true;

    [Header("�ض���������")]
    public List<CardData> specificCards = new List<CardData>();

    [Header("���������ɵĿ���")]
    public List<CardType> cardTypesToSpawn = new List<CardType>();

    [Header("Boss������")]
    public CardData bossCard;
    public bool spawnBossThisWave = false;
}