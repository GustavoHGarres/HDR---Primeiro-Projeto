using UnityEngine;

public class OpenPanelOnClick : MonoBehaviour
{
    [Header("Painel que será ativado")]
    public GameObject targetPanel;

    public void OpenPanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }
}
