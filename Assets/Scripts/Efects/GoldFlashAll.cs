using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldFlashAll : MonoBehaviour
{
    [Header("Raiz para varrer (ex.: Shape.Golden ou a própria skin)")]
    public Transform root; // arraste o objeto que contém os meshes dourados
    [Header("Opcional: filtrar por nome do Renderer (contém)")]
    public string nameContains = ""; // ex.: "Golden" (ou deixe vazio)

    [Header("Config Emission")]
    [ColorUsage(true,true)] public Color emissionColor = new Color(1f,0.9f,0.25f,1f);
    [Range(0f,50f)] public float idleStrength = 2f;
    [Range(0f,80f)] public float attackStrength = 20f;
    [Range(0.05f,1f)] public float flashTime = 0.18f;

    static readonly string[] COLOR_KEYS = { "_EmissionColor", "_EmissiveColor", "_EmissiveColorLDR" };
    static readonly string[] STRENGTH_KEYS = { "_EmissionStrength", "_EmissiveIntensity", "_EmissiveExposureWeight" };

    List<Material> mats = new List<Material>();
    Dictionary<Material,(string colorProp,string strengthProp)> cache = new();

    void Awake()
    {
        if (!root) root = transform;

        // coleta TODOS os renderers filhos
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (!string.IsNullOrEmpty(nameContains) && !r.name.Contains(nameContains)) continue;
            // instancia materiais editáveis
            var arr = r.materials;
            foreach (var m in arr)
            {
                if (m == null) continue;
                if (!mats.Contains(m)) mats.Add(m);
                if (!cache.ContainsKey(m)) cache[m] = DetectProps(m);
            }
        }
        SetEmission(idleStrength);
    }

    (string colorProp,string strengthProp) DetectProps(Material m)
    {
        string c = null, s = null;
        foreach (var k in COLOR_KEYS) if (m.HasProperty(k)) { c = k; break; }
        foreach (var k in STRENGTH_KEYS) if (m.HasProperty(k)) { s = k; break; }

        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissiveColor")) m.EnableKeyword("_EMISSIVE_COLOR");
        return (c,s);
    }

    void Apply(Material m, float strength)
    {
        var p = cache[m];
        if (!string.IsNullOrEmpty(p.colorProp))
            m.SetColor(p.colorProp, emissionColor);

        if (!string.IsNullOrEmpty(p.strengthProp))
            m.SetFloat(p.strengthProp, strength);
        else if (!string.IsNullOrEmpty(p.colorProp))
            m.SetColor(p.colorProp, emissionColor * Mathf.Max(1f, strength));
    }

    void SetEmission(float strength)
    {
        foreach (var m in mats) Apply(m, strength);
    }

    [NaughtyAttributes.Button]
    public void EnableIdle() => SetEmission(idleStrength);

    [NaughtyAttributes.Button]
    public void DisableEmission() => SetEmission(0f);

    [NaughtyAttributes.Button]
    public void AttackFlashTest() => StartCoroutine(FlashRoutine());

    [NaughtyAttributes.Button]
    public void AttackFlash() => StartCoroutine(FlashRoutine());

    IEnumerator FlashRoutine()
    {
        SetEmission(attackStrength);
        yield return new WaitForSeconds(flashTime);
        SetEmission(idleStrength);
    }
}