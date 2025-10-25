using UnityEngine;
using UnityEngine.UI;

public class PlayerSwitcherUI : MonoBehaviour
{
    [SerializeField] PlayerSwitcherChildren switcher;
    [SerializeField] Image[] icons; // 1..5, alguns podem começar desativados

    [Header("Cores")]
    [SerializeField] Color activeColor   = Color.white;
    [SerializeField] Color inactiveColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] Color lockedColor   = new Color(1f, 1f, 1f, 0.15f);

    void Awake()
    {
        if (!switcher) switcher = FindObjectOfType<PlayerSwitcherChildren>(true);
    }

    void OnEnable()
    {
        if (!switcher) return;
        switcher.OnPlayerChanged   -= RefreshFromActive;
        switcher.OnUnlockedChanged -= RefreshFromUnlock;
        switcher.OnPlayerChanged   += RefreshFromActive;
        switcher.OnUnlockedChanged += RefreshFromUnlock;

        FullRefresh();
    }

    void OnDisable()
    {
        if (!switcher) return;
        switcher.OnPlayerChanged   -= RefreshFromActive;
        switcher.OnUnlockedChanged -= RefreshFromUnlock;
    }

    void RefreshFromActive(int activeIdx) => FullRefresh();
    void RefreshFromUnlock(int oneBased, bool state) => FullRefresh();

    void FullRefresh()
    {
        if (icons == null) return;

        int active = switcher ? switcher.ActiveIndex : -1;

        for (int i = 0; i < icons.Length; i++)
        {
            var img = icons[i];
            if (!img) continue;

            int oneBased = i + 1;
            bool isUnlocked = switcher && switcher.IsUnlockedOneBased(oneBased);

            if (!isUnlocked)
                img.color = lockedColor;
            else
                img.color = (i == active) ? activeColor : inactiveColor;

            // Se preferir, pode ativar/desativar o GO do ícone:
            // img.gameObject.SetActive(isUnlocked);
        }
    }

    // Botão na UI para selecionar (só funciona se desbloqueado)
    public void Select(int oneBasedIndex)
    {
        if (!switcher) return;
        switcher.SwitchToOneBased(oneBasedIndex);
    }
}
