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

    [Header("Sound")]
    [SerializeField] private AudioClip itemGetSound;
    [SerializeField] private AudioClip grandItemGetSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Suspense")]
    [SerializeField] private bool cutMusicOnPickup = false;
    [SerializeField] private bool grandItem = false;

    [Header("Optional")]
    [SerializeField] private DemoScript demoScript;
    [SerializeField] private bool endDemo = false;

    private bool playerInRange = false;
    private PlayerControl playerControl;
    private bool pickedUp = false;

    private void Start()
    {
        demoScript = FindFirstObjectByType<DemoScript>();
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

        bool itemInventoryFull = false;
        bool specialAttackInventoryFull = false;
        bool pickedUpSomething = false;

        if (cutMusicOnPickup && MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeOutMusic();
        }

        if (prePickupDialogue != null && prePickupDialogue.Length > 0)
        {
            dialogueLines.AddRange(prePickupDialogue);
        }

        List<Item> itemsToRemove = new List<Item>();
        foreach (Item item in items)
        {
            if (item == null) continue;

            bool added = false;

            if (item.isKeyItem)
            {
                added = Inventory.Instance.AddKeyItem(item);
            }
            else
            {
                added = Inventory.Instance.AddItem(item);
            }

            if (!added && !item.isKeyItem)
            {
                itemInventoryFull = true;
                continue;
            }

            pickedUpSomething = true;
            itemsToRemove.Add(item);

            if (!itemCounts.ContainsKey(item.itemName))
                itemCounts[item.itemName] = 0;

            itemCounts[item.itemName]++;
        }

        foreach (Item item in itemsToRemove)
        {
            items.Remove(item);
        }

        List<SpecialAttack> attacksToRemove = new List<SpecialAttack>();
        foreach (SpecialAttack attack in specialAttacks)
        {
            if (attack == null) continue;

            bool added = Inventory.Instance.AddSpecialAttack(attack);

            if (!added)
            {
                specialAttackInventoryFull = true;
                continue;
            }

            pickedUpSomething = true;
            attacksToRemove.Add(attack);

            if (!itemCounts.ContainsKey(attack.specAttackName))
                itemCounts[attack.specAttackName] = 0;

            itemCounts[attack.specAttackName]++;
        }

        foreach (SpecialAttack attack in attacksToRemove)
        {
            specialAttacks.Remove(attack);
        }

        if (!grandItem && pickedUpSomething && audioSource != null && itemGetSound != null)
        {
            audioSource.clip = itemGetSound;
            audioSource.Play();
        }

        foreach (var pair in itemCounts)
        {
            if (pair.Value > 1)
                dialogueLines.Add($"You received {pair.Key} x{pair.Value}!");
            else
                dialogueLines.Add($"You found {pair.Key}!");
            if (grandItem && pickedUpSomething && audioSource != null && itemGetSound != null)
            {
                audioSource.clip = grandItemGetSound;
                audioSource.Play();
            }
        }

        if (itemInventoryFull)
            dialogueLines.Add("Your item inventory is too full to carry any more items.");

        if (specialAttackInventoryFull)
            dialogueLines.Add("Your special attack inventory is too full to carry any more special attacks.");

        if (dialogueLines.Count > 0 && DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
        {
            NPCDialogue tempDialogue = new NPCDialogue();
            List<NPCDialogue.DialogueLine> dialogueList = new List<NPCDialogue.DialogueLine>();

            foreach (string line in dialogueLines)
            {
                dialogueList.Add(new NPCDialogue.DialogueLine { dialogueText = line });
            }

            if (endDemo)
            {
                DialogueManager.Instance.onDialogueEnded += EndDemo;
            }

            DialogueManager.Instance.StartDialogue(tempDialogue, dialogueList.ToArray(), null);
        }


        InventoryUIController ui = FindFirstObjectByType<InventoryUIController>();

        if (ui != null)
        {
            ui.RefreshItemUI();
        }

        if (items.Count == 0 && specialAttacks.Count == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            pickedUp = false;
        }
    }

    private void EndDemo()
    {
        demoScript.EndDemo();
    }
}