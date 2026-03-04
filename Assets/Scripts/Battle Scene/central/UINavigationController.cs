using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UINavigationController : MonoBehaviour
{
    public static UINavigationController Instance;

    private Controls controls;

    private List<Button> currentButtons = new List<Button>();
    private int currentIndex = 0;
    private GameObject currentMenuRoot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new Controls();
        controls.UI.Enable();
    }

    private void Update()
    {
        if (currentMenuRoot == null || currentButtons.Count == 0)
            return;

        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    public void SetActiveMenu(GameObject menuRoot)
    {
        currentMenuRoot = menuRoot;
        RefreshButtons();
    }

    public void ClearMenu()
    {
        currentMenuRoot = null;
        currentButtons.Clear();
        currentIndex = 0;
    }

    private void RefreshButtons()
    {
        currentButtons.Clear();

        if (currentMenuRoot == null) return;

        Button[] buttons = currentMenuRoot.GetComponentsInChildren<Button>(true);

        foreach (var btn in buttons)
        {
            if (btn.gameObject.activeInHierarchy && btn.interactable)
                currentButtons.Add(btn);
        }

        currentIndex = 0;

        if (currentButtons.Count > 0)
            SelectButton(currentIndex);
    }

    private void HandleNavigation()
    {
        if (!controls.UI.Navigate.triggered)
            return;

        Vector2 input = controls.UI.Navigate.ReadValue<Vector2>();

        if (input.y > 0)
            currentIndex--;
        else if (input.y < 0)
            currentIndex++;

        if (currentIndex < 0)
            currentIndex = currentButtons.Count - 1;

        if (currentIndex >= currentButtons.Count)
            currentIndex = 0;

        SelectButton(currentIndex);
    }

    private void HandleSubmit()
    {
        if (controls.UI.Submit.triggered)
        {
            currentButtons[currentIndex].onClick.Invoke();
        }
    }

    private void HandleCancel()
    {
        if (controls.UI.Cancel.triggered)
        {
            UIManager.Instance.ShowPlayerOptions(
                TurnManager.Instance.GetCurrentPlayer()
            );
        }
    }

    private Button lastButton;

    private void SelectButton(int index)
    {
        if (index < 0 || index >= currentButtons.Count)
            return;

        if (lastButton != null)
        {
            ExecuteEvents.Execute(
                lastButton.gameObject,
                new PointerEventData(EventSystem.current),
                ExecuteEvents.pointerExitHandler
            );
        }

        Button btn = currentButtons[index];

        EventSystem.current.SetSelectedGameObject(btn.gameObject);

        ExecuteEvents.Execute(
            btn.gameObject,
            new PointerEventData(EventSystem.current),
            ExecuteEvents.pointerEnterHandler
        );

        lastButton = btn;
    }
}