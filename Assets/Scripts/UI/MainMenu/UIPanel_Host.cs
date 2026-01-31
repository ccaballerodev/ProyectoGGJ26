using DG.Tweening;
using UnityEngine;

public class UIPanel_Host : UIPanelBase
{
    public override Tween Show()
    {
        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.05f, 0.4f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1f, 0.15f));
        seq.Join(canvasGroup.DOFade(1f, 0.3f));

        return seq;
    }

    public override Tween Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0.9f, 0.3f));
        seq.Join(canvasGroup.DOFade(0f, 0.3f));
        seq.OnComplete(() => gameObject.SetActive(false));

        return seq;
    }
}