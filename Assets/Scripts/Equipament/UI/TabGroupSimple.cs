using UnityEngine;

public class TabGroupSimple : MonoBehaviour
{
    [SerializeField] private GameObject panelEquipment;
    [SerializeField] private GameObject panelInventory;

    public void ShowEquipment()
    {
        panelEquipment.SetActive(true);
        panelInventory.SetActive(false);
    }

    public void ShowInventory()
    {
        panelEquipment.SetActive(false);
        panelInventory.SetActive(true);
    }
}
