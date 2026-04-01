using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pokeapi : MonoBehaviour
{
    public TMPro.TMP_InputField inputField;
    public Image Preview;

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
        using(UnityWebRequest request = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon/" + inputField.text))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            PokeData data;
            data = JsonUtility.FromJson<PokeData>(request.downloadHandler.text);
            Debug.Log(data.name);
        }
    }

    public class PokeData
    {
        public string name;
    }
}
