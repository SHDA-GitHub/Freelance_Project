using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Flickering : MonoBehaviour
{
    public Image targetImage;
    public TextMeshProUGUI targetText;
    public SpriteRenderer targetSprite;
    public float flickerSpeed = 1f;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;

    private float alpha;
    private bool fadingOut = true;
    public bool isImage = false;
    public bool isText = false;
    public bool isSprite = false;

    void Update()
    {
        if (isImage)
        {
            if (targetImage == null) return;

            Color color = targetImage.color;

            if (fadingOut)
            {
                alpha -= Time.deltaTime * flickerSpeed;
                if (alpha <= minAlpha)
                {
                    alpha = minAlpha;
                    fadingOut = false;
                }
            }
            else
            {
                alpha += Time.deltaTime * flickerSpeed;
                if (alpha >= maxAlpha)
                {
                    alpha = maxAlpha;
                    fadingOut = true;
                }
            }

            color.a = alpha;
            targetImage.color = color;
        }
        if (isText)
        {
            if (targetText == null) return;

            Color color = targetText.color;

            if (fadingOut)
            {
                alpha -= Time.deltaTime * flickerSpeed;
                if (alpha <= minAlpha)
                {
                    alpha = minAlpha;
                    fadingOut = false;
                }
            }
            else
            {
                alpha += Time.deltaTime * flickerSpeed;
                if (alpha >= maxAlpha)
                {
                    alpha = maxAlpha;
                    fadingOut = true;
                }
            }

            color.a = alpha;
            targetText.color = color;
        }
        if (isSprite)
        {
            if (targetSprite == null) return;

            Color color = targetSprite.color;

            if (fadingOut)
            {
                alpha -= Time.deltaTime * flickerSpeed;
                if (alpha <= minAlpha)
                {
                    alpha = minAlpha;
                    fadingOut = false;
                }
            }
            else
            {
                alpha += Time.deltaTime * flickerSpeed;
                if (alpha >= maxAlpha)
                {
                    alpha = maxAlpha;
                    fadingOut = true;
                }
            }

            color.a = alpha;
            targetSprite.color = color;
        }
    }

    void Start()
    {
        if (targetImage != null && isImage == true)
          { alpha = targetImage.color.a; }
        else if (targetImage == null && isImage == true)
          { Debug.LogWarning("No Image assigned."); }

        if (targetText != null && isText == true)
          { alpha = targetText.color.a; }
        else if (targetText == null && isText == true)
          { Debug.LogWarning("No Text assigned."); }

        if (targetSprite != null && isSprite == true)
        { alpha = targetSprite.color.a; }
        else if (targetSprite == null && isSprite == true)
          { Debug.LogWarning("No Sprite assigned."); }
    }
}