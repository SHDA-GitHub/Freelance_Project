using UnityEngine;

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
    [TextArea] public string introFlavorText;
    [TextArea] public string transformFlavorText;
}