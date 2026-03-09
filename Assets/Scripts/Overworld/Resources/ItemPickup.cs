using UnityEngine;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour
{
    [Header("Optional Pre-Pickup Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] prePickupDialogue;

    [Header("Item Pickups")]
    [SerializeField] private List<Item> items = new List<Item>();

    [Header("Special Attack Pickups")]
    [SerializeField] private List<SpecialAttack> specialAttacks = new List<SpecialAttack>();

    private bool playerInRange = false;
    private PlayerControl playerControl;
    private bool pickedUp = false;

    private void Start()
    {
        playerControl = FindFirstObjectByType<PlayerControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (!pickedUp && playerInRange && playerControl != null && playerControl.isInteracting)
        {
            pickedUp = true;
            PickUp();
        }
    }

    private void PickUp()
    {
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        List<string> dialogueLines = new List<string>();

        if (prePickupDialogue != null && prePickupDialogue.Length > 0)
        {
            dialogueLines.AddRange(prePickupDialogue);
        }

        foreach (Item item in items)
        {
            if (item == null) continue;

            Inventory.Instance.AddItem(item);

            if (!itemCounts.ContainsKey(item.itemName))
                itemCounts[item.itemName] = 0;

            itemCounts[item.itemName]++;
        }

        foreach (SpecialAttack attack in specialAttacks)
        {
            if (attack == null) continue;

            Inventory.Instance.AddSpecialAttack(attack);

            if (!itemCounts.ContainsKey(attack.specAttackName))
                itemCounts[attack.specAttackName] = 0;

            itemCounts[attack.specAttackName]++;
        }

        foreach (var pair in itemCounts)
        {
            if (pair.Value > 1)
                dialogueLines.Add($"You found {pair.Key} x{pair.Value}!");
            else
                dialogueLines.Add($"You found a {pair.Key}!");
        }

        if (dialogueLines.Count > 0)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
            {
                NPCDialogue tempDialogue = new NPCDialogue();
                List<NPCDialogue.DialogueLine> dialogueList = new List<NPCDialogue.DialogueLine>();
                foreach (string line in dialogueLines)
                {
                    dialogueList.Add(new NPCDialogue.DialogueLine { dialogueText = line });
                }
                DialogueManager.Instance.StartDialogue(tempDialogue, dialogueList.ToArray(), null);
            }
        }

        Destroy(gameObject);
    }
}