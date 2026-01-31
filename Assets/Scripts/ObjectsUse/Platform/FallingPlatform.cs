using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : MonoBehaviour
{
    [Header("Opcional (recomendado)")]
    [SerializeField] private Collider solidCollider;          // collider NO trigger donde se paran
    [SerializeField] private bool disableColliderWhileReturn = true;

    [Header("Return settings")]
    [SerializeField] private float returnSpeed = 5f;          // unidades/segundo
    [SerializeField] private float returnAngularSpeed = 360f;  // grados/segundo
    [SerializeField] private float arriveDistance = 0.01f;     // tolerancia de llegada

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    private Coroutine returnRoutine;

    public bool IsFallen { get; private set; }
    public bool IsReturning { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        rb.useGravity = true;
        rb.isKinematic = true; // bloqueada al inicio
        IsFallen = false;
    }

    public void Fall()
    {
        if (IsReturning && returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            IsReturning = false;
        }

        rb.isKinematic = false;
        IsFallen = true;
    }

    /// <summary>
    /// Regresa suavemente a la posición inicial y deja la física bloqueada al final.
    /// </summary>
    public void ReturnToStart()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        IsReturning = true;

        // Bloqueamos la física para controlar el movimiento
        rb.isKinematic = true;

        // Limpiar velocidades (aunque sea kinematic, lo dejamos “quieto”)
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;

        bool colliderWasEnabled = false;
        if (disableColliderWhileReturn && solidCollider != null)
        {
            colliderWasEnabled = solidCollider.enabled;
            solidCollider.enabled = false;
        }

        // Mover en pasos de física
        while (Vector3.Distance(rb.position, startPos) > arriveDistance ||
               Quaternion.Angle(rb.rotation, startRot) > 0.5f)
        {
            yield return new WaitForFixedUpdate();

            Vector3 newPos = Vector3.MoveTowards(rb.position, startPos, returnSpeed * Time.fixedDeltaTime);
            Quaternion newRot = Quaternion.RotateTowards(rb.rotation, startRot, returnAngularSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);
            rb.MoveRotation(newRot);
        }

        // Asegurar alineación final exacta
        rb.position = startPos;
        rb.rotation = startRot;

        if (disableColliderWhileReturn && solidCollider != null)
            solidCollider.enabled = colliderWasEnabled;

        rb.Sleep();

        IsFallen = false;
        IsReturning = false;
        returnRoutine = null;
    }
}