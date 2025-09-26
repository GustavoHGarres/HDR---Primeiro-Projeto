using UnityEngine;

public enum ItemSlot { Head, Chest, Arms, Legs, None }

[CreateAssetMenu(menuName = "Game/Item", fileName = "ITM_NewItem")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identidade")]
    public string id;                 // ex: "shirt_01"
    public string displayName = "Item";
    [TextArea] public string description;
    public Sprite icon;

    // >>> Propriedades para a UI (mantém compatível com o UIInventoryItemSlot)
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    [Header("Slot / Visual")]
    public ItemSlot slot = ItemSlot.Chest;
    public GameObject prefabVisual;   // FBX/Prefab da peça (SkinnedMesh ou mesh/obj)

    [Header("Opcional: Atributos")]
    public int armor;
    public int weight;
}
