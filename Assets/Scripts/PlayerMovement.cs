using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private CharacterController controller;

    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 6f;
    public float gravity = -18f; // Gravedad un poco más fuerte para evitar el efecto "luna"

    [Header("Detección de Suelo (Raycast)")]
    public float groundCheckDistance = 0.3f; // Distancia extra debajo de los pies
    public LayerMask groundLayer;           // Asigna aquí la capa del suelo (puedes poner 'Everything' por ahora)

    private float verticalVelocity;
    private bool isGrounded;
    private bool isFalling;

    [Header("Referencias")]
    private Animator animator;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        controller = GetComponent<CharacterController>();
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
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Saltamos si el Raycast dice que estamos en el suelo
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        ChequearSueloConRaycast();
        ManejarMovimientoYGravedad();
        ActualizarAnimaciones();
    }

    private void ChequearSueloConRaycast()
    {
        // Lanzamos un rayo desde el centro del personaje hacia abajo
        // El origen es un poco arriba de los pies para evitar errores
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // Raycast: Origen, Dirección, Información del impacto, Distancia máxima
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        // Debug visual para ver el rayo en la escena (Rojo = Aire, Verde = Suelo)
        Debug.DrawRay(origin, Vector3.down * (groundCheckDistance + 0.1f), isGrounded ? Color.green : Color.red);

        // Estamos cayendo si no tocamos suelo y vamos hacia abajo
        isFalling = !isGrounded && verticalVelocity < -0.5f;
    }

    private void ManejarMovimientoYGravedad()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        Transform camTransform = Camera.main.transform;
        Vector3 forward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(camTransform.right, new Vector3(1, 0, 1)).normalized;
        Vector3 inputDir = (forward * moveInput.y + right * moveInput.x).normalized;

        // Rotación
        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Lógica de Gravedad mejorada
        if (isGrounded && verticalVelocity < 0)
        {
            // Pequeña fuerza negativa para mantener al jugador pegado al suelo en rampas
            verticalVelocity = -2f;
        }
        else
        {
            // Aplicar gravedad en el aire
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Aplicar el movimiento total al Character Controller
        Vector3 horizontalMove = inputDir * speed;
        Vector3 finalMove = horizontalMove + (Vector3.up * verticalVelocity);

        controller.Move(finalMove * Time.deltaTime);
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        bool estaMoviendose = moveInput.magnitude > 0.1f;

        animator.SetBool("Idle", !estaMoviendose && isGrounded);
        animator.SetBool("Jog", estaMoviendose && isGrounded);

        // El salto se activa cuando subimos
        animator.SetBool("Jump", !isGrounded && verticalVelocity > 0);

        // El falling se activa gracias a nuestro Raycast e isFalling
        animator.SetBool("Fall", isFalling);
    }

    void OnEnable() => inputActions?.Player.Enable();
    void OnDisable() => inputActions?.Player.Disable();
}