using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

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

        controls.UI.Cancel.performed += OnBackspacePressed;
    }

    private void OnEnable()
    {
        if (greetingDialogue != null)
            greetingDialogue.onChoiceMade += HandleGreetingChoice;

        if (buySellDialogue != null)
            buySellDialogue.onChoiceMade += HandleBuySellChoice;

        controls.UI.Enable();
    }

    private void OnDisable()
    {
        if (greetingDialogue != null)
            greetingDialogue.onChoiceMade -= HandleGreetingChoice;

        if (buySellDialogue != null)
            buySellDialogue.onChoiceMade -= HandleBuySellChoice;

        controls.UI.Disable();
    }

    private void OnBackspacePressed(InputAction.CallbackContext context)
    {
        if (shopUIRoot.activeSelf)
        {
            CloseShopUI();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
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

    void HandleGreetingChoice(string decision)
    {
        if (currentState != ShopState.Intro) return;

        if (decision == "wantToShop")
        {
            currentState = ShopState.Decision;
            buySellDialogue.TriggerDialogue();
        }
        else
        {
            currentState = ShopState.Finished;
        }
    }

    void HandleBuySellChoice(string choiceID)
    {
        if (currentState != ShopState.Decision) return;

        if (choiceID == "yes")
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
        enemyPatrolSurface.enabled = true;
        isDialogueActive = false;

        if (currentState == ShopState.Finished)
        {
            currentState = ShopState.Decision;

            var lines = buySellDialogue.GetDialogueLines();

            if (lines != null && lines.Length > 1)
            {
                DialogueManager.Instance.StartDialogue(buySellDialogue, new NPCDialogue.DialogueLine[] { lines[1] }, buySellDialogue.GetDialogueMusic());
            }
            else
            {
                buySellDialogue.TriggerDialogue();
            }
        }
    }
}