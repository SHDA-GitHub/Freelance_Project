using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)]
        public string dialogueText;

        [Header("Yes Choice")]
        public DialogueLine[] yesDialogueLines;

        [TextArea(2, 5)]
        public string yesButtonText = "Yes";

        [Header("No Choice")]
        public DialogueLine[] noDialogueLines;

        [TextArea(2, 5)]
        public string noButtonText = "No";

        public bool isChoiceActive;
    }

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Optional Dialogue Music")]
    [SerializeField] private AudioClip dialogueMusic;

    public System.Action<bool> onChoiceMade;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(this, dialogueLines, dialogueMusic);
    }

    public DialogueLine[] GetDialogueLines()
    {
        return dialogueLines;
    }

    public AudioClip GetDialogueMusic()
    {
        return dialogueMusic;
    }
}