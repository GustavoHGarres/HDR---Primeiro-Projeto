using UnityEngine;

public class ExistingPieceRef : MonoBehaviour
{
    [Tooltip("Chave da peça (use a MESMA usada no botão da UI)")]
    public string key;

    [Tooltip("Referência direta à peça, mesmo que esteja desativada")]
    public GameObject target;

    void Start()
    {
        if (string.IsNullOrWhiteSpace(key) && target) key = target.name;
        if (!string.IsNullOrWhiteSpace(key) && target)
        {
            EquipmentManager.Instance?.RegisterExistingPiece(key, target);

            // Aplica o estado salvo imediatamente
            bool on = EquipmentManager.Instance?.IsEquippedExistingByKey(key) ?? false;
            target.SetActive(on);
        }
        else
        {
            Debug.LogWarning("[ExistingPieceRef] Configure 'key' e 'target'.");
        }
    }
}
