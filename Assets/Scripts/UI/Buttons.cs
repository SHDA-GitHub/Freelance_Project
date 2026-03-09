using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [Header("Audio / Visual")]
    [SerializeField] private FadeScript fade;
    [SerializeField] private AudioClip confirm;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        fade = FindFirstObjectByType<FadeScript>();
    }

    public void StartGame()
    {
        TryChangeScene(StartGameSequence());
    }

    public void BackToMenu()
    {
        TryChangeScene(BackToMenuSequence());
    }

    public void Quit()
    {
        StartCoroutine(QuitSequence());
    }

    private void TryChangeScene(IEnumerator sequence)
    {
        StartCoroutine(sequence);
    }


    private IEnumerator StartGameSequence()
    {
        PlayConfirmEffect();
        yield return fade.SpriteFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Overworld");
    }

    private IEnumerator BackToMenuSequence()
    {
        PlayConfirmEffect();
        yield return fade.SpriteFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Title Screen");
    }

    private IEnumerator QuitSequence()
    {
        PlayConfirmEffect();
        yield return fade.SpriteFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        Application.Quit();
        Debug.Log("Quit!");
    }

    private void PlayConfirmEffect()
    {
        if (audioSource != null && confirm != null)
            audioSource.PlayOneShot(confirm);
    }
}
