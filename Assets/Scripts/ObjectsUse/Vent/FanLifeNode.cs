using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanLifeNode : MonoBehaviour
{
    [Header("Vida de este punto")]
    [SerializeField] private int hitsRemaining = 3;

    [Header("Filtro")]
    [SerializeField] private LayerMask trashLayer;  // asigna Trash
    [SerializeField] private bool destroyTrashOnHit = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private FanLifeController owner;
    private bool dead = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (trashLayer.value == 0)
            Debug.LogWarning($"[FanWeakPoint:{name}] trashLayer vacío. Asigna la Layer Trash.");
    }

    public void SetOwner(FanLifeController fan) => owner = fan;

    private void OnTriggerEnter(Collider other)
    {
        if (dead) return;

        // ? filtrar por layer Trash
        if (((1 << other.gameObject.layer) & trashLayer.value) == 0) return;

        // Para evitar que un mismo objeto reste muchas veces por entrar/quedarse,
        // normalmente lo destruyes al impactar:
        if (destroyTrashOnHit)
            Destroy(other.transform.root.gameObject);

        hitsRemaining--;

        if (logDebug)
            Debug.Log($"[FanWeakPoint:{name}] HIT por Trash. hitsRemaining={hitsRemaining}");

        if (hitsRemaining <= 0)
        {
            dead = true;

            if (logDebug)
                Debug.Log($"[FanWeakPoint:{name}] DESTROYED -> notifica al ventilador");

            owner?.NotifyWeakPointDestroyed(this);

            // Opcional: apagar visual del weakpoint
            gameObject.SetActive(false);
        }
    }
}