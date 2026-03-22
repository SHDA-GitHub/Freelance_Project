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
    [SerializeField] private NPCDialogue questInvFullDialogue;
    [SerializeField] private NPCDialogue questInvStillFullDialogue;

    [Header("Quest Requirement")]
    [SerializeField] private Item requiredItem;

    [Header("Quest Rewards")]
    [SerializeField] private List<Item> rewardItems = new List<Item>();
    [SerializeField] private List<SpecialAttack> rewardSpecialAttacks = new List<SpecialAttack>();
    private List<Item> pendingItems = new List<Item>();
    private List<SpecialAttack> pendingAttacks = new List<SpecialAttack>();

    [Header("Sound")]
    [SerializeField] private AudioClip itemGetSound;
    [SerializeField] private AudioSource audioSource;

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
            if (pendingItems.Count > 0 || pendingAttacks.Count > 0)
            {
                TryGivePendingRewards();
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

        InventoryItem foundItem = Inventory.Instance.keyItems
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

    void HandleQuestOfferChoice(string accepted)
    {
        if (accepted == "yes")
        {
            questAccepted = true;
        }
    }

    void HandleTurnInChoice(string gaveItem)
    {
        if (gaveItem != "yes")
            return;

        InventoryItem foundItem = Inventory.Instance.keyItems
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

        yield return new WaitForSeconds(0.37f);

        GiveRewards();
    }

    void GiveRewards()
    {

        Dictionary<string, int> rewardCounts = new Dictionary<string, int>();
        List<string> dialogueLines = new List<string>();

        foreach (Item item in rewardItems)
        {
            if (item == null) continue;

            bool added = Inventory.Instance.AddItem(item);

            if (!added)
            {
                pendingItems.Add(item);
                continue;
            }

            if (!rewardCounts.ContainsKey(item.itemName))
                rewardCounts[item.itemName] = 0;

            rewardCounts[item.itemName]++;
        }

        foreach (SpecialAttack attack in rewardSpecialAttacks)
        {
            if (attack == null) continue;

            bool added = Inventory.Instance.AddSpecialAttack(attack);

            if (!added)
            {
                pendingAttacks.Add(attack);
                continue;
            }

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
                ShowDialogue(dialogueLines);
            }

            if (pendingItems.Count > 0 || pendingAttacks.Count > 0)
            {
                if (questInvFullDialogue != null)
                    questInvFullDialogue.TriggerDialogue();
            }
        }

        void TryGivePendingRewards()
    {
        List<string> dialogueLines = new List<string>();
        Dictionary<string, int> rewardCounts = new Dictionary<string, int>();

        for (int i = pendingItems.Count - 1; i >= 0; i--)
        {
            var item = pendingItems[i];

            if (Inventory.Instance.AddItem(item))
            {
                if (!rewardCounts.ContainsKey(item.itemName))
                    rewardCounts[item.itemName] = 0;

                rewardCounts[item.itemName]++;
                pendingItems.RemoveAt(i);
            }
        }

        for (int i = pendingAttacks.Count - 1; i >= 0; i--)
        {
            var atk = pendingAttacks[i];

            if (Inventory.Instance.AddSpecialAttack(atk))
            {
                if (!rewardCounts.ContainsKey(atk.specAttackName))
                    rewardCounts[atk.specAttackName] = 0;

                rewardCounts[atk.specAttackName]++;
                pendingAttacks.RemoveAt(i);
            }
        }

        foreach (var pair in rewardCounts)
        {
            dialogueLines.Add($"You received {pair.Key} x{pair.Value}!");
        }

        ShowDialogue(dialogueLines);
        audioSource.clip = itemGetSound;
        audioSource.Play();

        if (pendingItems.Count > 0 || pendingAttacks.Count > 0)
        {
            if (questInvStillFullDialogue != null)
                questInvStillFullDialogue.TriggerDialogue();
        }
        else
        {
            if (questCompleteDialogue != null)
                questCompleteDialogue.TriggerDialogue();
        }

        return;
    }

    void ShowDialogue(List<string> lines)
    {
        if (lines.Count == 0) return;

        NPCDialogue tempDialogue = new NPCDialogue();
        List<NPCDialogue.DialogueLine> dialogueList = new List<NPCDialogue.DialogueLine>();

        foreach (string line in lines)
        {
            dialogueList.Add(new NPCDialogue.DialogueLine { dialogueText = line });
        }

        DialogueManager.Instance.StartDialogue(tempDialogue, dialogueList.ToArray(), null);
    }
}