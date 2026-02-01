using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePulse : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float radius = 6f;
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private LayerMask trashMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool drawGizmo = true;

    private readonly List<TrashFreezable> captured = new List<TrashFreezable>();

    public void Init(float newRadius, float newDuration, LayerMask newTrashMask)
    {
        radius = newRadius;
        freezeDuration = newDuration;
        trashMask = newTrashMask;

        if (logDebug)
            Debug.Log($"[FreezePulse] Init -> radius={radius}, duration={freezeDuration}, maskValue={trashMask.value}");
    }

    private void Start()
    {
        if (logDebug)
            Debug.Log($"[FreezePulse] Start en pos={transform.position}");

        CaptureAndFreeze();
        StartCoroutine(UnfreezeAfter());
    }

    private void CaptureAndFreeze()
    {
        captured.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            trashMask,
            QueryTriggerInteraction.Ignore
        );

        if (logDebug)
            Debug.Log($"[FreezePulse] OverlapSphere pos={transform.position} r={radius} hits={hits.Length} mask={trashMask.value}");

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];

            if (logDebug)
                Debug.Log($"[FreezePulse] Hit[{i}] name='{h.name}' layer={h.gameObject.layer} root='{h.transform.root.name}'");

            var t = h.GetComponentInParent<TrashFreezable>();

            if (t == null)
            {
                if (logDebug)
                    Debug.Log($"[FreezePulse] -> NO TrashFreezable en parent de '{h.name}'");
                continue;
            }

            if (!captured.Contains(t))
            {
                captured.Add(t);

                if (logDebug)
                    Debug.Log($"[FreezePulse] -> FREEZE '{t.gameObject.name}'");

                t.Freeze();
            }
        }

        if (logDebug)
            Debug.Log($"[FreezePulse] Capturados={captured.Count}");
    }

    private IEnumerator UnfreezeAfter()
    {
        if (logDebug)
            Debug.Log($"[FreezePulse] Esperando {freezeDuration:0.00}s para descongelar...");

        yield return new WaitForSeconds(freezeDuration);

        if (logDebug)
            Debug.Log($"[FreezePulse] Descongelando {captured.Count} objetos (Drop)...");

        for (int i = 0; i < captured.Count; i++)
        {
            if (captured[i] != null)
                captured[i].UnfreezeDrop();
        }

        if (logDebug)
            Debug.Log("[FreezePulse] Fin. Destruyendo pulso.");

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}