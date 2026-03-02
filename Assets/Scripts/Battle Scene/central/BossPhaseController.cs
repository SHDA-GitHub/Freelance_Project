using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhaseController : MonoBehaviour
{
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();
    [SerializeField] private SpriteRenderer flashOverlay;
    private BackgroundManager backgroundManager;
    [SerializeField] private float flashSpeed = 0.05f;
    [SerializeField] private int flashCount = 6;

    private CharacterStats stats;
    private int currentPhaseIndex = 0;
    private bool isTransitioning = false;
    public bool newBackground = true;

    private AudioSource musicSource;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        backgroundManager = FindFirstObjectByType<BackgroundManager>();

        if (backgroundManager == null)
        {
            Debug.LogWarning("No BackgroundManager found in scene.");
        }

        if (flashOverlay == null)
        {
            GameObject flashObject = GameObject.Find("Flash");

            if (flashObject != null)
            {
                flashOverlay = flashObject.GetComponent<SpriteRenderer>();

                if (flashOverlay == null)
                {
                    Debug.LogWarning("'Flash' GameObject found but no SpriteRenderer attached.");
                }
            }
            else
            {
                Debug.LogWarning("No GameObject named 'Flash' found in scene.");
            }
        }

        GameObject musicManager = GameObject.Find("MusicManager");
        if (musicManager != null)
        {
            musicSource = musicManager.GetComponent<AudioSource>();
            if (musicSource == null)
            {
                Debug.LogWarning("MusicManager found but no AudioSource attached.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject named 'MusicManager' found in scene.");
        }
    }

    public IEnumerator TryHandlePhaseTransition()
    {
        if (stats.currentHealth <= 0)
            yield break;

        if (isTransitioning) yield break;
        if (currentPhaseIndex >= phases.Count) yield break;

        BossPhase nextPhase = phases[currentPhaseIndex];

        if (stats.currentHealth <= nextPhase.hpThreshold)
        {
            isTransitioning = true;

            yield return StartCoroutine(HandlePhaseTransition(nextPhase));

            currentPhaseIndex++;
            isTransitioning = false;
        }
    }
    private void ChangeBackground(BossPhase phase)
    {
        if (!newBackground) return;
        if (phase.backgroundPrefab == null) return;
        if (backgroundManager == null) return;

        backgroundManager.OverrideBackground(phase.backgroundPrefab);
    }

    private IEnumerator HandlePhaseTransition(BossPhase phase)
    {
        string introText = !string.IsNullOrEmpty(phase.introFlavorText)
            ? FormatPhaseText(phase.introFlavorText, phase)
            : $"{stats.characterName} is changing form...";

        yield return TurnManager.Instance.flavorTextUI
            .ShowTextCoroutine(introText);

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(FlashWhite());

        ChangeMusic(phase);

        ChangeBackground(phase);

        stats.SetInvisible();

        TurnManager.Instance.enemyParty.Remove(stats);

        if (TurnManager.Instance.currentTurn == TurnType.Enemy)
        {
            TurnManager.Instance.currentCharacterIndex =
                Mathf.Max(TurnManager.Instance.currentCharacterIndex - 1, 0);
        }

        Destroy(gameObject);

        string transformText = !string.IsNullOrEmpty(phase.transformFlavorText)
            ? FormatPhaseText(phase.transformFlavorText, phase)
            : $"{stats.characterName} became {phase.phaseName}!";
        
        yield return TurnManager.Instance.flavorTextUI
            .ShowTextCoroutine(transformText);

        yield return new WaitForSeconds(0.3f);
    }

    private void ChangeMusic(BossPhase phase)
    {
        if (musicSource == null) return;
        if (phase.phaseMusic == null) return;

        if (musicSource.clip != phase.phaseMusic)
        {
            musicSource.clip = phase.phaseMusic;
            musicSource.Play();
        }
    }

    public IEnumerator FlashWhite()
    {
        if (flashOverlay == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            flashOverlay.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(flashSpeed);

            flashOverlay.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    private string FormatPhaseText(string template, BossPhase phase)
    {
        if (string.IsNullOrEmpty(template))
            return "";

        return template
            .Replace("{attacker}", stats.characterName)
            .Replace("{phase}", phase.phaseName);
    }
}