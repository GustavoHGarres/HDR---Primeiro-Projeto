using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    public int tabIndex;             // índice desta Tab (0..n)
    public Image highlight;          // uma imagem/borda dourada
    public MenuTabs tabs;            // arraste o MenuTabs aqui

    void OnEnable()
    {
        if (tabs) tabs.OnTabChanged.AddListener(HandleChanged);
    }
    void OnDisable()
    {
        if (tabs) tabs.OnTabChanged.RemoveListener(HandleChanged);
    }
    void HandleChanged(int idx)
    {
        if (highlight) highlight.enabled = (idx == tabIndex);
    }

    // Ligue isto no OnClick do próprio Button
    public void ClickOpenTab() => tabs.OpenTab(tabIndex);
}
