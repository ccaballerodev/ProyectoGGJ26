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
    public float gravity = -18f;

    [Header("Detección de Suelo (Raycast)")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Configuración Idle Especial (AFK)")]
    public float tiempoParaAburrirse = 20f;
    private bool estaAburrido = false;
    private string myTimerID;

    // --- VARIABLES DE RED ---
    // Sincronizan los datos necesarios para las animaciones sin usar NetworkAnimator
    private NetworkVariable<float> netMoveMagnitude = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netIsGrounded = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netIsFalling = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netEstaAburrido = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netIsJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
        // Generamos el ID siempre, pero el input solo para el dueño
        myTimerID = "TimerAburrido_" + OwnerClientId;

        if (IsOwner)
        {
            inputActions.Player.Enable();
            inputActions.Player.Jump.performed += OnJumpPerformed;
        }

        RecargarAnimator();
    }

    public void RecargarAnimator()
    {
        // Buscamos el animator en los hijos (el modelo activo lo tendrá)
        animator = GetComponentInChildren<Animator>(false);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            ResetearTimerAburrimiento();
            // Trigger manual de salto para red
            if (IsOwner) StartCoroutine(JumpTriggerRoutine());
        }
    }

    // Pequeña rutina para que el estado de salto se vea en la red un momento
    private System.Collections.IEnumerator JumpTriggerRoutine()
    {
        netIsJumping.Value = true;
        yield return new WaitForSeconds(0.1f);
        netIsJumping.Value = false;
    }

    void Update()
    {
        if (IsOwner)
        {
            ChequearSueloConRaycast();
            ManejarMovimientoYGravedad();
            GestionarTimerAburrimiento();

            // ACTUALIZAR VARIABLES DE RED
            netMoveMagnitude.Value = moveInput.magnitude;
            netIsGrounded.Value = isGrounded;
            netIsFalling.Value = isFalling;
            netEstaAburrido.Value = estaAburrido;
        }

        // ESTO LO EJECUTAN TODOS (Dueño y los demás)
        ActualizarAnimaciones();
    }

    private void ChequearSueloConRaycast()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);
        isFalling = !isGrounded && verticalVelocity < -0.5f;
    }

    private void ManejarMovimientoYGravedad()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        Transform camTransform = Camera.main.transform;
        Vector3 forward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(camTransform.right, new Vector3(1, 0, 1)).normalized;
        Vector3 inputDir = (forward * moveInput.y + right * moveInput.x).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = (inputDir * speed) + (Vector3.up * verticalVelocity);
        controller.Move(finalMove * Time.deltaTime);
    }

    private void GestionarTimerAburrimiento()
    {
        bool estaRealmenteQuieto = moveInput.magnitude < 0.1f && isGrounded && !isFalling;
        if (estaRealmenteQuieto)
        {
            if (!TimerManager.IsTimerRunning(myTimerID) && !estaAburrido)
            {
                TimerManager.StartTimer(myTimerID, tiempoParaAburrirse, () => { estaAburrido = true; });
            }
        }
        else ResetearTimerAburrimiento();
    }

    private void ResetearTimerAburrimiento()
    {
        if (TimerManager.TimerExists(myTimerID)) TimerManager.StopTimer(myTimerID);
        estaAburrido = false;
    }

    private void ActualizarAnimaciones()
    {
        // Si el modelo cambió o no se ha cargado, intentamos buscarlo
        if (animator == null || !animator.gameObject.activeInHierarchy)
        {
            RecargarAnimator();
            if (animator == null) return;
        }

        float mag = netMoveMagnitude.Value;
        bool grounded = netIsGrounded.Value;
        bool aburrido = netEstaAburrido.Value;

        // Usamos las variables de red para que todos vean lo mismo
        animator.SetBool("Idle", mag <= 0.1f && grounded && !aburrido);
        animator.SetBool("Jog", mag > 0.1f && grounded);
        animator.SetBool("Jump", netIsJumping.Value);
        animator.SetBool("Fall", netIsFalling.Value);
        animator.SetBool("IdleQuitMask", aburrido);
    }

    void OnEnable() => inputActions?.Player.Enable();
    void OnDisable() { inputActions?.Player.Disable(); if (myTimerID != null) TimerManager.StopTimer(myTimerID); }
}