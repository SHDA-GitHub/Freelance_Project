using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreLoadingScript : MonoBehaviour
{
    [SerializeField] private AudioClip[] musicTracks;

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
    }

    void Update()
    {
        
    }
}
