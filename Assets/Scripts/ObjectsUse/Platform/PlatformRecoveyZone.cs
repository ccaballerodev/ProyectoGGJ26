using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlatformRecoveryZone : MonoBehaviour, IAbilityUsable
{
    [Header("Referencias")]
    [SerializeField] private FallingPlatform platform;
    [SerializeField] private FallingPlatformTrigger fallTriggerToReset;

    [Header("Control")]
    [SerializeField] private bool recoverOnEnter = false;

    private bool playerInsideZone = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (platform == null)
            Debug.LogError("[PlatformRecoveryZone] Falta referencia a 'platform'.");

        if (fallTriggerToReset == null)
            Debug.LogWarning("[PlatformRecoveryZone] No asignaste 'fallTriggerToReset' (opcional pero recomendado).");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInsideZone = true;

        if (recoverOnEnter)
            UseAbility();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInsideZone = false;
    }

    // ? Implementación de interfaz: lo llama tu habilidad o el tester genérico
    public void UseAbility()
    {
        if (!playerInsideZone) return;   // solo funciona dentro de la zona
        if (platform == null) return;
        if (!platform.IsFallen) return;  // opcional

        platform.ReturnToStart();

        if (fallTriggerToReset != null)
            fallTriggerToReset.ResetTriggerState();

        Debug.Log("[PlatformRecoveryZone] Plataforma recuperada correctamente.");
    }
}