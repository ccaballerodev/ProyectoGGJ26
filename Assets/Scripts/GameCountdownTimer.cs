using UnityEngine;
using TMPro;
using System;

public class GameCountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Duración")]
    [SerializeField] private float startSeconds = 300f;

    [Header("Modo de tiempo")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Referencias")]
    [SerializeField] private FanLifeController fan; // ? asigna en Inspector

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    public float RemainingSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    // ? EVENTOS
    public event Action OnTimeExpired;         // tiempo llegó a 0  -> perder
    public event Action OnObjectiveCompleted;  // ventilador dejó de generar -> ganar

    private bool finished = false; // evita doble fin (race condition)

    private void Start()
    {
        StartTimer(); // arranca con startSeconds
    }

    private void Awake()
    {
        RemainingSeconds = Mathf.Max(0f, startSeconds);
        UpdateUI();
    }

    private void OnEnable()
    {
        if (fan != null)
            fan.OnStoppedGenerating += HandleFanStoppedGenerating;
    }

    private void OnDisable()
    {
        if (fan != null)
            fan.OnStoppedGenerating -= HandleFanStoppedGenerating;
    }

    private void Update()
    {
        if (!IsRunning || finished) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        int highCount = ToxicGasHighRegistry.HighCount;
        float multiplier = 1f + highCount; // 0->1, 1->2, 2->3, 3->4, 4->5
        RemainingSeconds -= dt * multiplier;

        if (RemainingSeconds <= 0f)
        {
            RemainingSeconds = 0f;
            IsRunning = false;
            finished = true;

            UpdateUI();

            if (logDebug) Debug.Log("[GameCountdownTimer] Tiempo agotado -> PERDER");
            OnTimeExpired?.Invoke();
            return;
        }

        UpdateUI();
    }

    public void StartTimer(float seconds = -1f)
    {
        if (seconds > 0f) RemainingSeconds = seconds;
        else RemainingSeconds = Mathf.Max(0f, startSeconds);

        finished = false;
        IsRunning = true;
        UpdateUI();

        if (logDebug) Debug.Log($"[GameCountdownTimer] START {RemainingSeconds:0.00}s");
    }

    public void StopTimer()
    {
        IsRunning = false;
        if (logDebug) Debug.Log("[GameCountdownTimer] STOP");
    }

    private void HandleFanStoppedGenerating()
    {
        if (finished) return; // si ya perdiste por tiempo, ignora

        finished = true;
        IsRunning = false;

        if (logDebug) Debug.Log("[GameCountdownTimer] Ventilador dejó de generar -> GANAR");
        OnObjectiveCompleted?.Invoke();
    }

    private void UpdateUI()
    {
        if (timeText == null) return;

        int total = Mathf.CeilToInt(RemainingSeconds);
        int minutes = total / 60;
        int secs = total % 60;
        timeText.text = $"{minutes:00}:{secs:00}";
    }
}