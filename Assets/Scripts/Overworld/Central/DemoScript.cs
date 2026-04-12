using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoScript : MonoBehaviour
{
    PlayerControl player;

    [SerializeField] private Transform targetParent;
    [SerializeField] private float fadeDuration = 1f;

    void Start()
    {
        player = FindFirstObjectByType<PlayerControl>();
    }

    public void EndDemo()
    {
        player.controls.UI.Disable();
        player.controls.Player.Disable();
        FadeIn();
    }

    public void FadeIn()
    {
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        Graphic[] graphics = targetParent.GetComponentsInChildren<Graphic>(true);
        SpriteRenderer[] sprites = targetParent.GetComponentsInChildren<SpriteRenderer>(true);

        float t = 0f;

        foreach (var g in graphics)
        {
            Color c = g.color;
            c.a = startAlpha;
            g.color = c;
        }

        foreach (var s in sprites)
        {
            Color c = s.color;
            c.a = startAlpha;
            s.color = c;
        }

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);

            foreach (var g in graphics)
            {
                Color c = g.color;
                c.a = alpha;
                g.color = c;
            }

            foreach (var s in sprites)
            {
                Color c = s.color;
                c.a = alpha;
                s.color = c;
            }

            t += Time.deltaTime;
            yield return null;
        }

        foreach (var g in graphics)
        {
            Color c = g.color;
            c.a = endAlpha;
            g.color = c;
        }

        foreach (var s in sprites)
        {
            Color c = s.color;
            c.a = endAlpha;
            s.color = c;
        }
    }
}
