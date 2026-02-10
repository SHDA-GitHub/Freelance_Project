using UnityEngine;

public class FollowPivot : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public float followSpeed = 25f;

    void LateUpdate()
    {
        if (!target) return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position + offset,
            Time.deltaTime * followSpeed
        );
    }
}
