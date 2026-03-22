using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR;

public class ShopNPC : MonoBehaviour
{
    private enum ShopState { Intro, Decision, Finished }
    private ShopState currentState = ShopState.Intro;

    [Header("Dialogue Data")]
    [SerializeField] private NPCDialogue greetingDialogue;

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
    private bool canInteract = true;

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

        controls.UI.Enable();
    }

    private void OnDisable()
    {
        if (greetingDialogue != null)
            greetingDialogue.onChoiceMade -= HandleGreetingChoice;

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
        if (!other.CompareTag("Player"))
            return;

        PlayerControl player = other.GetComponent<PlayerControl>();

        if (!player.isInteracting || !canInteract)
            return;

        if (DialogueManager.Instance.IsDialogueActive() || isDialogueActive)
            return;

        StartInteraction();
    }

    private void StartInteraction()
    {
        isDialogueActive = true;
        canInteract = false;
        greetingDialogue.TriggerDialogue();
    }

    void HandleGreetingChoice(string choiceID)
    {
        StartCoroutine(HandleChoiceAfterDialogue(choiceID));
    }

    IEnumerator HandleChoiceAfterDialogue(string choiceID)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

        switch (choiceID)
        {
            case "buy":
                OpenShopUI();
                yield break;
        }
        isDialogueActive = false;
        yield return new WaitForSeconds(0.2f);
        canInteract = true;
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

    public void CloseShopUI()
    {
        DialogueManager.Instance.isExternalUILocked = false;
        shopUIRoot.SetActive(false);

        player.EnableControls();
        enemyPatrolSurface.enabled = true;

        isDialogueActive = false;
        canInteract = true;
    }
}