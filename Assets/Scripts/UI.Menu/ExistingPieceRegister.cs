using UnityEngine;

public class ExistingPieceRegister : MonoBehaviour
{
    [Tooltip("Chave única para esta peça (use a mesma no botão da UI).")]
    public string key;

    void Awake()
    {
        if (string.IsNullOrWhiteSpace(key))
            key = gameObject.name;

        EquipmentManager.Instance?.RegisterExistingPiece(key, gameObject);

        // Aplica estado salvo imediatamente (caso o manager ainda não tenha feito)
        bool on = EquipmentManager.Instance?.IsEquippedExistingByKey(key) ?? false;
        gameObject.SetActive(on);
    }

    void OnDestroy()
    {
        EquipmentManager.Instance?.UnregisterExistingPiece(key, gameObject);
    }
}
