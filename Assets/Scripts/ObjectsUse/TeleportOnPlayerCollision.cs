using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class TeleportOnPlayerTrigger : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform teleportTarget;

    [Header("Opciones")]
    [SerializeField] private bool useTargetRotation = true;
    [SerializeField] private bool resetRigidbodyVelocity = true;

    [Header("Debug")]
    [SerializeField] private bool logEverything = true;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ? Recomendado para triggers estables con CharacterController
        rb.isKinematic = true;
        rb.useGravity = false;

        var col = GetComponent<Collider>();
        col.isTrigger = true; // ? ahora es trigger

        if (teleportTarget == null)
            Debug.LogError("[TP] Falta asignar teleportTarget.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;

        if (logEverything)
            Debug.Log($"[TP] OnTriggerEnter con '{other.name}' (root='{root.name}', tag='{root.tag}')");

        if (!root.CompareTag("Player")) return;
        if (teleportTarget == null) return;

        Debug.Log($"[TP] Player detectado -> TP a '{teleportTarget.name}'");
        StartCoroutine(TeleportRoutine(root));
    }

    private IEnumerator TeleportRoutine(Transform playerRoot)
    {
        CharacterController cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Rigidbody prb = playerRoot.GetComponent<Rigidbody>();
        if (prb != null && resetRigidbodyVelocity)
        {
#if UNITY_6000_0_OR_NEWER
            prb.linearVelocity = Vector3.zero;
#else
            prb.velocity = Vector3.zero;
#endif
            prb.angularVelocity = Vector3.zero;
        }

        playerRoot.position = teleportTarget.position;
        if (useTargetRotation) playerRoot.rotation = teleportTarget.rotation;

        yield return null;
        if (cc != null) cc.enabled = true;
    }
}