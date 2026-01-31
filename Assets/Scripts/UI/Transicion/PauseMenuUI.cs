using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel de Pausa")]
    public CanvasGroup menuCanvasGroup;
    public CanvasGroup backgroundCanvasGroup;
    public float animationDuration = 0.5f;

    [Header("Jugador")]
    public MonoBehaviour playerController;

    private bool isPaused = false;

    private void Update()
    {
        // Capturar ESC directamente
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    private void OpenMenu()
    {
        isPaused = true;

        // Desactivar controles del jugador
        if (playerController != null)
            playerController.enabled = false;

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activar panel y fondo
        menuCanvasGroup.gameObject.SetActive(true);
        backgroundCanvasGroup.gameObject.SetActive(true);

        // Fondo oscuro fade-in
        backgroundCanvasGroup.alpha = 0;
        backgroundCanvasGroup.DOFade(0.9f, animationDuration).SetEase(Ease.InOutSine);

        // Menú principal fade + scale
        menuCanvasGroup.alpha = 0;
        menuCanvasGroup.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        menuCanvasGroup.DOFade(1f, animationDuration).SetEase(Ease.InOutSine);
        menuCanvasGroup.transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
    }

    public void CloseMenu()
    {
        isPaused = false;

        // Reactivar controles del jugador
        if (playerController != null)
            playerController.enabled = true;

        // Ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Fondo oscuro fade-out
        backgroundCanvasGroup.DOFade(0, animationDuration).SetEase(Ease.InOutSine)
            .OnComplete(() => backgroundCanvasGroup.gameObject.SetActive(false));

        // Menú principal fade + scale-out
        menuCanvasGroup.DOFade(0, animationDuration).SetEase(Ease.InOutSine);
        menuCanvasGroup.transform.DOScale(0.8f, animationDuration).SetEase(Ease.InBack)
            .OnComplete(() => menuCanvasGroup.gameObject.SetActive(false));
    }
}