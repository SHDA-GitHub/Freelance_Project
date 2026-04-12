using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pokeapi : MonoBehaviour
{
    public TMPro.TMP_InputField inputField;
    public Image Preview;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();    
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (inputField.text.Length > 0)
            {
                StartCoroutine(CheckApi());
            }
        }
    }

    private IEnumerator CheckApi()
    {
        PokeData data;
        using (UnityWebRequest request = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon/" + inputField.text))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            data = JsonUtility.FromJson<PokeData>(request.downloadHandler.text);
            Debug.Log(data.name);
            Debug.Log(data.sprites.front_default);
        }
        if (data != null)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(data.sprites.front_default))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    yield break;
                }
                Texture2D preview = DownloadHandlerTexture.GetContent(request);
                Preview.sprite = Sprite.Create(preview, new Rect(0, 0, preview.width, preview.height), new Vector2(0.5f, 0.5f));
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(data.cries.latest, AudioType.OGGVORBIS))
            {
                yield return request.SendWebRequest();
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    [Serializable]
    public class PokeData
    {
        public string name;
        public PokeSprite sprites;
        public PokeCry cries;
    }

    [Serializable]
    public class PokeSprite//Male
    {
        public string front_default;
    }

    [Serializable]
    public class PokeCry
    {
        public string latest;
    }

    //public class PokeSpriteFemale
    //{
    //    public string front_female;
    //}

    //public class PokeSpriteShiny
    //{
    //    public string front_shiny;
    //}

    //public class PokeSpriteShinyFemale
    //{
    //    public string front_shiny_female;
    //}
}
