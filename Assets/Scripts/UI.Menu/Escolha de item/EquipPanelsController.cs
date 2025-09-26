using UnityEngine;

public class EquipPanelsController : MonoBehaviour
{
    [Header("Subpainéis do Equip (só um ativo por vez)")]
    public GameObject panelHead;
    public GameObject panelChest;
    public GameObject panelArms;
    public GameObject panelLegs;

    void HideAll()
    {
        if (panelHead) panelHead.SetActive(false);
        if (panelChest) panelChest.SetActive(false);
        if (panelArms) panelArms.SetActive(false);
        if (panelLegs) panelLegs.SetActive(false);
    }

    public void ShowHead()
    {
        HideAll();
        if (panelHead) panelHead.SetActive(true);
    }

    public void ShowChest()
    {
        HideAll();
        if (panelChest) panelChest.SetActive(true);
    }

    public void ShowArms()
    {
        HideAll();
        if (panelArms) panelArms.SetActive(true);
    }

    public void ShowLegs()
    {
        HideAll();
        if (panelLegs) panelLegs.SetActive(true);
    }
}
