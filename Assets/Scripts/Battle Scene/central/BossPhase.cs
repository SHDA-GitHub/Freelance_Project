using UnityEngine;

[System.Serializable]
public class BossPhase
{
    public int hpThreshold;
    public GameObject backgroundPrefab;
    public string phaseName;
    public AudioClip phaseMusic;
}