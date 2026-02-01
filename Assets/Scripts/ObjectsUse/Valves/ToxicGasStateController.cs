using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ToxicGasStateController : MonoBehaviour
{
    public enum GasState { Low, High }

    [Header("Referencia")]
    [SerializeField] private ParticleSystem ps;

    [Header("Inicio")]
    [SerializeField] private GasState startState = GasState.High;

    [Header("LOW -> HIGH timer random")]
    [SerializeField] private float minSecondsToHigh = 5f;
    [SerializeField] private float maxSecondsToHigh = 9f;

    [Header("Aplicación visual")]
    [SerializeField] private bool restartOnSwitch = false; // true = cambio brusco, false = transición natural
    [SerializeField] private bool logDebug = true;

    [Header("LOW preset")]
    [SerializeField] private float rateLow = 15f;
    [SerializeField] private float lifetimeLow = 2f;
    [Range(0, 255)][SerializeField] private int alphaLow255 = 30;
    [SerializeField] private int maxParticlesLow = 150;

    [Header("HIGH preset")]
    [SerializeField] private float rateHigh = 40f;
    [SerializeField] private float lifetimeHigh = 5f;
    [Range(0, 255)][SerializeField] private int alphaHigh255 = 70;
    [SerializeField] private int maxParticlesHigh = 200;

    public GasState State { get; private set; }

    private float timer = 0f;
    private float targetTimeToHigh = 0f;

    private void Awake()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        InitState();
    }

    private void Update()
    {
        // Solo contar tiempo cuando está en LOW
        if (State != GasState.Low) return;

        timer += Time.deltaTime;
        if (timer >= targetTimeToHigh)
        {
            SetHigh();
            if (logDebug) Debug.Log("[ToxicGasStateController] LOW -> HIGH (timer).");
        }
    }

    private void InitState()
    {
        State = startState;

        if (State == GasState.High)
        {
            Apply(rateHigh, lifetimeHigh, alphaHigh255, maxParticlesHigh, restart: true);
            if (logDebug) Debug.Log("[ToxicGasStateController] Start HIGH.");
        }
        else
        {
            Apply(rateLow, lifetimeLow, alphaLow255, maxParticlesLow, restart: true);
            RollNextHighTime();
            timer = 0f;
            if (logDebug) Debug.Log($"[ToxicGasStateController] Start LOW. Next HIGH in {targetTimeToHigh:0.00}s");
        }
    }

    /// <summary>
    /// Método público: si está en HIGH, baja a LOW y programa volver a HIGH con random 5-9.
    /// </summary>
    public bool TryHighToLow()
    {
        if (State != GasState.High)
        {
            if (logDebug) Debug.Log("[ToxicGasStateController] TryHighToLow: ya está LOW (o no HIGH).");
            return false;
        }

        // HIGH -> LOW
        State = GasState.Low;
        Apply(rateLow, lifetimeLow, alphaLow255, maxParticlesLow, restartOnSwitch);

        // Reiniciar timer random para volver a HIGH
        RollNextHighTime();
        timer = 0f;

        if (logDebug) Debug.Log($"[ToxicGasStateController] HIGH -> LOW. Next HIGH in {targetTimeToHigh:0.00}s");
        return true;
    }

    public void SetHigh()
    {
        State = GasState.High;
        Apply(rateHigh, lifetimeHigh, alphaHigh255, maxParticlesHigh, restartOnSwitch);
    }

    private void RollNextHighTime()
    {
        float a = Mathf.Min(minSecondsToHigh, maxSecondsToHigh);
        float b = Mathf.Max(minSecondsToHigh, maxSecondsToHigh);
        targetTimeToHigh = Random.Range(a, b);
    }

    private void Apply(float rate, float lifetime, int alpha255, int maxParticles, bool restart)
    {
        var emission = ps.emission;
        emission.rateOverTime = rate;

        var main = ps.main;
        main.startLifetime = lifetime;
        main.maxParticles = maxParticles;

        Color c = main.startColor.color;
        c.a = alpha255 / 255f;
        main.startColor = c;

        if (restart)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}