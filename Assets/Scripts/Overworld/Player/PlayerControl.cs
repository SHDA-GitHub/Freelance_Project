using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class PlayerControl : MonoBehaviour
{
    [Header("Playerfollow")]
    public List<PlayerSnapshot> history = new List<PlayerSnapshot>();
    [SerializeField] private float historyDuration = 5f;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform cameraPivotPoint;

    [Header("Player movement settings")]
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float fallMultiplier = 3.5f;

    [Header("Player groundcheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Player Animation")]
    [SerializeField] private Animator animatorFront;
    [SerializeField] private Animator animatorBack;

    [Header("Camera rotation")]
    [SerializeField] private float rotationCooldown = 0.5f;
    private float lastRotationTime = -Mathf.Infinity;
    [SerializeField] private float cameraRotationSpeed = 10f;
    private Quaternion targetCameraRotation;

    public bool controlsEnabled = true;
    public bool rotated = false;
    private float lastZDirection = 1f;
    public bool isInteracting = false;
    private Controls controls;
    private bool isGrounded;
    private Rigidbody rb;
    private float originalSpeed;
    private Vector3 m_Movement;

    private void Start()
    {
        isInteracting = false;
        targetCameraRotation = cameraPivotPoint.rotation;
        cameraPivotPoint.position = cameraPivotPoint.position;
        cameraPivotPoint.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void Awake()
    {
        controls = new Controls();
        controls.Player.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMoveCancel;
        controls.Player.Sprint.performed += OnSprint;
        controls.Player.Sprint.canceled += OnSprintCancel;
        controls.Player.Jump.performed += OnJump;
        controls.Player.Interact.performed += OnInteract;

        rb = GetComponent<Rigidbody>();
        originalSpeed = m_Speed;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;

        Vector2 input = context.ReadValue<Vector2>();
        m_Movement = new Vector3(input.x, 0, input.y);
    }

    private void OnMoveCancel(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;
        m_Movement = Vector3.zero;
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;
        m_Speed = originalSpeed * 1.5f;
    }

    private void OnSprintCancel(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;
        m_Speed = originalSpeed;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        StartCoroutine(interactRoutine());
    }

    IEnumerator interactRoutine()
    {
        isInteracting = true;
        yield return new WaitForSecondsRealtime(0.35f);
        isInteracting = false;
    }

    public void EnableControls()
    {
        controlsEnabled = true;
        if (!controlsEnabled) return;
        m_Speed = originalSpeed;
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        m_Movement = Vector3.zero;
        if (!controlsEnabled) return;
        m_Speed = originalSpeed;
    }

    void FixedUpdate()
    {
        if (cameraPivotPoint != null)
        {
            cameraPivotPoint.position = transform.position;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (m_Movement != Vector3.zero)
        {
            Vector3 camForward = playerCamera.forward;
            Vector3 camRight = playerCamera.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * m_Movement.z) + (camRight * m_Movement.x);
            moveDirection.Normalize();

            Vector3 moveOffset = moveDirection * m_Speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveOffset);

            Vector3 localMove = transform.InverseTransformDirection(moveDirection);

            animatorFront.SetFloat("BlendX", Mathf.Clamp(localMove.x, -1f, 1f));
            animatorFront.SetFloat("BlendY", Mathf.Clamp(localMove.z, -1f, 1f));
            animatorBack.SetFloat("BlendX", Mathf.Clamp(localMove.x, -1f, 1f));
            animatorBack.SetFloat("BlendY", Mathf.Clamp(localMove.z, -1f, 1f));
        }
        else
        {
            animatorFront.SetFloat("BlendX", 0f);
            animatorFront.SetFloat("BlendY", 0f);
            animatorBack.SetFloat("BlendX", 0f);
            animatorBack.SetFloat("BlendY", 0f);
        }

        if (cameraPivotPoint != null)
        {
            cameraPivotPoint.rotation = Quaternion.Slerp(
                cameraPivotPoint.rotation,
                targetCameraRotation,
                Time.fixedDeltaTime * cameraRotationSpeed
            );
        }

        if (rotated == false)
        {
            if (Mathf.Abs(m_Movement.z) > 0.01f)
            {
                lastZDirection = Mathf.Sign(m_Movement.z);
            }
        }
        else if (rotated == true)
        {
            if (Mathf.Abs(m_Movement.x) > 0.01f)
            {
                lastZDirection = Mathf.Sign(m_Movement.x);
            }
        }

        float baseRotation = (lastZDirection > 0) ? 0f : 180f;

        float rotationOffset = rotated ? 90f : 0f;

        float targetYRotation = baseRotation + rotationOffset;

        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.fixedDeltaTime;
        }

        float bx = animatorFront.GetFloat("BlendX");
        float by = animatorFront.GetFloat("BlendY");

        history.Add(new PlayerSnapshot(
            transform.position,
            transform.rotation,
            bx,
            by,
            !isGrounded,
            m_Speed > originalSpeed
        ));

        float maxTime = historyDuration / Time.fixedDeltaTime;

        if (history.Count > maxTime)
        {
            history.RemoveAt(0);
        }
    }

    public void SetRotated(bool value)
    {
        if (Time.time < lastRotationTime + rotationCooldown)
            return;

        if (rotated == value)
            return;

        rotated = value;
        lastRotationTime = Time.time;

        float rotationY = rotated ? 90f : 0f;
        SetCameraPivotRotation(rotationY);
    }

    public void SetCameraPivotRotation(float rotationY)
    {
        if (cameraPivotPoint != null)
        {
            targetCameraRotation = Quaternion.Euler(0f, rotationY, 0f);
        }
    }

    public void ResetCameraPivotRotation()
    {
        if (cameraPivotPoint != null)
        {
            targetCameraRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}