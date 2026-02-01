using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneFade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.6f;

    private bool isFading;

    private void Start()
    {
        // Solo afecta ESTA escena
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        fadeImage.color = Color.black;

        // Fade IN al iniciar la escena
        canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
            });
    }

    public void LoadScene(string sceneName)
    {
        if (isFading) return;
        isFading = true;

        Sequence seq = DOTween.Sequence();

        // Bloquea input
        seq.AppendCallback(() =>
        {
            canvasGroup.blocksRaycasts = true;
        });

        // Fade OUT
        seq.Append(
            canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
        );

        // Carga escena
        seq.AppendCallback(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    // Nuevo método para cerrar el juego
    public void QuitGame()
    {
        Debug.Log("Cerrando juego...");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Detener juego en el editor
#else
        Application.Quit(); // Cierra la aplicación en compilaciones
#endif
    }
}