using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToxicZoneTrigger : MonoBehaviour, IAbilityUsable
{
    [Header("Target que ejecuta la acción")]
    [SerializeField] private ToxicGasStateController gasController;

    [Header("Mask requerida")]
    [SerializeField] private MaskType requiredMask = MaskType.Mascara3;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool playerInside = false;
    private PlayerMaskState playerMask;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (gasController == null)
            Debug.LogError("[ToxicZoneTrigger] Falta referencia a gasController.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside = true;
        playerMask = other.transform.root.GetComponent<PlayerMaskState>();

        if (logDebug) Debug.Log("[ToxicZoneTrigger] Player ENTER zone.");
        if (playerMask == null && logDebug) Debug.LogWarning("[ToxicZoneTrigger] Player no tiene PlayerMaskState.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInside = false;
        playerMask = null;
        if (logDebug) Debug.Log("[ToxicZoneTrigger] Player EXIT zone.");
    }

    public void UseAbility()
    {
        if (!playerInside)
        {
            if (logDebug) Debug.Log("[ToxicZoneTrigger] UseAbility ignorado: Player no está en zona.");
            return;
        }

        if (playerMask == null || playerMask.currentMask != requiredMask)
        {
            if (logDebug) Debug.Log($"[ToxicZoneTrigger] Ignorado por máscara. Necesita {requiredMask}, tiene {(playerMask ? playerMask.currentMask : MaskType.Ninguna)}");
            return;
        }

        if (gasController == null) return;

        bool ok = gasController.TryHighToLow();

        if (logDebug)
            Debug.Log(ok
                ? "[ToxicZoneTrigger] OK: gas High->Low."
                : "[ToxicZoneTrigger] NO hizo nada (gas no estaba en High).");
    }
}