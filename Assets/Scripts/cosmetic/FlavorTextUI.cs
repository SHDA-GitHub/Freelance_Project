using UnityEngine;
using TMPro;

public class FlavorTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void ShowPlayText(string message)
    {
        text.text = message;
    }

    public void ShowCardInfoText(string message)
    {
        text.text = message;
    }
}
