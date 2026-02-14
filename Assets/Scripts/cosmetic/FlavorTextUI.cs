using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class FlavorTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private AudioSource typeSound;
    [SerializeField] private float letterDelay = 0.05f;

    private Coroutine typingCoroutine;

    public IEnumerator ShowTextCoroutine(string message)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        text.text = "";
        typingCoroutine = StartCoroutine(TypeText(message));
        yield return typingCoroutine;
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

            yield return new WaitForSeconds(letterDelay);
        }

        typingCoroutine = null;
    }
}