using UnityEngine;

public class LockAndKeyMechanic : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private Item requiredItem;
    [SerializeField] private bool consumeItem = true;

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue openDoorDialogue;
    [SerializeField] private NPCDialogue missingItemDialogue;

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

        InventoryItem foundItem = Inventory.Instance.keyItems
            .Find(i => i.itemData == requiredItem);

        if (foundItem != null)
        {
            opening = true;
            opened = true;

            if (consumeItem)
            {
                Inventory.Instance.keyItems.Remove(foundItem);
            }
        }
        else
        {
            if (missingItemDialogue != null)
                missingItemDialogue.TriggerDialogue();
        }
    }
}