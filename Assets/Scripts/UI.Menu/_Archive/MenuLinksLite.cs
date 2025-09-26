using UnityEngine;

public class MenuLinksLite : MonoBehaviour
{
    [Header("Painéis PRINCIPAIS")]
    public GameObject panelEquip;   // Panel_Inventory_Equip_Total
    public GameObject panelSpell;   // Panel_Inventory_Spell_Total
    public GameObject panelItems;   // Panel_Inventory_Items_Total

    [Header("SUB-painéis (dentro do Equip)")]
    public GameObject panelHead;    // Panel_Inventory_Head

    // --- TopBar ---
    public void ShowEquip()
    {
        if (panelEquip) panelEquip.SetActive(true);
        if (panelSpell) panelSpell.SetActive(false);
        if (panelItems) panelItems.SetActive(false);
    }

    public void ShowSpell()
    {
        if (panelEquip) panelEquip.SetActive(false);
        if (panelSpell) panelSpell.SetActive(true);
        if (panelItems) panelItems.SetActive(false);
    }

    public void ShowItems()
    {
        if (panelEquip) panelEquip.SetActive(false);
        if (panelSpell) panelSpell.SetActive(false);
        if (panelItems) panelItems.SetActive(true);
    }

    // --- Slot Head ---
    public void OpenHead()
    {
        if (panelHead) panelHead.SetActive(true);
        // se quiser fechar os outros subpainéis quando existirem, adicione aqui
    }
}
