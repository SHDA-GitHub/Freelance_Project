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

    [Header("Managers")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameManager gameManager;

    [Header("Deck Builder Guard")]
    [SerializeField] private TextMeshProUGUI deckBuilderWarningText;
    [SerializeField] private string deckBuilderSceneName = "Deck Builder";

    private void Awake()
    {
        fade = FindFirstObjectByType<FadeScript>();
        if (deckBuilderWarningText != null)
            deckBuilderWarningText.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        TryChangeScene(StartGameSequence());
    }

    public void BackToMenu()
    {
        TryChangeScene(BackToMenuSequence());
    }

    public void StartTutotrial()
    {
        TryChangeScene(StartTutorialSequence());
    }

    public void DeckBuilder()
    {
        TryChangeScene(DeckBuilderSequence());
    }

    public void Quit()
    {
        StartCoroutine(QuitSequence());
    }

    public void Forteit()
    {
        gameManager.ForfeitPlayerTurn();
    }

    public void nextTutorial()
    {
        tutorialManager.flipPageForward();
    }

    public void previousTutorial()
    {
        tutorialManager.flipPageBackward();
    }

    private void TryChangeScene(IEnumerator sequence)
    {
        if (!CanLeaveDeckBuilder())
            return;

        StartCoroutine(sequence);
    }

    private bool CanLeaveDeckBuilder()
    {
        if (SceneManager.GetActiveScene().name != deckBuilderSceneName)
            return true;
        if (GameObject.FindGameObjectWithTag("CardSlotOpen") != null)
        {
            ShowDeckBuilderWarning();
            return false;
        }
        return true;
    }

    private void ShowDeckBuilderWarning()
    {
        if (deckBuilderWarningText == null)
        { return; }
        deckBuilderWarningText.text = ("Thou cannot enter the heat of battle without taking all the proper precautions!\nThine deck is incomplete!");
        deckBuilderWarningText.gameObject.SetActive(true);
    }

    private IEnumerator StartGameSequence()
    {
        PlayConfirmEffect();
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Main");
    }

    private IEnumerator StartTutorialSequence()
    {
        PlayConfirmEffect();
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Tutorial");
    }

    private IEnumerator BackToMenuSequence()
    {
        PlayConfirmEffect();
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Title Screen");
    }

    private IEnumerator DeckBuilderSequence()
    {
        PlayConfirmEffect();
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Deck Builder");
    }

    private IEnumerator QuitSequence()
    {
        PlayConfirmEffect();
        yield return fade.ImageFadeInFlash();
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
