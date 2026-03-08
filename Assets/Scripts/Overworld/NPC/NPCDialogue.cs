using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogue;

    [Header("Optional Dialogue Music")]
    [SerializeField] private AudioClip dialogueMusic;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue, dialogueMusic);
    }
}