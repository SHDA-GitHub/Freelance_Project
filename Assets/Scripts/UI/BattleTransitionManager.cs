using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager Instance;

    [Header("Transitions")]
    [SerializeField] private List<BattleTransition> transitions;

    [Header("Timing")]
    [SerializeField] private float fillSpeed = 2f;
    [SerializeField] private float delayBeforeFade = 0.4f;
    [SerializeField] private float delayBeforeSceneLoad = 0.9f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transitionSound;

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "Battle Scene";

    [Header("Movement Control")]
    [SerializeField] private Unity.AI.Navigation.NavMeshSurface enemyPatrolSurface;

    [Header("Current Encounter")]
    [SerializeField] private GameObject currentEnemy;

    private PlayerControl player;
    private Controls controls;
    private FadeScript fade;
    [SerializeField] private AudioClip savedOverworldTrack;
    private float savedPlaybackTime;
    private bool transitionPlaying = false;

    private void Awake()
    {
        Instance = this;
        fade = FindFirstObjectByType<FadeScript>();
        player = FindFirstObjectByType<PlayerControl>();
        controls = new Controls();
    }

    private void Start()
    {
        savedOverworldTrack = MusicManager.Instance.GetCurrentTrack();
        foreach (var t in transitions)
            t.rootObject.SetActive(false);

        if (BattleResultBridge.HasResult && BattleResultBridge.BattleWon)
        {
            if (currentEnemy != null)
            {
                Destroy(currentEnemy);
                currentEnemy = null;
            }

            ResetBattleTransitionForOverworld();
        }
    }

    public void StartBattleTransition(BattleTransitionType type)
    {
        if (transitionPlaying) return;
        transitionPlaying = true;
        StartCoroutine(PlayTransition(type));
    }

    public void RegisterEncounterEnemy(GameObject enemy)
    {
        currentEnemy = enemy;
    }

    private IEnumerator PlayTransition(BattleTransitionType type)
    {
        BattleTransition transition = transitions.Find(t => t.type == type);

        if (transition == null)
        {
            Debug.LogWarning("No transition found for " + type);
            yield break;
        }

        if (player != null)
            player.DisableControls();

        if (enemyPatrolSurface != null)
            enemyPatrolSurface.enabled = false;

        if (MusicManager.Instance != null)
        {
            savedPlaybackTime = MusicManager.Instance.GetPlaybackTime();

            MusicManager.Instance.FadeOutMusic();
        }

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

        yield return new WaitForSeconds(delayBeforeSceneLoad);

        Scene overworld = SceneManager.GetSceneByName("Overworld");
        BattleDataBridge.SaveOverworldState(overworld);
        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
        foreach (GameObject obj in overworld.GetRootGameObjects())
        {
            obj.SetActive(false);
        }
    }

    public void ResetBattleTransitionForOverworld()
    {
        StartCoroutine(ResetTransitionCoroutine());
        controls?.Player.Enable();
        controls?.UI.Enable();

        EventSystem.current.SetSelectedGameObject(null);
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
            }
        }

        if (fade != null)
        {
            yield return fade.SpriteFadeOutFlash();
        }

        if (player != null)
            player.EnableControls();

        enemyPatrolSurface.enabled = true;

        DialogueManager.Instance.isExternalUILocked = false;
        controls.Player.Disable();
        yield return new WaitForSeconds(0.05f);
        controls.Player.Enable();
        controls.UI.Enable();

        InputSystem.ResetHaptics();

        BattleResultBridge.ResetBridge();

        yield return new WaitForSeconds(0.1f);

        if (MusicManager.Instance != null && savedOverworldTrack != null)
        {
            MusicManager.Instance.FadeInMusic();
        }

        yield return new WaitForSeconds(0.1f);

        MusicManager.Instance.PlayTrackFromTime(savedOverworldTrack, savedPlaybackTime);
        foreach (var transition in transitions)
        {
            transition.rootObject.SetActive(false);
        }

        if (BattleResultBridge.HasResult && BattleResultBridge.BattleWon)
        {
            if (currentEnemy != null)
            {
                Destroy(currentEnemy);
                currentEnemy = null;
            }

            ResetBattleTransitionForOverworld();
        }

        transitionPlaying = false;
    }
}