using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    private float verticalVelocity;
    private bool isGrounded;

    [Header("Referencias")]
    private Animator animator; 

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputActions.Player.Enable();

        inputActions.Player.Jump.performed += OnJumpPerformed;

        RecargarAnimator();
    }

    public void RecargarAnimator()
    {
        animator = GetComponentInChildren<Animator>(false);

        if (animator != null)
        {
            Debug.Log($"<color=green>Animator de {animator.gameObject.name} vinculado con éxito.</color>");
        }
        else
        {
            Debug.LogWarning("No se encontró un Animator activo en los hijos del Player.");
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        isGrounded = transform.position.y <= 0.05f;

        ManejarMovimiento();
        ManejarGravedad();
        ActualizarAnimaciones();
    }

    private void ManejarMovimiento()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        Transform camTransform = Camera.main.transform;
        Vector3 forward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(camTransform.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ManejarGravedad()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);

        if (transform.position.y < 0)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        bool estaMoviendose = moveInput.magnitude > 0.1f;

        animator.SetBool("Idle", !estaMoviendose && isGrounded);
        animator.SetBool("Jog", estaMoviendose && isGrounded);
        animator.SetBool("Jump", !isGrounded && verticalVelocity > 0);
        animator.SetBool("Fall", !isGrounded && verticalVelocity < 0);
    }

    void OnEnable() => inputActions?.Player.Enable();
    void OnDisable() => inputActions?.Player.Disable();
}