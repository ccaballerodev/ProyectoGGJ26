using UnityEngine;

public class ThrowHeldPrefabAbility : MonoBehaviour, IAbilityHoldReceiver
{
    [Header("Referencias")]
    [SerializeField] private PlayerGrabOverlapSpawnPrefab grabber;
    [SerializeField] private PlayerMaskState maskState;

    [Header("Requisito")]
    [SerializeField] private MaskType requiredMask = MaskType.Mascara4;

    [Header("Throw Force")]
    [SerializeField] private float throwForce = 14f;
    [SerializeField] private float upwardForce = 2.5f;
    [SerializeField] private float torque = 6f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void Awake()
    {
        if (grabber == null) grabber = GetComponent<PlayerGrabOverlapSpawnPrefab>();
        if (maskState == null) maskState = GetComponent<PlayerMaskState>();
    }

    // ? Este método lo llama el AbilityTester cuando el HOLD se completa
    public void OnAbilityHold()
    {
        if (maskState == null || maskState.currentMask != requiredMask)
        {
            if (logDebug)
                Debug.Log($"[ThrowReceiver] Ignorado por máscara. Necesita {requiredMask}, tiene {(maskState ? maskState.currentMask : MaskType.Ninguna)}");
            return;
        }

        if (grabber == null || !grabber.HasHeldObject)
        {
            if (logDebug) Debug.Log("[ThrowReceiver] No hay objeto en mano para lanzar.");
            return;
        }

        Rigidbody rb = grabber.ReleaseForThrow(forceGravity: true);
        if (rb == null)
        {
            if (logDebug) Debug.Log("[ThrowReceiver] ReleaseForThrow devolvió null.");
            return;
        }

        Vector3 impulse = transform.forward * throwForce + Vector3.up * upwardForce;
        rb.AddForce(impulse, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.VelocityChange);

        if (logDebug) Debug.Log("[ThrowReceiver] THROW ejecutado por HOLD.");
    }
}