using System.Collections;
using UnityEngine;

public class SmoothMove : MonoBehaviour
{
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float moveDistance = 5f;

    private Vector3 startPos;
    private Vector3 endPos;

    private void Start()
    {
        startPos = transform.position;
        endPos = new Vector3(transform.position.x, transform.position.y + moveDistance, transform.position.z);

        StartCoroutine(MoveObjectSmoothly());
    }

    private IEnumerator MoveObjectSmoothly()
    {
        float timeElapsed = 0f;

        while (timeElapsed < moveDuration)
        {
            float t = timeElapsed / moveDuration;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, endPos, smoothStep);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }
}