using UnityEngine;

public class MenuLinks : MonoBehaviour
{
    [Header("Painéis principais (mutuamente exclusivos)")]
    public GameObject panelInventoryEquipTotal; // Equipament
    public GameObject panelInventorySpellTotal; // Spell
    public GameObject panelInventoryItemsTotal; // Item

    [Header("Subpainéis do Inventory (mutuamente exclusivos)")]
    public GameObject panelInventoryHead;       // Abre ao clicar em Head
    // (se já tiver os outros, pode expor aqui também futuramente)
    // public GameObject panelInventoryChest;
    // public GameObject panelInventoryArms;
    // public GameObject panelInventoryLegs;

    GameObject[] _main;
    GameObject[] _subs;

    void Awake()
    {
        _main = new[] { panelInventoryEquipTotal, panelInventorySpellTotal, panelInventoryItemsTotal };
        _subs = new[] { panelInventoryHead /*, panelInventoryChest, panelInventoryArms, panelInventoryLegs*/ };
    }

    void ShowOnly(GameObject[] group, GameObject target)
    {
        foreach (var go in group)
        {
            if (!go) continue;
            go.SetActive(go == target);
        }
    }

    // ===== TopBar (fixos): ligue estes métodos nos OnClick =====
    public void ShowEquip() => ShowOnly(_main, panelInventoryEquipTotal);
    public void ShowSpell() => ShowOnly(_main, panelInventorySpellTotal);
    public void ShowItems() => ShowOnly(_main, panelInventoryItemsTotal);

    // ===== Slots dentro de Equip =====
    public void OpenHead() => ShowOnly(_subs, panelInventoryHead);

    // opcional: fechar subpainéis
    public void CloseSubPanels() => ShowOnly(_subs, null);
}
