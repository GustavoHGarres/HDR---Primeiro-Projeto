using UnityEngine;

public class EquipamentEnableManager : MonoBehaviour
{
    public static EquipamentEnableManager Instance { get; private set; }

    [Header("Refs do Player (na cena de gameplay)")]
    public GameObject chest_Santa_01; // arraste aqui seu objeto "Chest_Santa_01"

    bool _chestOn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetChestActive(bool active)
    {
        _chestOn = active;
        if (chest_Santa_01) chest_Santa_01.SetActive(active);
    }

    public void ToggleChest()
    {
        SetChestActive(!_chestOn);
    }

    public bool IsChestActive() => _chestOn;
}
