using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class PanelFadeOutActivateNext : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 0.5f;   // Duración del fade
    [SerializeField] private GameObject panelToActivate;  // Panel que se activará al terminar

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Llama desde un botón: hace fade out del panel actual y activa otro panel
    /// </summary>
    public void FadeAndActivateNext()
    {
        if (canvasGroup == null) return;

        canvasGroup.DOKill(); // Cancela cualquier animación en curso

        canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // Desactiva este panel
                gameObject.SetActive(false);

                // Activa el siguiente panel si está asignado
                if (panelToActivate != null)
                    panelToActivate.SetActive(true);
            });
    }
}