using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI effectNameText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void SetEffect(Sprite icon, string effectName, string playerName)
    {
        iconImage.sprite = icon;
        effectNameText.text = effectName;
        playerNameText.text = playerName;
    }

    public void ClearEffect()
    {
        iconImage.sprite = null;
        effectNameText.text = "";
        playerNameText.text = "";
        gameObject.SetActive(false);
    }
}