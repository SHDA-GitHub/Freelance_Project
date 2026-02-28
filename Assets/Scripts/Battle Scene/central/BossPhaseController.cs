using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhaseController : MonoBehaviour
{
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();
    [SerializeField] private SpriteRenderer flashOverlay;
    [SerializeField] private float flashSpeed = 0.05f;
    [SerializeField] private int flashCount = 6;

    private CharacterStats stats;
    private int currentPhaseIndex = 0;
    private bool isTransitioning = false;

    private AudioSource musicSource;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();

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

    private IEnumerator HandlePhaseTransition(BossPhase phase)
    {
        yield return TurnManager.Instance.flavorTextUI
            .ShowTextCoroutine($"{stats.characterName} is changing form...");
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(FlashWhite());

        BackgroundManager bgManager = FindFirstObjectByType<BackgroundManager>();
        if (bgManager != null)
        {
            bgManager.OverrideBackground(phase.backgroundPrefab);
        }

        ChangeMusic(phase);

        yield return TurnManager.Instance.flavorTextUI
            .ShowTextCoroutine($"{stats.characterName} became {phase.phaseName}");
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

    private IEnumerator FlashWhite()
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
}