using UnityEngine;
using UnityEngine.Events;

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

    private void Start()
    {
        Invoke(nameof(PlayLine), startDelay);
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
        OnSequenceFinished?.Invoke();
    }
}