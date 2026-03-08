using UnityEngine;

public class TrumpMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private bool startVisible;

    public bool IsOpen => targetPanel != null && targetPanel.activeSelf;

    private void Awake()
    {
        ApplyStartState();
    }

    public void Initialize(GameObject panel, bool visible)
    {
        targetPanel = panel;
        startVisible = visible;
        ApplyStartState();
    }

    public void Toggle()
    {
        if (targetPanel == null)
        {
            targetPanel = gameObject;
        }

        targetPanel.SetActive(!targetPanel.activeSelf);
    }

    public void Show()
    {
        if (targetPanel == null)
        {
            targetPanel = gameObject;
        }

        targetPanel.SetActive(true);
    }

    public void Hide()
    {
        if (targetPanel == null)
        {
            targetPanel = gameObject;
        }

        targetPanel.SetActive(false);
    }

    private void ApplyStartState()
    {
        if (targetPanel == null)
        {
            targetPanel = gameObject;
        }

        targetPanel.SetActive(startVisible);
    }
}
