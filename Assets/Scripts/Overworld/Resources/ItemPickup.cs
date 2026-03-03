using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Pickup")]
    [SerializeField] private Item item;

    [Header("Special Attack Pickup")]
    [SerializeField] private SpecialAttack specialAttack;

    private bool playerInRange = false;
    private PlayerControl playerControl;

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
        if (playerInRange && playerControl != null && playerControl.isInteracting)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (item != null)
        {
            Inventory.Instance.AddItem(item);
        }

        if (specialAttack != null)
        {
            Inventory.Instance.AddSpecialAttack(specialAttack);
        }

        Destroy(gameObject);
    }
}