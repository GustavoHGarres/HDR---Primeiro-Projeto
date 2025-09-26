using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EquipmentManager equip;

    [Header("Head")]
    [SerializeField] private Image headIcon;
    [SerializeField] private TMP_Text headLabel;

    [Header("Chest")]
    [SerializeField] private Image chestIcon;
    [SerializeField] private TMP_Text chestLabel;

    [Header("Arms")]
    [SerializeField] private Image armsIcon;
    [SerializeField] private TMP_Text armsLabel;

    [Header("Legs")]
    [SerializeField] private Image legsIcon;
    [SerializeField] private TMP_Text legsLabel;

    private void OnEnable()
    {
        if (!equip) equip = EquipmentManager.Instance;
        if (equip != null)
        {
            equip.OnEquipmentChanged += Refresh;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (equip != null) equip.OnEquipmentChanged -= Refresh;
    }

    private void Refresh()
    {
        SetSlot(ItemSlot.Head,  equip.GetEquipped(ItemSlot.Head),  headIcon,  headLabel);
        SetSlot(ItemSlot.Chest, equip.GetEquipped(ItemSlot.Chest), chestIcon, chestLabel);
        SetSlot(ItemSlot.Arms,  equip.GetEquipped(ItemSlot.Arms),  armsIcon,  armsLabel);
        SetSlot(ItemSlot.Legs,  equip.GetEquipped(ItemSlot.Legs),  legsIcon,  legsLabel);
    }

    private void SetSlot(ItemSlot slot, ItemDefinition def, Image icon, TMP_Text label)
    {
        if (def != null)
        {
            if (icon)  { icon.sprite = def.icon; icon.enabled = def.icon != null; }
            if (label) label.text = def.displayName;
        }
        else
        {
            if (icon)  { icon.sprite = null; icon.enabled = false; }
            if (label) label.text = "—";
        }
    }
}
