using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TimerManager
{
    private class TimerBehaviour : MonoBehaviour { }

    private static TimerBehaviour instance;
    private static Dictionary<string, TimerData> activeTimers = new Dictionary<string, TimerData>();

    private class TimerData
    {
        public Coroutine coroutine;
        public float originalDuration;
        public float remainingTime;
        public System.Action onComplete;
        public bool isPaused;
        public float startTime;
        public float endTime;
    }

    private static void Initialize()
    {
        if (instance == null)
        {
            GameObject gameObject = new GameObject("AdvancedTimerManager");
            instance = gameObject.AddComponent<TimerBehaviour>();
            Object.DontDestroyOnLoad(gameObject);
        }
    }

    public static void StartTimer(string timerId, float duration, System.Action onComplete)
    {
        Initialize();

        if (activeTimers.ContainsKey(timerId))
        {
            instance.StopCoroutine(activeTimers[timerId].coroutine);
            activeTimers.Remove(timerId);
        }

        TimerData timerData = new TimerData
        {
            originalDuration = duration,
            remainingTime = duration,
            onComplete = onComplete,
            isPaused = false,
            startTime = Time.time,
            endTime = Time.time + duration
        };

        timerData.coroutine = instance.StartCoroutine(TimerCoroutine(timerId, timerData));
        activeTimers[timerId] = timerData;
    }

    public static void ReplaceTimerDuration(string timerId, float newDuration)
    {
        if (activeTimers.ContainsKey(timerId))
        {
            TimerData timerData = activeTimers[timerId];

            instance.StopCoroutine(timerData.coroutine);

            timerData.originalDuration = newDuration;
            timerData.remainingTime = newDuration;
            timerData.startTime = Time.time;
            timerData.endTime = Time.time + newDuration;

            timerData.coroutine = instance.StartCoroutine(TimerCoroutine(timerId, timerData));

            Debug.Log($"Timer '{timerId}' REEMPLAZADO: nueva duración {newDuration} segundos");
        }
        else
        {
            Debug.LogWarning($"Timer con ID '{timerId}' no encontrado para reemplazar duración");
        }
    }

    public static void AddTimeToTimer(string timerId, float extraTime)
    {
        if (activeTimers.ContainsKey(timerId))
        {
            TimerData timerData = activeTimers[timerId];

            instance.StopCoroutine(timerData.coroutine);

            timerData.originalDuration += extraTime;
            timerData.remainingTime += extraTime;
            timerData.endTime += extraTime;

            timerData.coroutine = instance.StartCoroutine(TimerCoroutine(timerId, timerData));

            Debug.Log($"Timer '{timerId}' EXTENDIDO: +{extraTime} segundos agregados");
        }
        else
        {
            Debug.LogWarning($"Timer con ID '{timerId}' no encontrado para agregar tiempo");
        }
    }

    public static float GetRemainingTime(string timerId)
    {
        if (timerId == null)
            return 0f;

        if (activeTimers.ContainsKey(timerId) && !activeTimers[timerId].isPaused)
        {
            return activeTimers[timerId].remainingTime;
        }
        return 0f;
    }

    public static float GetProgress(string timerId)
    {
        if (activeTimers.ContainsKey(timerId) && !activeTimers[timerId].isPaused)
        {
            TimerData timerData = activeTimers[timerId];
            float elapsed = Time.time - timerData.startTime;
            return Mathf.Clamp01(elapsed / timerData.originalDuration);
        }
        return 0f;
    }
    public static void PauseTimer(string timerId)
    {
        if (activeTimers.ContainsKey(timerId) && !activeTimers[timerId].isPaused)
        {
            TimerData timerData = activeTimers[timerId];
            instance.StopCoroutine(timerData.coroutine);
            timerData.isPaused = true;

            timerData.remainingTime = timerData.endTime - Time.time;

            Debug.Log($"Timer '{timerId}' PAUSADO");
        }
    }
    public static void ResumeTimer(string timerId)
    {
        if (activeTimers.ContainsKey(timerId) && activeTimers[timerId].isPaused)
        {
            TimerData timerData = activeTimers[timerId];
            timerData.isPaused = false;

            timerData.startTime = Time.time;
            timerData.endTime = Time.time + timerData.remainingTime;

            timerData.coroutine = instance.StartCoroutine(TimerCoroutine(timerId, timerData));

            Debug.Log($"Timer '{timerId}' REANUDADO");
        }
    }

    public static void StopTimer(string timerId)
    {
        if (activeTimers.ContainsKey(timerId))
        {
            instance.StopCoroutine(activeTimers[timerId].coroutine);
            activeTimers.Remove(timerId);
            Debug.Log($"Timer '{timerId}' DETENIDO");
        }
    }

    public static bool IsTimerRunning(string timerId)
    {
        return activeTimers.ContainsKey(timerId) && !activeTimers[timerId].isPaused;
    }
    public static bool IsTimerPaused(string timerId)
    {
        return activeTimers.ContainsKey(timerId) && activeTimers[timerId].isPaused;
    }

    public static bool TimerExists(string timerId)
    {
        return activeTimers.ContainsKey(timerId);
    }

    public static List<string> GetAllActiveTimers()
    {
        return new List<string>(activeTimers.Keys);
    }

    public static void StopAllTimers()
    {
        foreach (var timer in activeTimers.Values)
        {
            instance.StopCoroutine(timer.coroutine);
        }
        activeTimers.Clear();
        Debug.Log("Todos los timers detenidos");
    }

    public static float GetOriginalDuration(string timerId)
    {
        if (activeTimers.ContainsKey(timerId))
        {
            return activeTimers[timerId].originalDuration;
        }
        else
        {
            Debug.LogWarning($"Timer con ID '{timerId}' no encontrado para obtener duración original");
            return 0f;
        }
    }


    private static IEnumerator TimerCoroutine(string timerId, TimerData timerData)
    {
        timerData.startTime = Time.time;
        timerData.endTime = Time.time + timerData.remainingTime;

        while (Time.time < timerData.endTime)
        {
            timerData.remainingTime = timerData.endTime - Time.time;
            yield return null;
        }

        if (activeTimers.ContainsKey(timerId))
        {
            activeTimers.Remove(timerId);
            timerData.onComplete?.Invoke();
        }
    }
}