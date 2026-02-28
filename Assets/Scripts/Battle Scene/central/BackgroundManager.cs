using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Spawn Parent")]
    [SerializeField] private Transform backgroundParent;

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

    [Header("State Toggles")]
    public bool isNormalEnemy;
    public bool isMiniBoss;
    public bool isBoss;
    public int isFinalBossPhase = 0;
    public bool isMoonSoldier;

    private GameObject currentBackground;

    private void Start()
    {
        SpawnBackground();
    }

    public void SpawnBackground()
    {
        if (currentBackground != null)
        {
            Destroy(currentBackground);
        }

        GameObject bgToSpawn = null;

        if (isFinalBossPhase == 1 && finalBossBGPhase1 != null)
        {
            bgToSpawn = finalBossBGPhase1;
        }
        if (isFinalBossPhase == 2 && finalBossBGPhase2 != null)
        {
            bgToSpawn = finalBossBGPhase2;
        }
        if (isFinalBossPhase == 3 && finalBossBGPhase3 != null)
        {
            bgToSpawn = finalBossBGPhase3;
        }
        if (isFinalBossPhase == 4 && finalBossBGPhase4 != null)
        {
            bgToSpawn = finalBossBGPhase4;
        }
        if (isMoonSoldier && MoonSoldierBG != null)
        {
            bgToSpawn = MoonSoldierBG;
        }
        else if (isBoss && bossBG.Length > 0)
        {
            bgToSpawn = bossBG[Random.Range(0, bossBG.Length)];
        }
        else if (isMiniBoss && minibossBG.Length > 0)
        {
            bgToSpawn = minibossBG[Random.Range(0, minibossBG.Length)];
        }
        else if (isNormalEnemy && enemyBG.Length > 0)
        {
            bgToSpawn = enemyBG[Random.Range(0, enemyBG.Length)];
        }

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
}