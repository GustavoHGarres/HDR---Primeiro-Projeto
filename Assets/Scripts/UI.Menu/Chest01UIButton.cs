using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chest01UIButton : MonoBehaviour
{
    [Header("UI que serão ligadas/desligadas")]
    public Image slotImage_Chest01;      // ex.: Image_Chest_01
    public Image previewImage_ItemChest; // ex.: Image_Item_Chest
    public TMP_Text txtPreviewName;      // opcional (ex.: "Chest")

    void OnEnable()
    {
        // Sincroniza a UI com o estado atual do equipamento
        bool chestOn = EquipamentEnableManager.Instance 
            ? EquipamentEnableManager.Instance.IsChestActive() 
            : false;
        ApplyUI(chestOn);
    }

    // Ligue no OnClick do Btn_Chest_01
    public void OnClickToggleChest01()
    {
        if (EquipamentEnableManager.Instance)
            EquipamentEnableManager.Instance.ToggleChest();

        bool chestOnNow = EquipamentEnableManager.Instance &&
                          EquipamentEnableManager.Instance.IsChestActive();

        ApplyUI(chestOnNow);
    }

    void ApplyUI(bool on)
    {
        if (slotImage_Chest01)      slotImage_Chest01.gameObject.SetActive(on);
        if (previewImage_ItemChest) previewImage_ItemChest.gameObject.SetActive(on);

        if (txtPreviewName) txtPreviewName.text = on ? "Chest" : "";
    }
}
