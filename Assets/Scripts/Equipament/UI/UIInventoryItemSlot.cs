using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryItemSlot : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    [Header("Data")]
    public ItemDefinition item;

    private EquipmentManager _equip;

    public void Setup(ItemDefinition def, EquipmentManager equip)
    {
        item = def;
        _equip = equip;

        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        if (label) label.text = def.displayName;
        if (icon)  { icon.sprite = def.icon; icon.enabled = def.icon != null; }
    }

    private void OnClick()
    {
        if (_equip != null && item != null)
            _equip.Equip(item);
    }
}
