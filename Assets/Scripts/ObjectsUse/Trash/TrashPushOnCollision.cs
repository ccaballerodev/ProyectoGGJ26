using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrashPushOnCollision : MonoBehaviour
{
    [Header("Empuje")]
    [SerializeField] private float pushMultiplier = 0.55f; // para velocidad ~10
    [SerializeField] private bool ignoreVertical = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision) => TryPush(collision);
    private void OnCollisionStay(Collision collision) => TryPush(collision);

    private void TryPush(Collision collision)
    {
        Transform root = collision.transform.root;
        if (!root.CompareTag("Player")) return;

        var receiver = root.GetComponent<PlayerPushReceiver>();
        if (receiver == null) return;

        // 1) Tomar un contacto (o promedio si hay varios)
        ContactPoint cp = collision.GetContact(0);

        // ? Dirección "desde el golpe hacia el jugador" (varía según dónde pegue)
        Vector3 dir = (root.position - cp.point);
        if (ignoreVertical) dir.y = 0f;

        // Fallback si queda muy pequeño
        if (dir.sqrMagnitude < 0.0001f)
        {
            // La normal apunta "desde el Player hacia el cubo" (normal del contacto)
            // Para empujar al Player, usamos la opuesta:
            dir = -cp.normal;
            if (ignoreVertical) dir.y = 0f;
        }

        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // 2) Magnitud del empuje basada en la velocidad real del cubo
#if UNITY_6000_0_OR_NEWER
        float speed = rb.linearVelocity.magnitude;
#else
        float speed = rb.velocity.magnitude;
#endif
        if (speed < 0.05f) return;

        Vector3 push = dir * (speed * pushMultiplier);
        receiver.AddPush(push);

        if (logDebug)
            Debug.Log($"[TrashPush] hitPoint={cp.point} dir={dir} speed={speed:0.00} push={push.magnitude:0.00}");
    }
}