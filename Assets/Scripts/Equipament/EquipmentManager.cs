using UnityEngine;
using System;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Persistência")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Anchors no Player")]
    public Transform headAnchor;
    public Transform chestAnchor;
    public Transform armsAnchor;
    public Transform legsAnchor;

    // Visual atual por slot
    private readonly Dictionary<ItemSlot, GameObject> _currentVisuals = new();

    // NEW: ItemDefinition atual por slot (para a UI)
    private readonly Dictionary<ItemSlot, ItemDefinition> _currentItems = new();

    // NEW: Evento para a UI
    public event Action OnEquipmentChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    /// <summary>Equipe 1 peça.</summary>
    public void Equip(ItemDefinition def)
    {
        if (!def || def.slot == ItemSlot.None) return;

        // remove visual anterior do slot
        Unequip(def.slot);

        // instancia novo visual
        var anchor = AnchorFor(def.slot);
        if (anchor && def.prefabVisual)
        {
            var go = Instantiate(def.prefabVisual, anchor);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale    = Vector3.one;

            _currentVisuals[def.slot] = go;
        }

        // registra o item equipado para a UI
        _currentItems[def.slot] = def;

        OnEquipmentChanged?.Invoke();
    }

    /// <summary>Remove a peça de um slot.</summary>
    public void Unequip(ItemSlot slot)
    {
        if (_currentVisuals.TryGetValue(slot, out var go) && go)
            Destroy(go);
        _currentVisuals.Remove(slot);

        _currentItems.Remove(slot);

        OnEquipmentChanged?.Invoke();
    }

    /// <summary>Remove todas as peças.</summary>
    [ContextMenu("Unequip All")]
    public void UnequipAll()
    {
        Unequip(ItemSlot.Head);
        Unequip(ItemSlot.Chest);
        Unequip(ItemSlot.Arms);
        Unequip(ItemSlot.Legs);
        // o evento já dispara a cada Unequip acima
    }

    /// <summary>Retorna o GameObject visual equipado no slot (se houver).</summary>
    public GameObject GetEquippedVisual(ItemSlot slot)
    {
        _currentVisuals.TryGetValue(slot, out var go);
        return go;
    }

    /// <summary>NEW: Retorna o ItemDefinition equipado no slot (se houver).</summary>
    public ItemDefinition GetEquipped(ItemSlot slot)
    {
        _currentItems.TryGetValue(slot, out var def);
        return def;
    }

    // --------- SUPORTE ---------
    private Transform AnchorFor(ItemSlot s) =>
        s switch
        {
            ItemSlot.Head  => headAnchor,
            ItemSlot.Chest => chestAnchor,
            ItemSlot.Arms  => armsAnchor,
            ItemSlot.Legs  => legsAnchor,
            _              => null
        };
}
