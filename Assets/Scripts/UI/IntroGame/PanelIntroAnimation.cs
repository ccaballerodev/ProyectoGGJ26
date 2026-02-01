using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class PanelIntroAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image targetImage;
    [SerializeField] private AudioSource introSFX;

    [Header("Animación")]
    [SerializeField] private float animationDelay = 0.4f;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float initialScaleMultiplier = 0.6f;

    [Header("Evento al terminar animación")]
    public UnityEvent OnAnimationComplete;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = targetImage.rectTransform.localScale;
    }

    private void OnEnable()
    {
        PlayAudio();
        PrepareInitialState();
        Invoke(nameof(PlayAnimation), animationDelay);
    }

    private void PlayAudio()
    {
        if (introSFX != null)
            introSFX.Play();
    }

    private void PrepareInitialState()
    {
        var c = targetImage.color;
        targetImage.color = new Color(c.r, c.g, c.b, 0f);
        targetImage.rectTransform.localScale = originalScale * initialScaleMultiplier;
    }

    private void PlayAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(targetImage.DOFade(1f, duration).SetEase(Ease.OutQuad));
        seq.Join(targetImage.rectTransform
            .DOScale(originalScale, duration)
            .SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            OnAnimationComplete?.Invoke();
        });
    }

    private void OnDisable()
    {
        CancelInvoke();
        DOTween.Kill(targetImage);
        DOTween.Kill(targetImage.rectTransform);
    }
}