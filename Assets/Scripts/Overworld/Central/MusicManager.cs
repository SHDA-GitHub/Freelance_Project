using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    private AudioClip baseTrack;
    private AudioClip overrideTrack;

    [SerializeField] private float fadeDuration = 0.6f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        baseTrack = audioSource.clip;
    }

    public void PlayBaseMusic(AudioClip clip)
    {
        baseTrack = clip;
        PlayTrack(clip);
    }

    public void PlayOverrideMusic(AudioClip clip)
    {
        overrideTrack = clip;
        PlayTrack(clip);
    }

    public void ClearOverrideMusic()
    {
        if (audioSource.clip != baseTrack)
        {
            overrideTrack = null;
            PlayTrack(baseTrack);
        }
    }

    void PlayTrack(AudioClip clip)
    {
        if (clip == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToTrack(clip));
    }

    IEnumerator FadeToTrack(AudioClip newTrack)
    {
        float startVolume = audioSource.volume;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;

        audioSource.clip = newTrack;
        audioSource.Play();

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }

    public AudioClip GetCurrentTrack()
    {
        return audioSource.clip;
    }

    public void FadeOutMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutOnly());
    }

    public void FadeInMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInOnly());
    }

    IEnumerator FadeOutOnly()
    {
        float startVolume = audioSource.volume;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
    }

    IEnumerator FadeInOnly()
    {
        float startVolume = audioSource.volume;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 4.5f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 4.5f;
    }

    public float GetPlaybackTime()
    {
        return audioSource.time;
    }

    public void PlayTrackFromTime(AudioClip clip, float time)
    {
        if (clip == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToTrackFromTime(clip, time));
    }

    IEnumerator FadeToTrackFromTime(AudioClip newTrack, float time)
    {
        float startVolume = audioSource.volume;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;

        audioSource.clip = newTrack;
        audioSource.time = time;
        audioSource.Play();

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}