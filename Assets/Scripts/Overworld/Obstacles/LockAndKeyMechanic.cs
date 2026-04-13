using UnityEngine;
using System.Collections;

public class LockAndKeyMechanic : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private Item requiredItem;
    [SerializeField] private bool consumeItem = true;

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue openDoorDialogue;
    [SerializeField] private NPCDialogue confirmDialogue;
    [SerializeField] private NPCDialogue rejectDialogue;

    [Header("Door Setup")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private Vector3 leftDoorDirection;
    [SerializeField] private Vector3 rightDoorDirection;
    [SerializeField] private float doorOpenOffset = 3f;
    [SerializeField] private float speed = 5f;

    private Vector3 leftDoorOrigin;
    private Vector3 rightDoorOrigin;

    private bool opening = false;
    private bool opened = false;

    private void Start()
    {
        leftDoorOrigin = leftDoor.position;
        rightDoorOrigin = rightDoor.position;

        if (openDoorDialogue != null)
            openDoorDialogue.onChoiceMade += HandleDoorChoice;
    }

    private void Update()
    {
        if (!opening) return;

        leftDoor.position = Vector3.MoveTowards(
            leftDoor.position,
            leftDoorOrigin + leftDoorDirection * doorOpenOffset,
            Time.deltaTime * speed
        );

        rightDoor.position = Vector3.MoveTowards(
            rightDoor.position,
            rightDoorOrigin + rightDoorDirection * doorOpenOffset,
            Time.deltaTime * speed
        );
    }

    private void OnTriggerStay(Collider other)
    {
        if (opened || !other.CompareTag("Player"))
            return;

        PlayerControl player = other.GetComponent<PlayerControl>();

        if (!player.isInteracting || DialogueManager.Instance.IsDialogueActive())
            return;

        openDoorDialogue.TriggerDialogue();
    }

    private void HandleDoorChoice(string choice)
    {
        if (choice != "Yes")
            return;

        if (requiredItem == null)
        {
            opening = true;
            opened = true;

            StartCoroutine(ShowSimpleConfirmSequence());
            return;
        }

        InventoryItem foundItem = Inventory.Instance.keyItems
            .Find(i => i.itemData == requiredItem);

        if (foundItem != null)
        {
            opening = true;
            opened = true;

            string itemName = foundItem.itemData.itemName;

            if (consumeItem)
            {
                Inventory.Instance.keyItems.Remove(foundItem);
            }

            StartCoroutine(ShowConfirmSequence(itemName));
        }
        else
        {
            StartCoroutine(ShowRejectSequence());
        }
    }

    private IEnumerator ShowConfirmSequence(string itemName)
    {
        yield return new WaitForSeconds(0.37f);
        NPCDialogue tempDialogue = new NPCDialogue();
        var lines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine { dialogueText = $"You used {itemName}." }
        };

        DialogueManager.Instance.StartDialogue(tempDialogue, lines, null);

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(0.37f);

        if (confirmDialogue != null)
            confirmDialogue.TriggerDialogue();
    }

    private IEnumerator ShowRejectSequence()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(0.37f);

        if (rejectDialogue != null)
            rejectDialogue.TriggerDialogue();
    }

    private IEnumerator ShowSimpleConfirmSequence()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(0.37f);

        if (confirmDialogue != null)
            confirmDialogue.TriggerDialogue();
    }
}