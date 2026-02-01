using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlatformRecoveryZone : MonoBehaviour, IAbilityUsable
{
    [Header("Mask requerida")]
    [SerializeField] private MaskType requiredMask = MaskType.Mascara2;

    [Header("Referencias")]
    [SerializeField] private FallingPlatform platform;
    [SerializeField] private FallingPlatformTrigger fallTriggerToReset;

    [Header("Control")]
    [SerializeField] private bool recoverOnEnter = false;

    private bool playerInsideZone = false;
    private PlayerMaskState playerMask;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (platform == null)
            Debug.LogError("[PlatformRecoveryZone] Falta referencia a 'platform'.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInsideZone = true;
        playerMask = other.transform.root.GetComponent<PlayerMaskState>();

        if (recoverOnEnter)
            UseAbility();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInsideZone = false;
        playerMask = null;
    }

    public void UseAbility()
    {
        if (!playerInsideZone) return;

        if (playerMask == null || playerMask.currentMask != requiredMask)
            return;

        if (platform == null) return;
        if (!platform.IsFallen) return;

        platform.ReturnToStart();

        if (fallTriggerToReset != null)
            fallTriggerToReset.ResetTriggerState();

        Debug.Log("[PlatformRecoveryZone] Plataforma recuperada correctamente.");
    }
}