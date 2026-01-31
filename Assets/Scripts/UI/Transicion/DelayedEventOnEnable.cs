using UnityEngine;
using UnityEngine.Events;

public class DelayedEventOnEnable : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo en segundos después de activar este GameObject para disparar el evento")]
    [SerializeField] private float delay = 5f;

    [Header("Evento a ejecutar")]
    [SerializeField] private UnityEvent onDelayedEvent;

    private void OnEnable()
    {
        // Inicia la corrutina al activarse el GameObject
        StartCoroutine(TriggerEventAfterDelay());
    }

    private System.Collections.IEnumerator TriggerEventAfterDelay()
    {
        yield return new WaitForSeconds(delay); // espera X segundos
        onDelayedEvent?.Invoke();               // dispara el evento
    }
}