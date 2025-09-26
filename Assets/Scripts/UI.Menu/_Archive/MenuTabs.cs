using UnityEngine;
using UnityEngine.Events;

public class MenuTabs : MonoBehaviour
{
    [Header("Painéis em ordem das Tabs")]
    public GameObject[] panels;

    [Header("Opcional: evento ao trocar de aba")]
    public UnityEvent<int> OnTabChanged; // envia o índice da aba

    int current = -1;

    void Start() { OpenTab(0); } // abre a primeira aba

    public void OpenTab(int index)
    {
        if (panels == null || panels.Length == 0) return;
        for (int i = 0; i < panels.Length; i++)
            if (panels[i]) panels[i].SetActive(i == index);

        current = index;
        OnTabChanged?.Invoke(index);
    }
}
