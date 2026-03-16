using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue questOfferDialogue;
    [SerializeField] private NPCDialogue questActiveDialogue;
    [SerializeField] private NPCDialogue questTurnInDialogue;
    [SerializeField] private NPCDialogue questCompleteDialogue;

    [Header("Quest Requirement")]
    [SerializeField] private Item requiredItem;

    [Header("Quest Rewards")]
    [SerializeField] private List<Item> rewardItems = new List<Item>();
    [SerializeField] private List<SpecialAttack> rewardSpecialAttacks = new List<SpecialAttack>();

    [Header("Sound")]
    [SerializeField] private AudioClip itemGetSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Inventory Full Dialogue")]
    [SerializeField] private NPCDialogue inventoryFullDialogue;
    [SerializeField] private NPCDialogue rewardPendingDialogue;
    private bool rewardPending = false;
    private bool rewardProcessing = false;

    private bool questAccepted = false;
    private bool questCompleted = false;

    private void Start()
    {
        if (questOfferDialogue != null)
            questOfferDialogue.onChoiceMade += HandleQuestOfferChoice;

        if (questTurnInDialogue != null)
            questTurnInDialogue.onChoiceMade += HandleTurnInChoice;

        if (rewardPendingDialogue != null)
            rewardPendingDialogue.onChoiceMade += HandlePendingRewardChoice;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerControl player = other.GetComponent<PlayerControl>();

        if (player == null)
            return;

        if (!player.isInteracting || DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive())
            return;

        if (rewardProcessing)
            return;

        if (questCompleted)
        {
            if (rewardPending)
            {
                rewardPendingDialogue.TriggerDialogue();
            }
            else
            {
                questCompleteDialogue.TriggerDialogue();
            }
            return;
        }

        if (!questAccepted)
        {
            questOfferDialogue.TriggerDialogue();
            return;
        }

        InventoryItem foundItem = Inventory.Instance.items
            .Find(i => i.itemData == requiredItem);

        if (foundItem != null)
        {
            questTurnInDialogue.TriggerDialogue();
        }
        else
        {
            questActiveDialogue.TriggerDialogue();
        }
    }

    void HandleQuestOfferChoice(bool accepted)
    {
        if (accepted)
        {
            questAccepted = true;
        }
    }

    void HandleTurnInChoice(bool gaveItem)
    {
        if (!gaveItem || rewardProcessing)
            return;

        InventoryItem foundItem = Inventory.Instance.items
            .Find(i => i.itemData == requiredItem);

        if (foundItem != null)
        {
            rewardProcessing = true;

            Inventory.Instance.items.Remove(foundItem);

            questCompleteDialogue.TriggerDialogue();

            StartCoroutine(GiveRewardsAfterDialogue());
        }
    }

    void HandlePendingRewardChoice(bool accept)
    {
        if (!accept)
            return;

        if (!CanReceiveRewards())
        {
            inventoryFullDialogue.TriggerDialogue();
            return;
        }

        rewardPending = false;
        GiveRewards();
    }

    IEnumerator GiveRewardsAfterDialogue()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(0.20f);

        if (!CanReceiveRewards())
        {
            rewardPending = true;
            inventoryFullDialogue.TriggerDialogue();
            yield break;
        }

        GiveRewards();
    }

    bool CanReceiveRewards()
    {
        int freeItemSlots = 16 - Inventory.Instance.items.Count;
        int freeAttackSlots = 16 - Inventory.Instance.specAttacks.Count;

        int neededItems = rewardItems.Count;
        int neededAttacks = rewardSpecialAttacks.Count;

        if (neededItems > freeItemSlots)
            return false;

        if (neededAttacks > freeAttackSlots)
            return false;

        return true;
    }

    void GiveRewards()
    {
        Dictionary<string, int> rewardCounts = new Dictionary<string, int>();
        List<string> dialogueLines = new List<string>();

        foreach (Item item in rewardItems)
        {
            if (item == null) continue;

            Inventory.Instance.AddItem(item);

            if (!rewardCounts.ContainsKey(item.itemName))
                rewardCounts[item.itemName] = 0;

            rewardCounts[item.itemName]++;
        }

        foreach (SpecialAttack attack in rewardSpecialAttacks)
        {
            if (attack == null) continue;

            Inventory.Instance.AddSpecialAttack(attack);

            if (!rewardCounts.ContainsKey(attack.specAttackName))
                rewardCounts[attack.specAttackName] = 0;

            rewardCounts[attack.specAttackName]++;
        }

        audioSource.clip = itemGetSound;
        audioSource.Play();

        foreach (var pair in rewardCounts)
        {
            if (pair.Value > 1)
                dialogueLines.Add($"You received {pair.Key} x{pair.Value}!");
            else
                dialogueLines.Add($"You received {pair.Key}!");
        }

        if (dialogueLines.Count > 0)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
            {
                List<NPCDialogue.DialogueLine> dialogueList = new List<NPCDialogue.DialogueLine>();
                foreach (string line in dialogueLines)
                {
                    dialogueList.Add(new NPCDialogue.DialogueLine { dialogueText = line });
                }
                DialogueManager.Instance.StartDialogue(null, dialogueList.ToArray(), null);
            }
        }
        questCompleted = true;
        rewardProcessing = false;
    }
}