using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestSlotBinder : MonoBehaviour
{
    [Header("Referências do Slot (UI)")]
    public Button slotButton;       // o Button do Slot_Chest
    public Image  slotIcon;         // a imagem do ícone no slot (ex.: Imagem_Chest)
    public TMP_Text slotLabel;      // o texto "Chest"

    [Header("Estado visual de bloqueio")]
    public CanvasGroup lockOverlay; // opcional: um overlay/ícone de cadeado
    public bool disableRaycastWhenLocked = true;

    void OnEnable()
    {
        ApplyState(); // aplica ao abrir a cena
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnProgressChanged += ApplyState;
    }

    void OnDisable()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnProgressChanged -= ApplyState;
    }

    void ApplyState()
    {
        bool unlocked = GameProgress.Instance && GameProgress.Instance.chestUnlocked;

        // interação do botão
        if (slotButton) slotButton.interactable = unlocked;

        // overlay de bloqueio
        if (lockOverlay)
        {
            lockOverlay.alpha = unlocked ? 0f : 1f;
            lockOverlay.blocksRaycasts = !unlocked && disableRaycastWhenLocked;
            lockOverlay.interactable = false;
        }

        // feedback visual simples (opcional)
        if (slotIcon)  slotIcon.color  = unlocked ? Color.white : new Color(1,1,1,0.4f);
        if (slotLabel) slotLabel.alpha = unlocked ? 1f : 0.5f;
    }
}
