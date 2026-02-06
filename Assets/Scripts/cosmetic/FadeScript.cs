using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class FadeScript : MonoBehaviour
{
    public Transform screenOverlay;
    public float flashDuration = 1f;
    public bool isDropshadow = false;
    public bool startFade = false;
    public bool isImage = false;
    public bool isText = false;
    public bool isSprite = false;
    public float rColor = 0;
    public float gColor = 0;
    public float bColor = 0;

    Image image;
    TextMeshProUGUI text;
    SpriteRenderer sprite;

    void Start()
    {
        image = screenOverlay.GetComponent<Image>();
        text = screenOverlay.GetComponent<TextMeshProUGUI>();
        sprite = screenOverlay.GetComponent<SpriteRenderer>();
        if (startFade == true && isImage == true)
        {
            StartCoroutine(ImageFadeOutFlash());
        }
        if (startFade == true && isText == true)
        {
            StartCoroutine(TextFadeOutFlash());
        }
        if (startFade == true && isSprite == true)
        {
            StartCoroutine(SpriteFadeOutFlash());
        }
    }

    public IEnumerator ImageFadeInFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                image.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            image.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                image.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            image.color = endColor;
        }


    }

    public IEnumerator ImageFadeOutFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                image.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            image.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                image.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            image.color = endColor;
        }
    }

    public IEnumerator TextFadeInFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                text.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            text.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                text.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            text.color = endColor;
        }


    }

    public IEnumerator TextFadeOutFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                text.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            text.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                text.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            text.color = endColor;
        }
    }

    public IEnumerator SpriteFadeInFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                sprite.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            sprite.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 0);
            Color endColor = new Color(rColor, gColor, bColor, 1);
            float t = 0f;

            while (t < flashDuration)
            {
                sprite.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            sprite.color = endColor;
        }


    }

    public IEnumerator SpriteFadeOutFlash()
    {
        if (isDropshadow == false)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                sprite.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            sprite.color = endColor;
        }
        else if (isDropshadow == true)
        {
            Color startColor = new Color(rColor, gColor, bColor, 1);
            Color endColor = new Color(rColor, gColor, bColor, 0);

            float t = 0f;

            while (t < flashDuration)
            {
                sprite.color = Color.Lerp(startColor, endColor, t / flashDuration);
                t += Time.deltaTime;
                yield return null;
            }

            sprite.color = endColor;
        }
    }
}
