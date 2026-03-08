using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [SerializeField] private NPCDialogue dialogue;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl player = other.GetComponent<PlayerControl>();

            if (player.isInteracting && !DialogueManager.Instance.IsDialogueActive())
            {
                dialogue.TriggerDialogue();
            }
        }
    }
}