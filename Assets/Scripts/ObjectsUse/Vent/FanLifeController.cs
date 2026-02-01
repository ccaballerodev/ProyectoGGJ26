using UnityEngine;
using System;
using System.Collections.Generic;

public class FanLifeController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FanCreator fanCreator;
    [SerializeField] private Transform fanVisual;

    [Header("Visual")]
    [SerializeField] private float spinSpeed = 360f;

    [Header("Vida (lista de weak points)")]
    [SerializeField] private List<FanLifeNode> weakPoints = new List<FanLifeNode>();

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    public event Action OnStoppedGenerating;

    private bool stopped = false;

    private void Awake()
    {
        if (fanCreator == null) fanCreator = GetComponentInChildren<FanCreator>();

        // Auto-registrar weakpoints hijos si no llenaste la lista
        if (weakPoints.Count == 0)
            weakPoints.AddRange(GetComponentsInChildren<FanLifeNode>(true));

        // Enlazar owner
        foreach (var wp in weakPoints)
            if (wp != null) wp.SetOwner(this);

        if (logDebug)
            Debug.Log($"[FanController] WeakPoints iniciales: {weakPoints.Count}");
    }

    private void Update()
    {
        if (!stopped && fanVisual != null)
            fanVisual.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
    }

    // ? Lo llama un weakpoint cuando se destruye (contador llega a 0)
    public void NotifyWeakPointDestroyed(FanLifeNode wp)
    {
        if (stopped) return;

        weakPoints.Remove(wp);

        if (logDebug)
            Debug.Log($"[FanController] WeakPoint removido. Restantes: {weakPoints.Count}");

        // ? Si ya no quedan -> parar de generar INMEDIATO
        if (weakPoints.Count <= 0)
        {
            StopFanImmediate();
            EndGame();
        }
    }

    private void StopFanImmediate()
    {
        if (stopped) return;
        stopped = true;

        if (fanCreator != null)
            fanCreator.SetActiveSpawning(false);

        if (logDebug)
            Debug.Log("[FanController] STOP IMMEDIATE: ya no genera basura.");

        OnStoppedGenerating?.Invoke();
    }

    private void EndGame()
    {
        // Aquí conectas tu lógica real de victoria
        Debug.Log("[FanController] END GAME (victoria).");
    }
}