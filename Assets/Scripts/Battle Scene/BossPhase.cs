using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossPhase
{
    public int hpThreshold;
    public GameObject backgroundPrefab;
    public string phaseName;
    public AudioClip phaseMusic;

    [Header("Optional Form Change")]
    public EnemyPreset newEnemyPreset;

    [Header("Phase Dialogue")]

    [TextArea(2, 4)]
    public List<string> introDialogue = new List<string>();

    [TextArea(2, 4)]
    public List<string> transformDialogue = new List<string>();
}