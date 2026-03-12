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

    [Header("Battle Control")]
    [SerializeField] private bool allowNoBattleChoice = false;
    private bool playerChoseNo = false;

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
                dialogue.onChoiceMade = OnDialogueChoiceMade;
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

            if (!DialogueManager.Instance.PlayerCancelledChoice() || !allowNoBattleChoice)
            {
                StartBattle();
            }
            else
            {
                Debug.Log("Battle cancelled because player chose No.");
            }
        }
    }

    private void OnDialogueChoiceMade(bool yesChosen)
    {
        if (!yesChosen && allowNoBattleChoice)
        {
            playerChoseNo = true;
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