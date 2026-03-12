using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class BattleSceneTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "Battle Scene";
    [SerializeField] private OverworldEnemyPatrolScript OEPS;

    [Header("Optional Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    [Header("Battle Control")]
    [SerializeField] private bool allowNoBattleChoice = false;

    private bool waitingForDialogue = false;
    private bool playerChoseNo = false;

    private void Update()
    {
        if (waitingForDialogue && !DialogueManager.Instance.IsDialogueActive())
        {
            waitingForDialogue = false;
            if (!playerChoseNo || !allowNoBattleChoice)
            {
                StartBattle();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !DialogueManager.Instance.IsDialogueActive())
        {
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
        BattleDataBridge.UpcomingEnemyPreset = OEPS.enemyType;
        BattleDataBridge.BattleMusic = OEPS.battleMusic;
        BattleDataBridge.BackgroundSelection = OEPS.backgroundType;

        SceneManager.LoadScene(battleSceneName);
    }
}