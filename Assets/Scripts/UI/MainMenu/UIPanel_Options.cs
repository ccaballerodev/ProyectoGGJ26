using DG.Tweening;
using UnityEngine;

public class UIPanel_Options : UIPanelBase
{
    public override Tween Show()
    {
        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        transform.localScale = new Vector3(1, 0, 1);
        canvasGroup.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScaleY(1, 0.5f).SetEase(Ease.OutCubic));
        seq.Join(canvasGroup.DOFade(1, 0.5f));

        return seq;
    }

    public override Tween Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScaleY(0, 0.4f).SetEase(Ease.InCubic));
        seq.Join(canvasGroup.DOFade(0, 0.4f));
        seq.OnComplete(() => gameObject.SetActive(false));

        return seq;
    }
}