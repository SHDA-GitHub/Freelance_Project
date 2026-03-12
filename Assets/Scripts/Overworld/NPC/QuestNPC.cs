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

    private bool questAccepted = false;
    private bool questCompleted = false;

    private void Start()
    {
        if (questOfferDialogue != null)
            questOfferDialogue.onChoiceMade += HandleQuestOfferChoice;

        if (questTurnInDialogue != null)
            questTurnInDialogue.onChoiceMade += HandleTurnInChoice;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerControl player = other.GetComponent<PlayerControl>();

        if (!player.isInteracting || DialogueManager.Instance.IsDialogueActive())
            return;

        if (questCompleted)
        {
            questCompleteDialogue.TriggerDialogue();
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
        if (!gaveItem)
            return;

        InventoryItem foundItem = Inventory.Instance.items
            .Find(i => i.itemData == requiredItem);

        if (foundItem != null)
        {
            Inventory.Instance.items.Remove(foundItem);

            questCompleted = true;

            questCompleteDialogue.TriggerDialogue();

            StartCoroutine(GiveRewardsAfterDialogue());
        }
    }

    IEnumerator GiveRewardsAfterDialogue()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        GiveRewards();
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

        foreach (var pair in rewardCounts)
        {
            if (pair.Value > 1)
                dialogueLines.Add($"You received {pair.Value} {pair.Key}s!");
            else
                dialogueLines.Add($"You received {pair.Key}!");
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
    }
}