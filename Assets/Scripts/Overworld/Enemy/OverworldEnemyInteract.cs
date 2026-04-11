using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class OverworldEnemyInteract : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<WeightedEnemy> enemies = new List<WeightedEnemy>();
    public AudioClip battleMusic;
    public BattleBackgroundType backgroundType;
    public BattleTransitionType transitionType = BattleTransitionType.Normal;

    [Header("Optional Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    [Header("Bribe Settings")]
    [SerializeField] private bool allowPayToSkipBattle = false;
    [SerializeField] private float skipBattleCost = 5f;
    private bool hasProcessedDialogueResult = false;

    [Header("Battle Control")]
    [SerializeField] private bool allowNoBattleChoice = false;
    private bool playerChoseNo = false;

    private bool playerInRange = false;
    private bool waitingForDialogue = false;
    private PlayerControl playerControl;

    private void Start()
    {
        hasProcessedDialogueResult = true;
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
                hasProcessedDialogueResult = false;
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

                if (allowPayToSkipBattle && hasProcessedDialogueResult == false)
                {
                    StartCoroutine(TryPayToSkipBattle());
                }
            }
        }
    }

    private void OnDialogueChoiceMade(string yesChosen)
    {
        if (yesChosen != "yes" && allowNoBattleChoice)
        {
            playerChoseNo = true;
        }
    }

    private IEnumerator TryPayToSkipBattle()
    {
            CurrencyManager.Instance.SpendCoins(skipBattleCost);
            Debug.Log($"Player paid {skipBattleCost} to skip battle.");
            NPCDialogue.DialogueLine line = new NPCDialogue.DialogueLine
            {
                dialogueText = $"You gave up {skipBattleCost} dollars.",
                isChoiceActive = false,
            };
            yield return new WaitForSeconds(0.4f);
            DialogueManager.Instance.InjectDialogueLine(line);
            hasProcessedDialogueResult = true;
    }

    private EnemyPreset GetRandomEnemy()
    {
        float totalWeight = 0f;

        foreach (var e in enemies)
            totalWeight += e.spawnChance;

        float randomValue = Random.Range(0, totalWeight);

        float current = 0f;

        foreach (var e in enemies)
        {
            current += e.spawnChance;

            if (randomValue <= current)
                return e.enemy;
        }

        return enemies[0].enemy;
    }

    private void StartBattle()
    {
        BattleDataBridge.UpcomingEnemyPreset = GetRandomEnemy();
        BattleDataBridge.BattleMusic = battleMusic;
        BattleDataBridge.BackgroundSelection = backgroundType;

        var manager = FindFirstObjectByType<BattleTransitionManager>();

        if (manager != null)
        {
            manager.RegisterEncounterEnemy(gameObject);
            manager.StartBattleTransition(transitionType);
        }
    }
}