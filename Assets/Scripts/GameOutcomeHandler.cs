using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOutcomeHandler : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private GameCountdownTimer timer;

    [Header("Win")]
    [Tooltip("Nombre EXACTO de la escena de victoria (debe estar en Build Settings).")]
    [SerializeField] private string winSceneName = "WinScene";

    [Header("Opciones")]
    [SerializeField] private bool resetTimeScaleOnLoad = true;

    private bool ended = false;

    private void Awake()
    {
        if (timer == null)
            timer = FindFirstObjectByType<GameCountdownTimer>();
    }

    private void OnEnable()
    {
        if (timer == null) return;

        timer.OnObjectiveCompleted += OnWin;
        timer.OnTimeExpired += OnLose;
    }

    private void OnDisable()
    {
        if (timer == null) return;

        timer.OnObjectiveCompleted -= OnWin;
        timer.OnTimeExpired -= OnLose;
    }

    private void OnWin()
    {
        if (ended) return;
        ended = true;

        Debug.Log($"[GAME] WIN -> Cargando escena '{winSceneName}'");

        if (resetTimeScaleOnLoad) Time.timeScale = 1f;
        SceneManager.LoadScene(winSceneName);
    }

    private void OnLose()
    {
        if (ended) return;
        ended = true;

        Debug.Log("[GAME] LOSE -> Recargando escena actual");

        if (resetTimeScaleOnLoad) Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}