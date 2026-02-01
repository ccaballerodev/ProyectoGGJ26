using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowHeldPrefabHoldToActivate : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private PlayerGrabOverlapSpawnPrefab grabber;

    [Header("Input")]
    [SerializeField] private Key throwKey = Key.Q;

    [Header("Hold-to-activate")]
    [Tooltip("Tiempo que debes mantener presionado para que se lance.")]
    [SerializeField] private float requiredHoldTime = 0.75f;

    [Tooltip("Usar tiempo sin escala (no afecta Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Throw Force (fijo)")]
    [SerializeField] private float throwForce = 14f;
    [SerializeField] private float upwardForce = 2.5f;
    [SerializeField] private float torque = 6f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private InputAction throwAction;

    private bool holding = false;
    private bool executed = false;
    private float holdStartTime = 0f;

    private float Now() => useUnscaledTime ? Time.unscaledTime : Time.time;

    private void Awake()
    {
        if (grabber == null)
            grabber = GetComponent<PlayerGrabOverlapSpawnPrefab>();

        throwAction = new InputAction("ThrowHold", binding: $"<Keyboard>/{throwKey}");
    }

    private void OnEnable()
    {
        throwAction.Enable();
        throwAction.started += OnStarted;
        throwAction.canceled += OnCanceled;
    }

    private void OnDisable()
    {
        throwAction.started -= OnStarted;
        throwAction.canceled -= OnCanceled;
        throwAction.Disable();
    }

    private void Update()
    {
        if (!holding || executed) return;

        float held = Now() - holdStartTime;
        if (held >= requiredHoldTime)
        {
            TryThrow(held);
        }
    }

    private void OnStarted(InputAction.CallbackContext ctx)
    {
        // Solo empezar a cargar si hay algo en mano
        if (grabber == null || !grabber.HasHeldObject)
        {
            if (logDebug) Debug.Log("[ThrowHold] No hay objeto en mano, no inicio hold.");
            holding = false;
            return;
        }

        holding = true;
        executed = false;
        holdStartTime = Now();

        if (logDebug) Debug.Log("[ThrowHold] Hold START");
    }

    private void OnCanceled(InputAction.CallbackContext ctx)
    {
        if (!holding) return;

        float held = Now() - holdStartTime;

        // Si suelta antes del tiempo requerido: no lanza
        if (!executed && held < requiredHoldTime)
        {
            if (logDebug) Debug.Log($"[ThrowHold] Cancelado (held={held:0.00}s < {requiredHoldTime:0.00}s). NO lanza.");
        }

        holding = false;
        executed = false;
    }

    private void TryThrow(float heldTime)
    {
        // Evitar doble ejecución si Update lo llama varias veces
        executed = true;
        holding = false;

        if (grabber == null || !grabber.HasHeldObject)
        {
            if (logDebug) Debug.Log("[ThrowHold] Se completó hold pero ya no hay objeto.");
            return;
        }

        Rigidbody rb = grabber.ReleaseForThrow(forceGravity: true);
        if (rb == null)
        {
            if (logDebug) Debug.Log("[ThrowHold] ReleaseForThrow devolvió null.");
            return;
        }

        Vector3 dir = transform.forward;
        Vector3 impulse = dir * throwForce + Vector3.up * upwardForce;

        rb.AddForce(impulse, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.VelocityChange);

        if (logDebug)
            Debug.Log($"[ThrowHold] THROW ejecutado (held={heldTime:0.00}s >= {requiredHoldTime:0.00}s). force={throwForce}");
    }

    // Si quieres mostrar progreso en UI
    public float GetHoldProgress01()
    {
        if (!holding) return 0f;
        float held = Now() - holdStartTime;
        return Mathf.Clamp01(held / requiredHoldTime);
    }
}