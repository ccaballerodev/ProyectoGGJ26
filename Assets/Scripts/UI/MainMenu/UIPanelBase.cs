using DG.Tweening;
using UnityEngine;

public abstract class UIPanelBase : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    protected bool isVisible;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public abstract Tween Show();
    public abstract Tween Hide();
}