using System;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE
using Cinemachine;
#endif

public class PlayerSwitcherChildren : MonoBehaviour
{
    [Header("Filhos jogáveis (na ordem 1..5)")]
    public List<GameObject> variants = new List<GameObject>(5);

    [Header("Qual começa ativo (1..5)")]
    [Range(1, 5)] public int startIndex = 1;

    [Header("Desbloqueio por slot (1..5)")]
    [Tooltip("Quais slots começam desbloqueados. Normalmente só o 1 = true.")]
    public List<bool> unlocked = new List<bool>(5) { true, false, false, false, false };

#if CINEMACHINE
    [Header("Opcional (Cinemachine)")]
    public CinemachineFreeLook freeLook;
    public Transform cameraFollowOverride;
    public Transform cameraLookAtOverride;
#endif

    public event Action<int> OnPlayerChanged;                // idx 0..n (ativo)
    public event Action<int, bool> OnUnlockedChanged;        // oneBased, state
    public int ActiveIndex => _active;                       // 0..n
    public int ActiveIndexOneBased => _active < 0 ? 0 : _active + 1;
    public GameObject ActiveGO => (_active >= 0 && _active < variants.Count) ? variants[_active] : null;
    public Transform ActiveTransform => ActiveGO ? ActiveGO.transform : null;

    int _active = -1;

    void OnValidate()
    {
        // mantém listas alinhadas em tamanho
        EnsureListSize(variants, 5);
        EnsureListSize(unlocked, variants.Count);
        if (unlocked.Count > 0 && !unlocked[0]) unlocked[0] = true; // garante slot 1 desbloqueado
    }

    void EnsureListSize<T>(List<T> list, int size)
    {
        if (list == null) return;
        while (list.Count < size) list.Add(default);
        while (list.Count > size) list.RemoveAt(list.Count - 1);
    }

    void Awake()
    {
        // Desativa todos
        for (int i = 0; i < variants.Count; i++)
            if (variants[i]) variants[i].SetActive(false);

        // Ativa inicial (só se estiver desbloqueado)
        int idx = Mathf.Clamp(startIndex - 1, 0, Mathf.Max(0, variants.Count - 1));
        if (variants.Count > 0 && variants[idx] && IsUnlockedOneBased(idx + 1))
        {
            variants[idx].SetActive(true);
            _active = idx;
            HookCameraTo(variants[idx].transform);
            EnsureAnimatorReady(variants[idx]);
            OnPlayerChanged?.Invoke(_active);
        }
        else
        {
            _active = -1;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTo(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchTo(4);
    }

    // ====== API ======

    public bool IsUnlockedOneBased(int oneBasedIndex)
    {
        int i = oneBasedIndex - 1;
        return (i >= 0 && i < unlocked.Count) && unlocked[i];
    }

    public void UnlockOneBased(int oneBasedIndex)
    {
        int i = oneBasedIndex - 1;
        if (i < 0 || i >= unlocked.Count) return;
        if (!unlocked[i])
        {
            unlocked[i] = true;
            OnUnlockedChanged?.Invoke(oneBasedIndex, true);
        }
    }

    public void LockOneBased(int oneBasedIndex)
    {
        int i = oneBasedIndex - 1;
        if (i < 0 || i >= unlocked.Count) return;
        if (unlocked[i])
        {
            unlocked[i] = false;
            OnUnlockedChanged?.Invoke(oneBasedIndex, false);

            // se travou o que estava ativo, desativa
            if (_active == i && variants[i])
            {
                variants[i].SetActive(false);
                _active = -1;
                OnPlayerChanged?.Invoke(_active);
            }
        }
    }

    /// <summary>Troca para índice 0-based (ignora se o slot estiver bloqueado).</summary>
    public void SwitchTo(int index)
    {
        if (index < 0 || index >= variants.Count) return;
        if (!IsUnlockedOneBased(index + 1)) return;        // BLOQUEIO: sem coleta, sem troca
        if (index == _active) return;
        if (!variants[index]) return;

        // Pega pose anterior
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        if (_active >= 0 && _active < variants.Count && variants[_active])
        {
            var from = variants[_active].transform;
            pos = from.position;
            rot = from.rotation;
        }

        var toGO = variants[index];
        var toTr = toGO.transform;

        // Desliga CC do novo para não “empurrar” ao teleporte
        var toCC = toGO.GetComponent<CharacterController>();
        bool reEnableToCC = false;
        if (toCC && toCC.enabled) { toCC.enabled = false; reEnableToCC = true; }

        // Liga e posiciona
        toGO.SetActive(true);
        toTr.SetPositionAndRotation(pos, rot);

        // Desliga antigo
        if (_active >= 0 && _active < variants.Count && variants[_active])
            variants[_active].SetActive(false);

        if (reEnableToCC) toCC.enabled = true;

        // Zera física
        var rb = toGO.GetComponent<Rigidbody>(); if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        var rb2d = toGO.GetComponent<Rigidbody2D>(); if (rb2d) { rb2d.velocity = Vector2.zero; rb2d.angularVelocity = 0f; }

        _active = index;
        HookCameraTo(toTr);
        EnsureAnimatorReady(toGO); // 🔒 evita travar animações ao ativar
        OnPlayerChanged?.Invoke(_active);
    }

    public void SwitchToOneBased(int oneBasedIndex) => SwitchTo(Mathf.Clamp(oneBasedIndex - 1, 0, variants.Count - 1));
    public void Next() { if (variants.Count == 0) return; int next = (_active + 1 + variants.Count) % variants.Count; SwitchTo(next); }
    public void Prev() { if (variants.Count == 0) return; int prev = (_active - 1 + variants.Count) % variants.Count; SwitchTo(prev); }

    // ====== Câmera ======
    void HookCameraTo(Transform activeRoot)
    {
#if CINEMACHINE
        if (!freeLook) return;
        Transform follow = cameraFollowOverride ? cameraFollowOverride : FindChildByName(activeRoot, "CameraTarget", "Head", "Spine", "Hips");
        Transform lookAt = cameraLookAtOverride ? cameraLookAtOverride : FindChildByName(activeRoot, "Head", "CameraTarget", "Spine");
        if (!follow) follow = activeRoot;
        if (!lookAt)  lookAt  = activeRoot;
        freeLook.Follow = follow;
        freeLook.LookAt = lookAt;
#endif
    }

    // ====== Anti-trava de animação ======
    void EnsureAnimatorReady(GameObject go)
    {
        var anim = go.GetComponent<Animator>();
        if (!anim) return;

        // Corrige animadores que “congelam” ao SetActive(true)
        // Rebind + Update(0) garante estado base correto
        anim.Update(0f);
        anim.Rebind();
        anim.Update(0f);

        // Se tiver um estado "Idle" no Layer 0, força um crossfade suave
        if (anim.HasState(0, Animator.StringToHash("Idle")))
        {
            anim.CrossFade("Idle", 0.05f, 0, 0f);
        }

        // Garante velocidade
        anim.speed = 1f;
    }

#if CINEMACHINE
    Transform FindChildByName(Transform root, params string[] names)
    {
        foreach (var n in names)
        {
            var t = root.Find(n);
            if (t) return t;
        }
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            foreach (var n in names)
                if (t.name.Equals(n, StringComparison.Ordinal)) return t;
        return null;
    }
#endif
}
