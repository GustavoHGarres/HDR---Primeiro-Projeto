using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    // ItemDefinition atual por slot (para a UI)
    private readonly Dictionary<ItemSlot, ItemDefinition> _currentItems = new();

    // Evento para a UI
    public event Action OnEquipmentChanged;

    // >>> NOVO: chaves que você quer resetar (começar OFF) quando o jogo inicia
    [Header("Existing Pieces – resetar no início (começar OFF)")]
    [Tooltip("Preencha com os nomes exatos das peças existentes, ex.: ARMA#1_23927.Shape, HeadPiece_01, etc.")]
    [SerializeField] private List<string> resetExistingKeysOnStart = new();

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

    void Start()
    {
        // Se quiser que SEMPRE comece pelado: zera o estado salvo dessas chaves
        if (resetExistingKeysOnStart != null)
        {
            foreach (var key in resetExistingKeysOnStart)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                PlayerPrefs.SetInt(PREF_EXISTING_PREFIX + key, 0); // força OFF
            }
            PlayerPrefs.Save();
        }

        // Aplica o que estiver salvo (depois do reset acima, tudo ficará OFF)
        ApplySavedStateForAllExisting();
    }

    // ---------------- EQUIP / UNEQUIP via ItemDefinition ----------------

    public void Equip(ItemDefinition def)
    {
        if (!def || def.slot == ItemSlot.None) return;

        Unequip(def.slot);

        var anchor = AnchorFor(def.slot);
        if (anchor && def.prefabVisual)
        {
            var go = Instantiate(def.prefabVisual, anchor);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale    = Vector3.one;

            _currentVisuals[def.slot] = go;
        }

        _currentItems[def.slot] = def;
        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(ItemSlot slot)
    {
        if (_currentVisuals.TryGetValue(slot, out var go) && go)
            Destroy(go);
        _currentVisuals.Remove(slot);

        _currentItems.Remove(slot);
        OnEquipmentChanged?.Invoke();
    }

    [ContextMenu("Unequip All")]
    public void UnequipAll()
    {
        Unequip(ItemSlot.Head);
        Unequip(ItemSlot.Chest);
        Unequip(ItemSlot.Arms);
        Unequip(ItemSlot.Legs);
    }

    public GameObject GetEquippedVisual(ItemSlot slot)
    {
        _currentVisuals.TryGetValue(slot, out var go);
        return go;
    }

    public ItemDefinition GetEquipped(ItemSlot slot)
    {
        _currentItems.TryGetValue(slot, out var def);
        return def;
    }

    private Transform AnchorFor(ItemSlot s) =>
        s switch
        {
            ItemSlot.Head  => headAnchor,
            ItemSlot.Chest => chestAnchor,
            ItemSlot.Arms  => armsAnchor,
            ItemSlot.Legs  => legsAnchor,
            _              => null
        };

    // ---------------- EXISTING PIECES (peças já na cena do gameplay) ----------------
    //
    // Objetivo: permitir que a UI (em outra cena aditiva) ligue/desligue
    // quaisquer objetos existentes do gameplay por uma "chave" (string).
    //
    // Como usar:
    //  - No objeto do gameplay (ex.: ARMA#1_23927.Shape) adicione o script
    //    ExistingPieceRegister e defina a mesma chave que usará no botão.
    //  - No botão da UI, use EquipExistingButtonBinder e informe a mesma chave.
    //
    // Tudo persiste em PlayerPrefs.

    private const string PREF_EXISTING_PREFIX = "EXISTING_";

    // cache de objetos registrados na cena do gameplay
    private readonly Dictionary<string, GameObject> _existingPieces = new();

    public void RegisterExistingPiece(string key, GameObject go)
    {
        if (string.IsNullOrWhiteSpace(key) || !go) return;
        _existingPieces[key] = go;

        // Ao registrar, aplica o estado salvo
        bool on = IsEquippedExistingByKey(key);
        go.SetActive(on);
    }

    public void UnregisterExistingPiece(string key, GameObject go)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        if (_existingPieces.TryGetValue(key, out var cached) && cached == go)
            _existingPieces.Remove(key);
    }

    public bool IsEquippedExistingByKey(string key)
    {
        return PlayerPrefs.GetInt(PREF_EXISTING_PREFIX + key, 0) == 1;
    }

    public void ToggleExistingByKey(string key)
    {
        bool targetOn = !IsEquippedExistingByKey(key);
        SetExistingByKey(key, targetOn);
    }

    public void SetExistingByKey(string key, bool on)
    {
        var go = FindExistingPiece(key);
        if (go) go.SetActive(on);

        PlayerPrefs.SetInt(PREF_EXISTING_PREFIX + key, on ? 1 : 0);
        PlayerPrefs.Save();

        OnEquipmentChanged?.Invoke();
    }

    private GameObject FindExistingPiece(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        // 1) Cache primeiro
        if (_existingPieces.TryGetValue(key, out var go) && go) return go;

        // 2) Varre TODAS as cenas carregadas, incluindo filhos inativos e em profundidade
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scn = SceneManager.GetSceneAt(i);
            if (!scn.isLoaded) continue;

            var roots = scn.GetRootGameObjects();
            foreach (var r in roots)
            {
                // a) bateu no root
                if (r.name == key) return r;

                // b) deep search incluindo inativos
                var all = r.GetComponentsInChildren<Transform>(true);
                foreach (var t in all)
                {
                    if (t.name == key)
                        return t.gameObject;
                }

                // c) se você passar um caminho (ex.: "Player_1/…/ARMA#1_23927.Shape")
                var byPath = r.transform.Find(key);
                if (byPath) return byPath.gameObject;
            }
        }

        // 3) Último recurso: GameObject.Find (custo maior, mas funciona)
        return GameObject.Find(key);
    }

    // --- EXISTING PIECES: utilidades extras ---

    public bool HasExistingKey(string key)
    {
        return PlayerPrefs.HasKey(PREF_EXISTING_PREFIX + key);
    }

    /// Reaplica o estado salvo (PlayerPrefs) em todas as peças já registradas.
    public void ApplySavedStateForAllExisting()
    {
        foreach (var kv in _existingPieces)
        {
            var key = kv.Key;
            var go  = kv.Value;
            if (!go) continue;

            bool on = IsEquippedExistingByKey(key);
            go.SetActive(on);
        }
        OnEquipmentChanged?.Invoke();
    }

    public void ResetExistingByKey(string key, bool defaultOn = false)
    {
        PlayerPrefs.DeleteKey(PREF_EXISTING_PREFIX + key);
        SetExistingByKey(key, defaultOn);
    }
}
