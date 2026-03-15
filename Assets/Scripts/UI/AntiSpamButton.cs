using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AntiSpamButton : MonoBehaviour
{
    [Header("Mode")]
    public bool useCooldown = true;

    [Header("Cooldown Settings")]
    public float cooldown = 1f;

    private bool onCooldown = false;
    private bool hasBeenPressed = false;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnButtonPressed()
    {
        if (!useCooldown && hasBeenPressed)
            return;

        if (useCooldown && onCooldown)
            return;

        Debug.Log("Button pressed!");

        if (useCooldown)
        {
            StartCoroutine(Cooldown());
        }
        else
        {
            hasBeenPressed = true;

            if (button != null)
                button.interactable = false;
        }
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;

        if (button != null)
            button.interactable = false;

        yield return new WaitForSeconds(cooldown);

        if (button != null)
            button.interactable = true;

        onCooldown = false;
    }
}