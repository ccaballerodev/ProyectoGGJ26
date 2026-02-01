using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPushReceiver : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float damping = 12f;
    [SerializeField] private float maxPushSpeed = 5.5f;

    private CharacterController cc;
    private Vector3 externalVelocity;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (externalVelocity.sqrMagnitude > 0.0001f)
        {
            cc.Move(externalVelocity * Time.deltaTime);
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, damping * Time.deltaTime);
        }
    }

    // ? Esto lo llaman los cubos
    public void AddPush(Vector3 pushVelocity)
    {
        // clamp para que no se vuelva loco
        if (pushVelocity.magnitude > maxPushSpeed)
            pushVelocity = pushVelocity.normalized * maxPushSpeed;

        externalVelocity += pushVelocity;
    }
}