using System.Collections;
using TMPro;
using UnityEngine;

public class FlavorTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private AudioSource typeSound;
    [SerializeField] private float letterDelay = 0.05f;

    private float speedMultiplier = 2f;
    private Coroutine typingCoroutine;

    public void SetFastMode(bool fast)
    {
        speedMultiplier = fast ? 2f : 1f;
    }

    public IEnumerator ShowTextCoroutine(string message)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        text.text = "";
        typingCoroutine = StartCoroutine(TypeText(message));

        yield return new WaitUntil(() => typingCoroutine == null);
    }

    public void ShowImmediateText(string message)
    {
        StopAllCoroutines();
        text.text = message;
    }

    private IEnumerator TypeText(string message)
    {
        text.text = "";

        foreach (char letter in message)
        {
            text.text += letter;

            if (typeSound != null)
                typeSound.Play();

            float adjustedDelay = letterDelay / speedMultiplier;
            yield return new WaitForSeconds(adjustedDelay);
        }

        typingCoroutine = null;
    }
}