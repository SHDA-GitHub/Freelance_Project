using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private Animator animatorFront;
    [SerializeField] private Animator animatorBack;
    private float lastZDirection = 1f;
    [SerializeField] private LayerMask groundMask;
    public bool isInteracting = false;
    private bool isGrounded;
    private float originalSpeed;
    private Controls controls;
    private Rigidbody rb;
    private Vector3 m_Movement;

    private void Start()
    {
        isInteracting = false;
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
        Vector2 input = context.ReadValue<Vector2>();
        m_Movement = new Vector3(input.x, 0, input.y);
    }

    private void OnMoveCancel(InputAction.CallbackContext context) => m_Movement = Vector3.zero;

    private void OnSprint(InputAction.CallbackContext context) => m_Speed = originalSpeed * 1.5f;

    private void OnSprintCancel(InputAction.CallbackContext context) => m_Speed = originalSpeed;

    private void OnJump(InputAction.CallbackContext context)
    {
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
        yield return new WaitForSecondsRealtime(0.1f);
        isInteracting = false;
    }

    void FixedUpdate()
    {
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

        if (Mathf.Abs(m_Movement.z) > 0.01f)
        {
            lastZDirection = Mathf.Sign(m_Movement.z);
        }

        float targetYRotation = (lastZDirection > 0) ? 0f : 180f;
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
    }
}