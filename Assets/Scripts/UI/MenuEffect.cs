using UnityEngine;
using System.Collections;

public class MenuEffect : MonoBehaviour
{
    [SerializeField] private AudioClip introSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Camera Shake")]
    [SerializeField] private Transform battleCamera;
    [SerializeField] private float shakeAmount = 1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeSpeed = 0.02f;

    [Header("Sprite Stretch")]
    [SerializeField] private Transform stretchSprite;
    [SerializeField] private float stretchDuration = 0.1f;
    [SerializeField] private float targetXScale = 10f;

    private Vector3 originalCameraPosition;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Boom"))
        {
            StartCoroutine(ShakeCamera());

            if (stretchSprite != null)
            {
                StartCoroutine(StretchSprite());
            }
        }
    }

    public IEnumerator ShakeCamera()
    {
        if (battleCamera == null)
            yield break;

        audioSource.clip = introSound;
        audioSource.Play();
        originalCameraPosition = battleCamera.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetY = Random.Range(-shakeAmount, shakeAmount);
            battleCamera.localPosition = new Vector3(
                originalCameraPosition.x,
                originalCameraPosition.y + offsetY,
                originalCameraPosition.z
            );

            elapsed += shakeSpeed;
            yield return new WaitForSeconds(shakeSpeed);
        }

        battleCamera.localPosition = originalCameraPosition;
    }

    private IEnumerator StretchSprite()
    {
        Vector3 startScale = new Vector3(0f, stretchSprite.localScale.y, stretchSprite.localScale.z);
        Vector3 endScale = new Vector3(targetXScale, stretchSprite.localScale.y, stretchSprite.localScale.z);
        float elapsed = 0f;

        stretchSprite.localScale = startScale;

        while (elapsed < stretchDuration)
        {
            stretchSprite.localScale = Vector3.Lerp(startScale, endScale, elapsed / stretchDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        stretchSprite.localScale = endScale;
    }
}