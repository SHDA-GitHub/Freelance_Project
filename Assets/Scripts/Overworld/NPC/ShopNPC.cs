using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopNPC : MonoBehaviour
{
    private enum ShopState { Intro, Decision, Finished }
    private ShopState currentState = ShopState.Intro;

    [Header("Dialogue Data")]
    [SerializeField] private NPCDialogue greetingDialogue;
    [SerializeField] private NPCDialogue buySellDialogue;

    [Header("Shop UI Elements")]
    [SerializeField] private GameObject shopUIRoot;
    [SerializeField] private ShopUIController shopUIController;
    [SerializeField] private GameObject shopFirstSelectedButton;

    [Header("Shop Stock")]
    [SerializeField] private ShopStock shopStock;

    private PlayerControl player;
    private Controls controls;
    [SerializeField] private NavMeshSurface enemyPatrolSurface;
    private bool isDialogueActive = false;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerControl>();
        controls = new Controls();
    }

    private void OnEnable()
    {
        if (greetingDialogue != null)
            greetingDialogue.onChoiceMade += HandleGreetingChoice;

        if (buySellDialogue != null)
            buySellDialogue.onChoiceMade += HandleBuySellChoice;
    }

    private void OnDisable()
    {
        if (greetingDialogue != null)
            greetingDialogue.onChoiceMade -= HandleGreetingChoice;

        if (buySellDialogue != null)
            buySellDialogue.onChoiceMade -= HandleBuySellChoice;
    }

    private void OnTriggerStay(Collider other)
    {
        if (player.isInteracting && !DialogueManager.Instance.IsDialogueActive() && !isDialogueActive)
        {
            StartInteraction();
        }
    }

    private void StartInteraction()
    {
        isDialogueActive = true;
        currentState = ShopState.Intro;
        greetingDialogue.TriggerDialogue();
    }

    void HandleGreetingChoice(bool wantToShop)
    {
        if (currentState != ShopState.Intro) return;

        if (wantToShop)
        {
            currentState = ShopState.Decision;
            buySellDialogue.TriggerDialogue();
        }
        else
        {
            currentState = ShopState.Finished;
            isDialogueActive = false;
        }
    }

    void HandleBuySellChoice(bool isBuying)
    {
        if (currentState != ShopState.Decision) return;

        if (isBuying)
        {
            OpenShopUI();
        }
        else
        {
            OpenSellUI();
        }

        currentState = ShopState.Finished;
        isDialogueActive = false;
    }

    private void OpenShopUI()
    {
        if (shopUIRoot != null)
        {
            DialogueManager.Instance.isExternalUILocked = true;
            shopUIRoot.SetActive(true);

            player.DisableControls();
            controls.Player.Disable();
            enemyPatrolSurface.enabled = false;

            shopUIController.OpenShop(shopStock);

            if (shopFirstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(shopFirstSelectedButton);
            }
        }
    }

    private void OpenSellUI()
    {
        InventoryUIController invUI = FindFirstObjectByType<InventoryUIController>();
        if (invUI != null)
        {
            DialogueManager.Instance.isExternalUILocked = true;
            invUI.RefreshItemUI();
            invUI.ShowItemsInventory();
            invUI.SendMessage("OpenInventory", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void CloseShopUI()
    {
        DialogueManager.Instance.isExternalUILocked = false;
        shopUIRoot.SetActive(false);
        player.EnableControls();
        controls.Player.Enable();
        enemyPatrolSurface.enabled = true;
        isDialogueActive = false;
    }
}