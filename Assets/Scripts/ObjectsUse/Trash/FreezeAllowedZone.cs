using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FreezeAllowedZone : MonoBehaviour, IAbilityUsable
{
    [Header("Mask requerida")]
    [SerializeField] private MaskType requiredMask = MaskType.Mascara1;

    [Header("Pulse Prefab")]
    [SerializeField] private FreezePulse pulsePrefab;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseRadius = 6f;
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private LayerMask trashMask = ~0;

    [Header("Spawn")]
    [SerializeField] private bool spawnAtPlayer = true;
    [SerializeField] private Transform pulseSpawnPoint;

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 3f;
    [SerializeField] private bool useUnscaledCooldown = false;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool playerInside = false;
    private Transform playerRoot;
    private PlayerMaskState playerMask;

    private float nextReadyTime = 0f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (pulsePrefab == null)
            Debug.LogError("[FreezeAllowedZone] Falta asignar pulsePrefab.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside = true;
        playerRoot = other.transform.root;
        playerMask = playerRoot.GetComponent<PlayerMaskState>();

        if (logDebug) Debug.Log($"[FreezeAllowedZone] Player ENTER zone. root='{playerRoot.name}'");
        if (playerMask == null && logDebug) Debug.LogWarning("[FreezeAllowedZone] Player no tiene PlayerMaskState.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside = false;
        playerRoot = null;
        playerMask = null;

        if (logDebug) Debug.Log("[FreezeAllowedZone] Player EXIT zone.");
    }

    public float GetRemainingCooldown()
    {
        float now = useUnscaledCooldown ? Time.unscaledTime : Time.time;
        return Mathf.Max(0f, nextReadyTime - now);
    }

    public void UseAbility()
    {
        float now = useUnscaledCooldown ? Time.unscaledTime : Time.time;

        if (!playerInside)
        {
            if (logDebug) Debug.Log("[FreezeAllowedZone] Ignorado: fuera de zona.");
            return;
        }

        if (playerMask == null || playerMask.currentMask != requiredMask)
        {
            if (logDebug) Debug.Log($"[FreezeAllowedZone] Ignorado por máscara. Necesita {requiredMask}, tiene {(playerMask ? playerMask.currentMask : MaskType.Ninguna)}");
            return;
        }

        if (now < nextReadyTime)
        {
            if (logDebug) Debug.Log($"[FreezeAllowedZone] Cooldown: {GetRemainingCooldown():0.00}s");
            return;
        }

        if (pulsePrefab == null) return;

        Vector3 spawnPos;
        if (!spawnAtPlayer && pulseSpawnPoint != null) spawnPos = pulseSpawnPoint.position;
        else if (playerRoot != null) spawnPos = playerRoot.position;
        else spawnPos = transform.position;

        var pulse = Instantiate(pulsePrefab, spawnPos, Quaternion.identity);
        pulse.Init(pulseRadius, freezeDuration, trashMask);

        nextReadyTime = now + cooldownSeconds;

        if (logDebug) Debug.Log("[FreezeAllowedZone] OK: FreezePulse instanciado.");
    }
}