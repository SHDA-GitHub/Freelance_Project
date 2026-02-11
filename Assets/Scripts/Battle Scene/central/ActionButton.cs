using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ActionButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;
    private ScriptableObject actionData;
    private Action<ScriptableObject> onClickCallback;

    public void Setup(ScriptableObject action, Action<ScriptableObject> callback)
    {
        actionData = action;
        onClickCallback = callback;

        if (buttonText != null)
            buttonText.text = action.name;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(actionData));
    }
}
