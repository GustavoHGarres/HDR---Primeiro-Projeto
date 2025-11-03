using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class EquipExistingButtonBinder : MonoBehaviour
{
    [Header("Chave da peça existente (igual à do ExistingPieceRegister)")]
    public string pieceKey;

    [Header("UI (opcional)")]
    public TMP_Text stateLabel; // exibe "ON" / "OFF"

    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        Refresh();
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
    }

    void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;
    }

    void OnClick()
    {
        if (string.IsNullOrWhiteSpace(pieceKey)) return;
        EquipmentManager.Instance?.ToggleExistingByKey(pieceKey);
        Refresh();
    }

    void Refresh()
    {
        if (string.IsNullOrWhiteSpace(pieceKey)) return;

        bool on = EquipmentManager.Instance?.IsEquippedExistingByKey(pieceKey) ?? false;
        if (stateLabel) stateLabel.text = on ? "ON" : "OFF";
    }
}
