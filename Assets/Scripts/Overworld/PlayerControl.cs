using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float m_Speed = 5f;
    private float originalSpeed;
    private Controls controls;
    private Rigidbody rb;
    private Vector3 m_Movement;


    private void Awake()
    {
        controls = new Controls();
        controls.Player.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMoveCancel;
        controls.Player.Sprint.performed += OnSprint;
        controls.Player.Sprint.canceled += OnSprintCancel;
        //controls.Player.Jump.performed += OnJump;

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

    void Start()
    {

    }

    void FixedUpdate()
    {
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

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));

            Vector3 moveOffset = moveDirection * m_Speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveOffset);
        }
    }
}
