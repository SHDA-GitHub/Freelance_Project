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
    }

    private void OnDialogueChoiceMade(bool yesChosen)
    {
        playerChoseNo = !yesChosen;

        if (playerChoseNo && allowNoBattleChoice)
        {
            Debug.Log("Player chose No. Battle cancelled.");
            waitingForDialogue = false;
            return;
        }

        StartBattle();
    }

    private void StartBattle()
    {
        BattleDataBridge.UpcomingEnemyPreset = OEPS.enemyType;
        BattleDataBridge.BattleMusic = OEPS.battleMusic;
        BattleDataBridge.BackgroundSelection = OEPS.backgroundType;

        SceneManager.LoadScene(battleSceneName);
    }
}