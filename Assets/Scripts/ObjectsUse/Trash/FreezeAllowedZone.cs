using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FreezeAllowedZone : MonoBehaviour, IAbilityUsable
{
    [Header("Pulse Prefab")]
    [SerializeField] private FreezePulse pulsePrefab;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseRadius = 6f;
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private LayerMask trashMask = ~0;

    [Header("Spawn")]
    [SerializeField] private bool spawnAtPlayer = true;
    [SerializeField] private Transform pulseSpawnPoint; // opcional

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 3f;
    [SerializeField] private bool useUnscaledCooldown = false;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool playerInside = false;
    private Transform playerRoot;

    private float nextReadyTime = 0f;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (pulsePrefab == null)
            Debug.LogError("[FreezeAllowedZone] Falta asignar pulsePrefab.");

        if (logDebug)
            Debug.Log($"[FreezeAllowedZone] Awake. cooldown={cooldownSeconds}, spawnAtPlayer={spawnAtPlayer}, maskValue={trashMask.value}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside = true;
        playerRoot = other.transform.root;

        if (logDebug)
            Debug.Log($"[FreezeAllowedZone] Player ENTER zone. root='{playerRoot.name}'");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside = false;
        playerRoot = null;

        if (logDebug)
            Debug.Log("[FreezeAllowedZone] Player EXIT zone.");
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
            if (logDebug) Debug.Log("[FreezeAllowedZone] UseAbility ignorado: fuera de la zona permitida.");
            return;
        }

        if (now < nextReadyTime)
        {
            if (logDebug)
                Debug.Log($"[FreezeAllowedZone] En cooldown. Falta: {GetRemainingCooldown():0.00}s");
            return;
        }

        if (pulsePrefab == null)
        {
            Debug.LogError("[FreezeAllowedZone] No hay pulsePrefab asignado.");
            return;
        }

        Vector3 spawnPos;

        if (!spawnAtPlayer && pulseSpawnPoint != null)
            spawnPos = pulseSpawnPoint.position;
        else if (playerRoot != null)
            spawnPos = playerRoot.position;
        else
            spawnPos = transform.position;

        if (logDebug)
            Debug.Log($"[FreezeAllowedZone] UseAbility OK. Instanciando pulso en {spawnPos} | radius={pulseRadius} dur={freezeDuration} maskValue={trashMask.value}");

        var pulse = Instantiate(pulsePrefab, spawnPos, Quaternion.identity);
        pulse.Init(pulseRadius, freezeDuration, trashMask);

        nextReadyTime = now + cooldownSeconds;

        if (logDebug)
            Debug.Log($"[FreezeAllowedZone] Cooldown iniciado: {cooldownSeconds:0.00}s");
    }
}