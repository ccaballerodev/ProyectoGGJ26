using UnityEngine;
using UnityEngine.InputSystem;
using System.Globalization;

public class AbilityTesterPC_Generic : MonoBehaviour
{
    [Header("PC Key (New Input System)")]
    [SerializeField] private Key key = Key.E;

    [Header("Tap vs Hold (misma tecla)")]
    [SerializeField] private float tapMaxDuration = 0.20f;
    [SerializeField] private float holdRequiredTime = 0.75f;

    [Header("Broadcast (Tap): UseAbility()")]
    [SerializeField] private bool includeInactive = false;

    [Header("Hold receivers (opcional)")]
    [Tooltip("Si lo dejas vacío, hará broadcast buscando receptores en escena.")]
    [SerializeField] private MonoBehaviour[] holdReceivers;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private InputAction tapAction;
    private InputAction holdAction;

    private void Awake()
    {
        string keyPath = $"<Keyboard>/{key.ToString().ToLower()}";

        // ? Formato Invariant (siempre usa punto decimal)
        string tapDur = tapMaxDuration.ToString(CultureInfo.InvariantCulture);
        string holdDur = holdRequiredTime.ToString(CultureInfo.InvariantCulture);

        tapAction = new InputAction(
            name: "TestAllAbilities_Tap",
            type: InputActionType.Button,
            binding: keyPath,
            interactions: $"Tap(duration={tapDur})"
        );

        holdAction = new InputAction(
            name: "AbilityHold",
            type: InputActionType.Button,
            binding: keyPath,
            interactions: $"Hold(duration={holdDur})"
        );
    }

    private void OnEnable()
    {
        tapAction.performed += OnTapPerformed;
        tapAction.Enable();

        holdAction.performed += OnHoldPerformed;
        holdAction.canceled += OnHoldCanceled;
        holdAction.Enable();
    }

    private void OnDisable()
    {
        tapAction.performed -= OnTapPerformed;
        tapAction.Disable();

        holdAction.performed -= OnHoldPerformed;
        holdAction.canceled -= OnHoldCanceled;
        holdAction.Disable();
    }

    private void OnDestroy()
    {
        tapAction?.Dispose();
        holdAction?.Dispose();
    }

    private void OnTapPerformed(InputAction.CallbackContext ctx)
    {
        int count = 0;

        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (!includeInactive && !b.isActiveAndEnabled) continue;

            if (b is IAbilityUsable usable)
            {
                count++;
                if (logDebug)
                    Debug.Log($"[AbilityTester TAP] UseAbility() -> {b.gameObject.name} ({b.GetType().Name})");
                usable.UseAbility();
            }
        }

        if (logDebug) Debug.Log($"[AbilityTester TAP] Total UseAbility ejecutados: {count}");
    }

    private void OnHoldCanceled(InputAction.CallbackContext ctx)
    {
        if (logDebug)
            Debug.Log("[AbilityTester HOLD] CANCELED (soltó antes) -> NO envía mensaje");
    }

    private void OnHoldPerformed(InputAction.CallbackContext ctx)
    {
        if (logDebug)
            Debug.Log($"[AbilityTester HOLD] PERFORMED -> enviando mensaje a receptores");

        if (holdReceivers != null && holdReceivers.Length > 0)
        {
            int sent = 0;
            foreach (var r in holdReceivers)
            {
                if (r == null) continue;
                if (r is IAbilityHoldReceiver receiver)
                {
                    sent++;
                    if (logDebug) Debug.Log($"[AbilityTester HOLD] -> {r.gameObject.name} ({r.GetType().Name})");
                    receiver.OnAbilityHold();
                }
            }
            if (logDebug) Debug.Log($"[AbilityTester HOLD] Mensajes enviados: {sent}");
            return;
        }

        // fallback: broadcast
        int count = 0;
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (!includeInactive && !b.isActiveAndEnabled) continue;

            if (b is IAbilityHoldReceiver receiver)
            {
                count++;
                if (logDebug) Debug.Log($"[AbilityTester HOLD] -> {b.gameObject.name} ({b.GetType().Name})");
                receiver.OnAbilityHold();
            }
        }

        if (logDebug) Debug.Log($"[AbilityTester HOLD] Total receptores notificados: {count}");
    }
}