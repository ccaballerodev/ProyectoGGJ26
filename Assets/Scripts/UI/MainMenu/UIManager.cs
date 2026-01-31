using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private UIPanelBase mainPanel;
    [SerializeField] private UIPanelBase optionsPanel;
    [SerializeField] private UIPanelBase hostPanel;
    [SerializeField] private UIPanelBase artPanel;

    private UIPanelBase currentPanel;
    private bool isTransitioning;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentPanel = mainPanel;
        currentPanel.Show();
    }

    public void ShowPanel(UIPanelBase target)
    {
        if (isTransitioning || target == currentPanel) return;

        isTransitioning = true;

        Sequence seq = DOTween.Sequence();

        if (currentPanel != null)
            seq.Append(currentPanel.Hide());

        seq.Append(target.Show());

        seq.OnComplete(() =>
        {
            currentPanel = target;
            isTransitioning = false;
        });
    }

    // Métodos públicos para botones
    public void GoToMain() => ShowPanel(mainPanel);
    public void GoToOptions() => ShowPanel(optionsPanel);
    public void GoToHost() => ShowPanel(hostPanel);
    public void GoToArt() => ShowPanel(artPanel);
}