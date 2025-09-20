using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private HashSet<string> emblems = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromPrefs();
    }

    public void AddEmblem(string id)
    {
        if (!emblems.Contains(id))
        {
            emblems.Add(id);
            SaveToPrefs();
            Debug.Log("[Inventory] Emblem added: " + id);
        }
    }

    public bool HasEmblem(string id) => emblems.Contains(id);

    void SaveToPrefs()
    {
        // simples: concat
        var arr = string.Join(";", emblems);
        PlayerPrefs.SetString("inventory_emblems", arr);
        PlayerPrefs.Save();
    }

    void LoadFromPrefs()
    {
        var s = PlayerPrefs.GetString("inventory_emblems", "");
        if (string.IsNullOrEmpty(s)) return;
        var parts = s.Split(new char[]{';'}, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts) emblems.Add(p);
    }
}
