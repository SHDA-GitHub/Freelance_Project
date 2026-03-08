using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class OverworldEnemyInteract : MonoBehaviour
{
    [Header("Battle Settings")]
    public EnemyPreset enemyType;
    public AudioClip battleMusic;
    public BattleBackgroundType backgroundType;
    [SerializeField] private string battleSceneName = "Battle Scene";

    private bool playerInRange = false;
    private PlayerControl playerControl;

    private void Start()
    {
        playerControl = FindFirstObjectByType<PlayerControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && playerControl != null && playerControl.isInteracting)
        {
            StartBattle();
        }
    }

    private void StartBattle()
    {
        BattleDataBridge.UpcomingEnemyPreset = enemyType;
        BattleDataBridge.BattleMusic = battleMusic;
        BattleDataBridge.BackgroundSelection = backgroundType;
        SceneManager.LoadScene(battleSceneName);
    }
}