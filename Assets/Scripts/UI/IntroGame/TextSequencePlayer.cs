using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TextSequencePlayer : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private TextSequenceSO sequence;

    [Header("Referencias")]
    [SerializeField] private TypewriterEffect typewriter;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private float holdAfterWrite = 0.8f;
    [SerializeField] private float holdAfterErase = 0.3f;

    [Header("Eventos")]
    public UnityEvent OnSequenceFinished;

    private int index = 0;
    private bool sequenceRunning = false;

    private void OnEnable()
    {
        index = 0;
        sequenceRunning = false;
        StartCoroutine(WaitForTypewriterAndStart());
    }

    private IEnumerator WaitForTypewriterAndStart()
    {
        // Espera hasta que el typewriter esté activo en la jerarquía
        while (typewriter == null || !typewriter.gameObject.activeInHierarchy)
        {
            yield return null; // espera un frame
        }

        // Espera startDelay adicional si quieres
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        StartSequence();
    }

    private void StartSequence()
    {
        if (!sequenceRunning)
        {
            sequenceRunning = true;
            PlayLine();
        }
    }

    private void PlayLine()
    {
        if (index >= sequence.lines.Count)
        {
            FinishSequence();
            return;
        }

        typewriter.PlayLine(sequence.lines[index], () =>
        {
            Invoke(nameof(EraseLine), holdAfterWrite);
        });
    }

    private void EraseLine()
    {
        typewriter.ReverseLine(() =>
        {
            Invoke(nameof(Advance), holdAfterErase);
        });
    }

    private void Advance()
    {
        index++;
        PlayLine();
    }

    private void FinishSequence()
    {
        sequenceRunning = false;
        OnSequenceFinished?.Invoke();
    }
}