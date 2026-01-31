using UnityEngine;
using DG.Tweening;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private GameObject nextPanel;

    [Header("Configuración de Fade")]
    [Tooltip("Duración del fade de salida y entrada")]
    [SerializeField] private float fadeDuration = 0.5f;

    /// <summary>
    /// Cambia de panel con fade si tienen CanvasGroup
    /// </summary>
    public void SwitchPanel()
    {
        // Fade OUT del panel actual
        if (currentPanel != null)
        {
            CanvasGroup currentCG = currentPanel.GetComponent<CanvasGroup>();
            if (currentCG != null)
            {
                currentCG.DOFade(0, fadeDuration).SetEase(Ease.InOutSine)
                    .OnComplete(() => currentPanel.SetActive(false));
            }
            else
            {
                currentPanel.SetActive(false);
            }
        }

        // Activar y fade IN del siguiente panel
        if (nextPanel != null)
        {
            CanvasGroup nextCG = nextPanel.GetComponent<CanvasGroup>();
            if (nextCG != null)
            {
                nextPanel.SetActive(true);
                nextCG.alpha = 0;
                nextCG.DOFade(1, fadeDuration).SetEase(Ease.InOutSine);
            }
            else
            {
                nextPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Permite cambiar la duración del fade desde código
    /// </summary>
    /// <param name="duration">Nueva duración en segundos</param>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }
}