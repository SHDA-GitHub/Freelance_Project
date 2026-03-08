using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class OverworldEnemyInteract : MonoBehaviour
{
    [Header("Battle Settings")]
    public EnemyPreset enemyType;
    public AudioClip battleMusic;
    public BattleBackgroundType backgroundType;
    [SerializeField] private string battleSceneName = "Battle Scene";

    [Header("Optional Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    private bool playerInRange = false;
    private bool waitingForDialogue = false;
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
        if (playerControl == null) return;

        if (playerInRange && playerControl.isInteracting && !DialogueManager.Instance.IsDialogueActive())
        {
            if (dialogue != null)
            {
                dialogue.TriggerDialogue();
                waitingForDialogue = true;
            }
            else
            {
                StartBattle();
            }
        }

        if (waitingForDialogue && !DialogueManager.Instance.IsDialogueActive())
        {
            waitingForDialogue = false;
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