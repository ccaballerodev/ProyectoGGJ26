using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FallingPlatformTrigger : MonoBehaviour
{
    [SerializeField] private FallingPlatform platform;
    [SerializeField] private float fallDelay = 0f;

    private int playerInside = 0;
    private bool hasFallen = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (platform == null)
            platform = GetComponentInParent<FallingPlatform>();

        if (platform == null)
            Debug.LogError("[FallingPlatformTrigger] No encontré FallingPlatform en el padre.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInside++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        playerInside--;

        if (!hasFallen && playerInside <= 0)
        {
            hasFallen = true;

            if (fallDelay > 0f) Invoke(nameof(DoFall), fallDelay);
            else DoFall();
        }
    }

    private void DoFall()
    {
        platform.Fall();
    }

    public void ResetTriggerState()
    {
        playerInside = 0;
        hasFallen = false;
    }
}