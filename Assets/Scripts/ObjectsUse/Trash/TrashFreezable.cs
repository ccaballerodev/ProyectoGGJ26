using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrashFreezable : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    [Header("Drop behavior")]
    [Tooltip("Si está activo, al terminar el congelado se fuerza useGravity = true para que caiga.")]
    [SerializeField] private bool forceGravityOnDrop = true;

    private Rigidbody rb;

    // Para evitar conflictos si se aplican varios congelados
    private int freezeCount = 0;

    // Estados previos
    private bool savedKinematic;
    private bool savedUseGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Freeze()
    {
        freezeCount++;
        if (logDebug) Debug.Log($"[TrashFreezable] Freeze '{name}' count={freezeCount}");

        if (freezeCount > 1) return; // ya estaba congelado

        // Guardar estados actuales
        savedKinematic = rb.isKinematic;
        savedUseGravity = rb.useGravity;

        // Congelar completamente
        rb.isKinematic = true;
        rb.useGravity = false; // mientras está congelado, no tiene sentido gravedad

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Descongela en modo "Drop": cae al suelo.
    /// </summary>
    public void UnfreezeDrop()
    {
        if (freezeCount <= 0) return;

        freezeCount--;
        if (logDebug) Debug.Log($"[TrashFreezable] UnfreezeDrop '{name}' count={freezeCount}");

        if (freezeCount > 0) return; // otro congelado sigue activo

        // Al soltar, queremos que vuelva a simular física
        rb.isKinematic = false;

        // Si quieres que SIEMPRE caiga después del congelado:
        if (forceGravityOnDrop)
            rb.useGravity = true;
        else
            rb.useGravity = savedUseGravity;

        // Sin reanudar el empuje, que solo caiga
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;
    }
}