using DG.Tweening;
using UnityEngine;

public class UIPanel_Main : UIPanelBase
{
    protected override void Awake()
    {
        base.Awake();
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public override Tween Show()
    {
        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(1f, 0.4f).From(0.95f).SetEase(Ease.InOutSine));
        seq.Join(canvasGroup.DOFade(1f, 0.4f));

        return seq;
    }

    public override Tween Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(0.95f, 0.3f).SetEase(Ease.InOutSine));
        seq.Join(canvasGroup.DOFade(0f, 0.3f));
        seq.OnComplete(() => gameObject.SetActive(false));

        return seq;
    }
}