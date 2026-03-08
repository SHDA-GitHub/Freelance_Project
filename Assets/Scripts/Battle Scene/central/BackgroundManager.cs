using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Spawn Parent")]
    [SerializeField] private Transform backgroundParent;

    [Header("Use Cross Scene Data")]
    [SerializeField] private bool useBattleDataBridge = true;

    [Header("Normal Enemy Backgrounds")]
    [SerializeField] private GameObject[] enemyBG;

    [Header("MiniBoss Backgrounds")]
    [SerializeField] private GameObject[] minibossBG;

    [Header("Boss Backgrounds")]
    [SerializeField] private GameObject[] bossBG;

    [Header("Final Boss Background")]
    [SerializeField] private GameObject finalBossBGPhase1;
    [SerializeField] private GameObject finalBossBGPhase2;
    [SerializeField] private GameObject finalBossBGPhase3;
    [SerializeField] private GameObject finalBossBGPhase4;

    [Header("Moon Soldier Background")]
    [SerializeField] private GameObject MoonSoldierBG;

    [Header("State Toggles (Manual Scene Control)")]
    public bool isNormalEnemy;
    public bool isMiniBoss;
    public bool isBoss;
    public int isFinalBossPhase = 0;
    public bool isMoonSoldier;

    private GameObject currentBackground;

    private void Start()
    {
        if (useBattleDataBridge)
        {
            ApplyBattleData();
        }

        SpawnBackground();
    }

    private void ApplyBattleData()
    {
        isNormalEnemy = false;
        isMiniBoss = false;
        isBoss = false;
        isMoonSoldier = false;
        isFinalBossPhase = 0;

        switch (BattleDataBridge.BackgroundSelection)
        {
            case BattleBackgroundType.Normal:
                isNormalEnemy = true;
                break;

            case BattleBackgroundType.Miniboss:
                isMiniBoss = true;
                break;

            case BattleBackgroundType.Boss:
                isBoss = true;
                break;

            case BattleBackgroundType.MoonSoldier:
                isMoonSoldier = true;
                break;

            case BattleBackgroundType.FinalBoss:
                isFinalBossPhase = 1;
                break;
        }
    }

    public void SpawnBackground()
    {
        if (currentBackground != null)
            Destroy(currentBackground);

        GameObject bgToSpawn = null;

        if (isFinalBossPhase == 1 && finalBossBGPhase1 != null)
            bgToSpawn = finalBossBGPhase1;

        else if (isFinalBossPhase == 2 && finalBossBGPhase2 != null)
            bgToSpawn = finalBossBGPhase2;

        else if (isFinalBossPhase == 3 && finalBossBGPhase3 != null)
            bgToSpawn = finalBossBGPhase3;

        else if (isFinalBossPhase == 4 && finalBossBGPhase4 != null)
            bgToSpawn = finalBossBGPhase4;

        else if (isMoonSoldier && MoonSoldierBG != null)
            bgToSpawn = MoonSoldierBG;

        else if (isBoss && bossBG.Length > 0)
            bgToSpawn = bossBG[Random.Range(0, bossBG.Length)];

        else if (isMiniBoss && minibossBG.Length > 0)
            bgToSpawn = minibossBG[Random.Range(0, minibossBG.Length)];

        else if (isNormalEnemy && enemyBG.Length > 0)
            bgToSpawn = enemyBG[Random.Range(0, enemyBG.Length)];

        if (bgToSpawn != null)
        {
            currentBackground = Instantiate(bgToSpawn, backgroundParent);
            currentBackground.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No background selected or array is empty.");
        }
    }

    public void OverrideBackground(GameObject newBG)
    {
        if (currentBackground != null)
            Destroy(currentBackground);

        currentBackground = Instantiate(newBG, backgroundParent);
        currentBackground.transform.localPosition = Vector3.zero;
    }
}