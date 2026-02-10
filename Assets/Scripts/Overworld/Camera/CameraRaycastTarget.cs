using UnityEngine;

public class CameraRaycastTarget : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform playerPivot;
    public Transform cameraRig;

    [Header("Raycast Settings")]
    public float rayDistance = 100f;
    public LayerMask obstacleLayers;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;
    public float minYawOffset = 90f;
    public float maxYawOffset = 180f;

    private float remainingRotation;
    private int rotationDirection;
    private bool isRotating;

    void Update()
    {
        CheckLineOfSight();
        RotateAroundPlayer();
    }

    void CheckLineOfSight()
    {
        Vector3 origin = cameraRig.position;
        Vector3 direction = player.position - origin;
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance))
        {
            if (!hit.collider.CompareTag("Player") && !isRotating)
            {
                StartRotation();
            }
        }
    }

    void StartRotation()
    {
        float angle = Random.Range(minYawOffset, maxYawOffset);
        rotationDirection = Random.value > 0.5f ? 1 : -1;
        remainingRotation = angle;
        isRotating = true;
    }

    void RotateAroundPlayer()
    {
        if (!isRotating) return;

        float step = rotationSpeed * Time.deltaTime;
        float applied = Mathf.Min(step, remainingRotation);

        cameraRig.RotateAround(
            playerPivot.position,
            Vector3.up,
            applied * rotationDirection
        );

        remainingRotation -= applied;

        if (remainingRotation <= 0f)
        {
            isRotating = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!player || !cameraRig) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(cameraRig.position, player.position);
    }
}