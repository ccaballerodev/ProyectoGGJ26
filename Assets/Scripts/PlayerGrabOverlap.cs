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

    [Header("Animator & Layers")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string grabParameterName = "Grab";
    [SerializeField] private int layerWhileHeld = 2; // Default 2 es "Ignore Raycast"

    [Header("Settings")]
    [SerializeField] private Vector3 holdOffset = new Vector3(0, 0.2f, 0); // Ejemplo: sube 0.5 unidades

    [Header("Behavior")]
    [SerializeField] private bool logDebug = true;

    private InputAction grabAction;
    private GameObject heldInstance;
    private Rigidbody heldRb;
    private int originalLayer;

    public bool HasHeldObject => heldInstance != null;
    public Rigidbody HeldRigidbody => heldRb;

    private void Awake()
    {
        if (holdPoint == null)
            Debug.LogError("[GrabSpawnPrefab] Asigna holdPoint en el Inspector.");

        // Configuración del input manual
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
        else DropObject();
    }

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupMask, QueryTriggerInteraction.Ignore);

        if (logDebug)
            Debug.Log($"[GrabSpawnPrefab] Hits encontrados: {hits.Length}");

        GameObject bestObject = null;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            // Buscamos el Rigidbody en el objeto o sus padres
            Rigidbody targetRb = hits[i].attachedRigidbody;
            if (targetRb == null) continue;

            Transform targetT = targetRb.transform;

            // Filtro para agarrar solo lo que está frente al jugador
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
                bestObject = targetRb.gameObject;
            }
        }

        if (bestObject != null)
        {
            PickupObject(bestObject);
        }
    }

    private void PickupObject(GameObject obj)
    {
        heldInstance = obj;
        heldRb = obj.GetComponent<Rigidbody>();
        originalLayer = obj.layer;

        // 1. Emparentar al punto de la mano
        heldInstance.transform.SetParent(holdPoint);
        heldInstance.transform.localPosition = holdOffset;
        heldInstance.transform.localRotation = Quaternion.identity;

        // 2. Desactivar física para que no pese mientras lo llevas
        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
        }

        // 3. Cambiar Layer y activar bool en el Animator
        heldInstance.layer = layerWhileHeld;
        if (characterAnimator != null) characterAnimator.SetBool(grabParameterName, true);

        if (logDebug) Debug.Log($"[GrabSpawnPrefab] Agarrado: {obj.name}");
    }

    public void DropObject()
    {
        if (!HasHeldObject) return;

        if (logDebug) Debug.Log($"[GrabSpawnPrefab] Soltando '{heldInstance.name}'");

        // 1. Liberar el objeto
        heldInstance.transform.SetParent(null);

        // 2. Devolver física para que caiga al suelo
        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;

            // Un pequeño impulso para que no se quede pegado al jugador
            heldRb.AddForce(transform.forward * 1.5f, ForceMode.Impulse);
        }

        // 3. Restaurar Layer original y apagar el bool del Animator
        heldInstance.layer = originalLayer;
        if (characterAnimator != null) characterAnimator.SetBool(grabParameterName, false);

        heldInstance = null;
        heldRb = null;
    }

    // Útil para habilidades de lanzamiento externas
    public Rigidbody ReleaseForThrow(bool forceGravity = true)
    {
        if (!HasHeldObject) return null;

        Rigidbody rb = heldRb;
        DropObject(); // Usamos la lógica de soltar para limpiar todo

        if (rb != null) rb.useGravity = forceGravity;
        return rb;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
#endif
}