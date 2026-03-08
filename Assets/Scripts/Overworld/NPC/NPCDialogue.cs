using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogue;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}