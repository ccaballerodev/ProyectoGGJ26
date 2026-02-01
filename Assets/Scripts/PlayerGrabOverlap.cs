using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabOverlapSpawnPrefab : MonoBehaviour
{
    [Header("Hold Point")]
    [SerializeField] private Transform holdPoint;

    [Header("Pickup Detection")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private LayerMask pickupMask = ~0;
    [Range(0f, 1f)]
    [SerializeField] private float forwardDotThreshold = 0.2f;

    [Header("Input (New Input System)")]
    [SerializeField] private Key grabKey = Key.F;

    [Header("Behavior")]
    [SerializeField] private bool destroyWorldObjectOnPickup = true;
    [SerializeField] private bool logDebug = true;

    private InputAction grabAction;

    private GameObject heldInstance;
    private Rigidbody heldRb;

    public bool HasHeldObject => heldInstance != null;
    public Rigidbody HeldRigidbody => heldRb;

    private void Awake()
    {
        if (holdPoint == null)
            Debug.LogError("[GrabSpawnPrefab] Asigna holdPoint en el Inspector.");

        grabAction = new InputAction("Grab", binding: $"<Keyboard>/{grabKey}");
    }

    private void OnEnable()
    {
        grabAction.Enable();
        grabAction.performed += OnGrabPerformed;
    }

    private void OnDisable()
    {
        grabAction.performed -= OnGrabPerformed;
        grabAction.Disable();
    }

    private void OnGrabPerformed(InputAction.CallbackContext ctx)
    {
        if (!HasHeldObject) TryPickup();
        else DropAndDestroy(); // drop “normal”: si quieres, puede soltarlo en vez de destruirlo
    }

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupMask, QueryTriggerInteraction.Ignore);

        if (logDebug)
            Debug.Log($"[GrabSpawnPrefab] Overlap hits={hits.Length} radius={pickupRadius} mask={pickupMask.value}");

        SpawnedFromPrefab bestMarker = null;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var marker = hits[i].GetComponentInParent<SpawnedFromPrefab>();
            if (marker == null || marker.sourcePrefab == null) continue;

            Transform targetT = marker.transform;

            // filtro “al frente”
            if (forwardDotThreshold > 0f)
            {
                Vector3 to = targetT.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.0001f)
                {
                    float dot = Vector3.Dot(transform.forward, to.normalized);
                    if (dot < forwardDotThreshold) continue;
                }
            }

            float dist = (targetT.position - transform.position).sqrMagnitude;
            if (dist < bestScore)
            {
                bestScore = dist;
                bestMarker = marker;
            }
        }

        if (bestMarker == null)
        {
            if (logDebug) Debug.Log("[GrabSpawnPrefab] No encontré objetos con SpawnedFromPrefab en rango.");
            return;
        }

        // ? Instanciar el prefab en la mano
        SpawnInHand(bestMarker.sourcePrefab);

        // (opcional) destruir el del mundo
        if (destroyWorldObjectOnPickup)
        {
            if (logDebug) Debug.Log($"[GrabSpawnPrefab] Destruyendo world object '{bestMarker.gameObject.name}'");
            Destroy(bestMarker.gameObject);
        }
    }

    private void SpawnInHand(GameObject prefab)
    {
        heldInstance = Instantiate(prefab, holdPoint.position, holdPoint.rotation);
        heldInstance.transform.SetParent(holdPoint, worldPositionStays: true);
        heldInstance.transform.localPosition = Vector3.zero;
        heldInstance.transform.localRotation = Quaternion.identity;

        heldRb = heldInstance.GetComponent<Rigidbody>();
        if (heldRb != null)
        {
            // mientras lo sostienes, lo haces estable
            heldRb.isKinematic = true;
            heldRb.useGravity = false;

#if UNITY_6000_0_OR_NEWER
            heldRb.linearVelocity = Vector3.zero;
#else
            heldRb.velocity = Vector3.zero;
#endif
            heldRb.angularVelocity = Vector3.zero;
        }

        if (logDebug) Debug.Log($"[GrabSpawnPrefab] Instanciado en mano: '{heldInstance.name}' desde prefab '{prefab.name}'");
    }

    // Drop normal (si no te interesa dejarlo tirado, lo destruyes)
    public void DropAndDestroy()
    {
        if (!HasHeldObject) return;

        if (logDebug) Debug.Log($"[GrabSpawnPrefab] DropAndDestroy '{heldInstance.name}'");

        Destroy(heldInstance);
        heldInstance = null;
        heldRb = null;
    }

    /// <summary>
    /// Esto lo usará tu habilidad única para lanzar.
    /// Suelta el objeto de la mano y lo deja listo para física.
    /// </summary>
    public Rigidbody ReleaseForThrow(bool forceGravity = true)
    {
        if (!HasHeldObject) return null;

        heldInstance.transform.SetParent(null, worldPositionStays: true);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = forceGravity;

#if UNITY_6000_0_OR_NEWER
            heldRb.linearVelocity = Vector3.zero;
#else
            heldRb.velocity = Vector3.zero;
#endif
            heldRb.angularVelocity = Vector3.zero;
        }

        Rigidbody rb = heldRb;

        heldInstance = null;
        heldRb = null;

        return rb;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
#endif
}