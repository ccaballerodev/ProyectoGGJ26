using UnityEngine;

public class ToxicGasHighReporter : MonoBehaviour
{
    [SerializeField] private ToxicGasStateController gas;
    [SerializeField] private bool logDebug = false;

    private void Awake()
    {
        if (gas == null) gas = GetComponent<ToxicGasStateController>();
        if (gas == null)
            Debug.LogError("[ToxicGasHighReporter] Falta ToxicGasStateController en el mismo objeto.");
    }

    private void OnEnable()
    {
        if (gas == null) return;

        gas.OnStateChanged += OnGasStateChanged;

        // registrar estado inicial
        ToxicGasHighRegistry.Register(gas);

        if (logDebug)
            Debug.Log($"[ToxicGasHighReporter] Enable -> IsHigh={gas.IsHigh} | HighCount={ToxicGasHighRegistry.HighCount}");
    }

    private void OnDisable()
    {
        if (gas == null) return;

        gas.OnStateChanged -= OnGasStateChanged;
        ToxicGasHighRegistry.Unregister(gas);

        if (logDebug)
            Debug.Log($"[ToxicGasHighReporter] Disable -> HighCount={ToxicGasHighRegistry.HighCount}");
    }

    private void OnGasStateChanged(ToxicGasStateController controller, ToxicGasStateController.GasState state)
    {
        ToxicGasHighRegistry.Register(controller);

        if (logDebug)
            Debug.Log($"[ToxicGasHighReporter] State={state} | HighCount={ToxicGasHighRegistry.HighCount}");
    }
}