using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class TypewriterEffectAuto : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private AudioSource typeSFX;

    [Header("Velocidad")]
    [Tooltip("Tiempo por carácter")]
    [SerializeField] private float charDuration = 0.05f;

    [Tooltip("Cuántas veces más rápido se borra")]
    [SerializeField] private float reverseSpeedMultiplier = 3f;

    private Tween typingTween;
    private string fullText;
    private int lastCharIndex;

    public bool IsPlaying => typingTween != null && typingTween.IsActive();

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        // Guardamos el texto que ya estaba en el TMP_Text
        fullText = targetText.text;
    }

    /// <summary>
    /// Reproduce la animación de escritura del texto ya presente en el TMP_Text
    /// </summary>
    public void Play()
    {
        targetText.text = "";  // empezamos desde vacío
        lastCharIndex = 0;

        typingTween?.Kill();

        int charCount = fullText.Length;

        typingTween = DOTween.To(() => 0, val =>
        {
            int length = Mathf.Clamp(val, 0, charCount);
            targetText.text = fullText.Substring(0, length);

            // Sonido por cada carácter
            if (typeSFX != null && length > lastCharIndex)
            {
                typeSFX.PlayOneShot(typeSFX.clip);
                lastCharIndex = length;
            }
        }, charCount, charCount * charDuration)
        .SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(() => typingTween = null);
    }

    /// <summary>
    /// Reproduce la animación de borrado del texto
    /// </summary>
    public void Reverse(Action onComplete = null)
    {
        typingTween?.Kill();

        int startLength = targetText.text.Length;

        typingTween = DOTween.To(() => startLength, val =>
        {
            int length = Mathf.Clamp(val, 0, startLength);
            targetText.text = fullText.Substring(0, length);
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