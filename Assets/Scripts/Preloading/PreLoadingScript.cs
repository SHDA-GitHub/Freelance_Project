using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PreLoadingScript : MonoBehaviour
{
    public static PreLoadingScript Instance;
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioClip[] soundEffects;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(LoadAudioData());
    }

    public IEnumerator LoadAudioData()
    {
        foreach (var clip in musicTracks)
        {
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading)
                yield return null;
        }
        foreach (var clip in soundEffects)
        {
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading)
                yield return null;
        }
    }
}
