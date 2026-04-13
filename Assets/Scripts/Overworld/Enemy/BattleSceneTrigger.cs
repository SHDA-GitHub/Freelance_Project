using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static BattleDataBridge;

public class BattleSceneTrigger : MonoBehaviour
{
    [SerializeField] private OverworldEnemyPatrolScript OEPS;

    [Header("Optional Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    [Header("Battle Control")]
    [SerializeField] private bool allowNoBattleChoice = false;

    private PlayerControl playerControl;
    private bool waitingForDialogue = false;
    private bool playerChoseNo = false;

    private void Start()
    {
        playerControl = FindFirstObjectByType<PlayerControl>();
    }

    private void Update()
    {
        if (waitingForDialogue && !DialogueManager.Instance.IsDialogueActive())
        {
            waitingForDialogue = false;

            if (!playerChoseNo || !allowNoBattleChoice)
            {
                OEPS.StartBattle();
            }
            else
            {
                Debug.Log("Battle cancelled because player chose No.");
            }

            playerChoseNo = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (DialogueManager.Instance.IsDialogueActive()) return;

        if (playerControl.isInteracting)
        {
            if (dialogue != null)
            {
                dialogue.onChoiceMade = OnDialogueChoiceMade;
                dialogue.TriggerDialogue();
                waitingForDialogue = true;
            }
            else
            {
                OEPS.StartBattle();
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
}