using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private FlavorTextUI flavorText;

    private string[] currentDialogue;
    private int dialogueIndex;

    private bool waitingForInput = false;
    private bool dialogueActive = false;
    private bool canStartDialogue = true;

    private PlayerControl player;

    private void Awake()
    {
        Instance = this;
        dialogueUI.SetActive(false);
        player = FindFirstObjectByType<PlayerControl>();
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (dialogueActive || !canStartDialogue) return;

        dialogueActive = true;
        canStartDialogue = false;

        currentDialogue = dialogueLines;
        dialogueIndex = 0;

        dialogueUI.SetActive(true);

        StartCoroutine(RunDialogue());
    }

    IEnumerator RunDialogue()
    {
        while (dialogueIndex < currentDialogue.Length)
        {
            yield return StartCoroutine(flavorText.ShowTextCoroutine(currentDialogue[dialogueIndex]));

            waitingForInput = true;

            yield return new WaitUntil(() => player.isInteracting);

            waitingForInput = false;
            dialogueIndex++;
        }

        EndDialogue();
    }

    void EndDialogue()
    {
        dialogueActive = false;
        dialogueUI.SetActive(false);

        StartCoroutine(DialogueCooldown());
    }

    IEnumerator DialogueCooldown()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        canStartDialogue = true;
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}