using System.Collections;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static NPCDialogue;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private FlavorTextUI flavorText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private GameObject endDialogueIndicator;
    [SerializeField] private NavMeshSurface enemyPatrolSurface;

    [Header("Music")]
    [SerializeField] private MusicManager musicManager;

    private AudioClip previousTrack;

    private NPCDialogue currentNPCDialogue;
    private NPCDialogue.DialogueLine[] currentDialogueLines;
    private int dialogueIndex;

    private bool dialogueActive = false;
    private bool canStartDialogue = true;

    private PlayerControl player;

    private void Awake()
    {
        Instance = this;
        dialogueUI.SetActive(false);
        player = FindFirstObjectByType<PlayerControl>();

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    public void StartDialogue(NPCDialogue npcDialogue, NPCDialogue.DialogueLine[] dialogueLines, AudioClip dialogueMusic = null)
    {
        if (dialogueActive || !canStartDialogue) return;

        player.DisableControls();
        enemyPatrolSurface.enabled = false;
        currentNPCDialogue = npcDialogue;
        currentDialogueLines = dialogueLines;
        dialogueIndex = 0;

        dialogueActive = true;
        canStartDialogue = false;

        dialogueUI.SetActive(true);

        if (dialogueMusic != null && musicManager != null && MusicManager.Instance.GetCurrentTrack() != dialogueMusic)
        {
            previousTrack = MusicManager.Instance.GetCurrentTrack();
            MusicManager.Instance.PlayOverrideMusic(dialogueMusic);
        }

        endDialogueIndicator.SetActive(false);
        StartCoroutine(RunDialogue());
    }

    IEnumerator RunDialogue()
    {
        while (dialogueIndex < currentDialogueLines.Length)
        {
            DialogueLine currentLine = currentDialogueLines[dialogueIndex];

            yield return StartCoroutine(flavorText.ShowTextCoroutine(currentLine.dialogueText));

            if (currentLine.isChoiceActive)
            {
                bool choiceMade = false;

                ShowChoiceButtons(currentLine, () => choiceMade = true);

                yield return new WaitUntil(() => choiceMade);
            }
            else
            {
                endDialogueIndicator.SetActive(true);
                yield return new WaitUntil(() => player.isInteracting);
                endDialogueIndicator.SetActive(false);
                dialogueIndex++;
            }
        }

        EndDialogue();
    }

    private void ShowChoiceButtons(DialogueLine currentLine, System.Action onChoiceMadeCallback)
    {
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        yesButton.GetComponentInChildren<TextMeshProUGUI>().text = string.IsNullOrEmpty(currentLine.yesButtonText) ? "Yes" : currentLine.yesButtonText;
        noButton.GetComponentInChildren<TextMeshProUGUI>().text = string.IsNullOrEmpty(currentLine.noButtonText) ? "No" : currentLine.noButtonText;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() => StartCoroutine(HandleChoice(currentLine, true, onChoiceMadeCallback)));
        noButton.onClick.AddListener(() => StartCoroutine(HandleChoice(currentLine, false, onChoiceMadeCallback)));
    }

    private IEnumerator HandleChoice(DialogueLine currentLine, bool isYes, System.Action onChoiceMadeCallback)
    {
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        DialogueLine[] chosenLines = isYes ? currentLine.yesDialogueLines : currentLine.noDialogueLines;

        if (chosenLines != null && chosenLines.Length > 0)
        {
            DialogueLine[] remainingLines = new DialogueLine[currentDialogueLines.Length - dialogueIndex - 1];
            for (int i = 0; i < remainingLines.Length; i++)
                remainingLines[i] = currentDialogueLines[dialogueIndex + 1 + i];

            currentDialogueLines = new DialogueLine[chosenLines.Length + remainingLines.Length];
            chosenLines.CopyTo(currentDialogueLines, 0);
            remainingLines.CopyTo(currentDialogueLines, chosenLines.Length);

            dialogueIndex = 0;

            yield return StartCoroutine(RunDialogue());
        }
        else
        {
            dialogueIndex++;
        }
        onChoiceMadeCallback?.Invoke();
        currentNPCDialogue?.onChoiceMade?.Invoke(isYes);

        yield return null;
    }

    void EndDialogue()
    {
        player.EnableControls();
        enemyPatrolSurface.enabled = true;
        dialogueActive = false;
        dialogueUI.SetActive(false);

        if (previousTrack != null && MusicManager.Instance.GetCurrentTrack() != previousTrack)
        {
            MusicManager.Instance.PlayOverrideMusic(previousTrack);
        }
        else
        {
            MusicManager.Instance.ClearOverrideMusic();
        }

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