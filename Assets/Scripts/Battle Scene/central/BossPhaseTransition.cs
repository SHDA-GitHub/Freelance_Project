using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BossPhaseTransition : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float fillSpeed = 2f;
    [SerializeField] private float delayBeforeFade = 0.4f;
    [SerializeField] private float delayBeforeSceneLoad = 0.9f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transitionSound;

    [SerializeField] private List<BattleTransition> transitions;
    private FadeScript fade;
    private bool transitionPlaying = false;

    void Start()
    {
        fade = FindFirstObjectByType<FadeScript>();
        foreach (var t in transitions)
            t.rootObject.SetActive(false);
    }

    public void playTransition(BattleTransitionType transitionType)
    {
        StartCoroutine(EnemyTransition(transitionType));
    }

    private IEnumerator EnemyTransition(BattleTransitionType t)
    {
        StartCoroutine(PlayTransition(t));
        yield return new WaitForSeconds(delayBeforeFade + delayBeforeSceneLoad + 0.45f);
        StartCoroutine(ResetTransitionCoroutine());
    }

    private IEnumerator PlayTransition(BattleTransitionType type)
    {
        transitionPlaying = true;
        BattleTransition transition = transitions.Find(t => t.type == type);

        if (transition == null)
        {
            Debug.LogWarning("No transition found for " + type);
            yield break;
        }

        if (MusicManager.Instance != null)
            MusicManager.Instance.FadeOutMusic();

        yield return new WaitForSeconds(0.1f);

        transition.rootObject.SetActive(true);

        foreach (var img in transition.fillImages)
            img.fillAmount = 0f;

        if (audioSource != null && transition.introSound != null)
            audioSource.PlayOneShot(transition.introSound);

        bool finished = false;

        while (!finished)
        {
            finished = true;

            foreach (var img in transition.fillImages)
            {
                img.fillAmount += Time.deltaTime * fillSpeed;

                if (img.fillAmount < 1f)
                    finished = false;
            }

            yield return null;
        }

        yield return new WaitForSeconds(delayBeforeFade);

        if (fade != null)
            yield return fade.SpriteFadeInFlash();
    }

    public bool TransitionPlaying()
    {
        return transitionPlaying;
    }

    private IEnumerator ResetTransitionCoroutine()
    {
        if (audioSource != null)
            audioSource.Stop();

        foreach (var transition in transitions)
        {
            transition.rootObject.SetActive(true);

            foreach (var img in transition.fillImages)
            {
                img.fillAmount = 0f;
                transitionPlaying = false;
            }
        }

        if (fade != null)
        {
            yield return fade.SpriteFadeOutFlash();
        }

        yield return new WaitForSeconds(0.2f);

        foreach (var transition in transitions)
        {
            transition.rootObject.SetActive(false);
        }
    }
}
