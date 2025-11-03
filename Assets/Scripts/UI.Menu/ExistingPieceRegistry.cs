using UnityEngine;

public class ExistingPieceRegistry : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        [Tooltip("Chave da peça. Se vazio, usa o nome do Target.")]
        public string key;

        [Tooltip("GameObject da peça no gameplay (pode estar DESATIVADO).")]
        public GameObject target;

        [Tooltip("Se não houver save ainda, liga/desliga por padrão?")]
        public bool defaultOn;
    }

    [Header("Peças existentes para registrar (Head/Arms/Chest/Legs, etc.)")]
    public Entry[] entries;

    [Tooltip("Ao iniciar, aplicar defaults quando não houver save ainda?")]
    public bool applyDefaultsIfNoSave = true;

    void Start()
    {
        var em = EquipmentManager.Instance;
        if (!em) return;

        foreach (var e in entries)
        {
            if (!e.target) continue;

            string key = string.IsNullOrWhiteSpace(e.key) ? e.target.name : e.key;

            // registra
            em.RegisterExistingPiece(key, e.target);

            // aplica estado salvo (ou default se não houver save)
            bool on = em.IsEquippedExistingByKey(key);
            if (applyDefaultsIfNoSave && !em.HasExistingKey(key))
                on = e.defaultOn;

            em.SetExistingByKey(key, on);
        }

        // Garante que todo mundo ficou coerente com o salvo
        em.ApplySavedStateForAllExisting();
    }
}
