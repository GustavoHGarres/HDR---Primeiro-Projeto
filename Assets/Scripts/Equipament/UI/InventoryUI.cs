using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform gridParent;           // Content do ScrollView
    [SerializeField] private UIInventoryItemSlot slotPrefab; // PFB_UI_InventorySlot
    [SerializeField] private List<ItemDefinition> items;     // seus ITM_*_Santa
    [SerializeField] private EquipmentManager equipmentManager;

    private void Start() => Build();

    public void Build()
    {
        foreach (Transform t in gridParent) Destroy(t.gameObject);
        foreach (var def in items)
        {
            var slot = Instantiate(slotPrefab, gridParent);
            slot.Setup(def, equipmentManager);
        }
    }
}
