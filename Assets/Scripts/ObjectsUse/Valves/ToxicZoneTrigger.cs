using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToxicZoneTrigger : MonoBehaviour, IAbilityUsable
{
    [Header("Target que ejecuta la acción")]
    [SerializeField] private ToxicGasStateController gasController;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool playerInside = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (gasController == null)
            Debug.LogError("[AbilityZoneTrigger] Falta referencia a gasController.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInside = true;
        if (logDebug) Debug.Log("[AbilityZoneTrigger] Player ENTER zone.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInside = false;
        if (logDebug) Debug.Log("[AbilityZoneTrigger] Player EXIT zone.");
    }

    /// <summary>
    /// Se llama desde tu habilidad o el tester genérico.
    /// Solo actúa si el Player está dentro.
    /// </summary>
    public void UseAbility()
    {
        if (!playerInside)
        {
            if (logDebug) Debug.Log("[AbilityZoneTrigger] UseAbility ignorado: Player no está en zona.");
            return;
        }

        if (gasController == null) return;

        // Acción: High -> Low
        bool ok = gasController.TryHighToLow();

        if (logDebug)
            Debug.Log(ok
                ? "[AbilityZoneTrigger] UseAbility OK: gas High->Low."
                : "[AbilityZoneTrigger] UseAbility NO hizo nada (gas no estaba en High).");
    }
}