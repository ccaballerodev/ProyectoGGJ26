using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private AudioSource typeSFX;

    [Header("Velocidad fija")]
    [Tooltip("Tiempo por carácter")]
    [SerializeField] private float charDuration = 0.05f;

    [Tooltip("Cuántas veces más rápido se borra")]
    [SerializeField] private float reverseSpeedMultiplier = 3f;

    private Tween typingTween;
    private string currentLine;
    private int lastCharIndex;

    public bool IsPlaying => typingTween != null && typingTween.IsActive();

    public void PlayLine(string line, Action onComplete = null)
    {
        currentLine = line;
        targetText.text = "";
        lastCharIndex = 0;

        typingTween?.Kill();

        int charCount = currentLine.Length;

        typingTween = DOTween.To(() => 0, val =>
        {
            int length = Mathf.Clamp(val, 0, charCount);
            targetText.text = currentLine.Substring(0, length);

            if (typeSFX != null && length > lastCharIndex)
            {
                typeSFX.PlayOneShot(typeSFX.clip);
                lastCharIndex = length;
            }
        }, charCount, charCount * charDuration)
        .SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            typingTween = null;
            onComplete?.Invoke();
        });
    }

    public void ReverseLine(Action onComplete = null)
    {
        typingTween?.Kill();

        int startLength = targetText.text.Length;

        typingTween = DOTween.To(() => startLength, val =>
        {
            int length = Mathf.Clamp(val, 0, startLength);
            targetText.text = currentLine.Substring(0, length);
        }, 0, (startLength * charDuration) / reverseSpeedMultiplier)
        .SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            typingTween = null;
            onComplete?.Invoke();
        });
    }
}