using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldFlash : MonoBehaviour
{
    [Header("Renderers DOURADOS (SkinnedMeshRenderer/Renderer)")]
    public Renderer[] goldRenderers;

    [Tooltip("Se vazio aplica em TODOS os slots; senão, limita a estes índices.")]
    public int[] materialIndices;

    [Header("Config")]
    [ColorUsage(true,true)] public Color emissionColor = new Color(1f,0.9f,0.25f,1f); // amarelo
    [Range(0f,50f)] public float idleStrength = 2f;
    [Range(0f,80f)] public float attackStrength = 20f;
    [Range(0.05f,1f)] public float flashTime = 0.18f;

    // Propriedades possíveis (Daz e HDRP)
    static readonly string[] EMISSION_COLOR_KEYS   = { "_EmissionColor", "_EmissiveColor", "_EmissiveColorLDR" };
    static readonly string[] EMISSION_STRENGTH_KEYS= { "_EmissionStrength", "_EmissiveIntensity", "_EmissiveExposureWeight" };

    // Cache por material
    class MatProps {
        public string colorProp;
        public string strengthProp;
    }
    Dictionary<Material, MatProps> cache = new Dictionary<Material, MatProps>();

    void Awake()
    {
        // Instancia materiais UMA vez (evita instanciar a cada frame)
        foreach (var r in goldRenderers)
        {
            if (!r) continue;
            var mats = r.materials; // instancia uma cópia editável
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (!cache.ContainsKey(m))
                    cache[m] = DetectProps(m);
            }
        }

        SetEmission(idleStrength); // estado inicial discreto
    }

    MatProps DetectProps(Material m)
    {
        var props = new MatProps();
        foreach (var k in EMISSION_COLOR_KEYS) if (m.HasProperty(k)) { props.colorProp = k; break; }
        foreach (var k in EMISSION_STRENGTH_KEYS) if (m.HasProperty(k)) { props.strengthProp = k; break; }

        // Liga keywords comuns de emissivo
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissiveColor")) m.EnableKeyword("_EMISSIVE_COLOR");

        return props;
    }

    void SetEmission(float strength)
    {
        foreach (var r in goldRenderers)
        {
            if (!r) continue;
            var mats = r.materials;

            if (materialIndices != null && materialIndices.Length > 0)
            {
                foreach (int idx in materialIndices)
                {
                    if (idx < 0 || idx >= mats.Length) continue;
                    ApplyToMaterial(mats[idx], strength);
                }
            }
            else
            {
                for (int i = 0; i < mats.Length; i++)
                    ApplyToMaterial(mats[i], strength);
            }

            // (Opcional) Atualiza GI – em HDRP nem sempre é necessário
            RendererExtensions.UpdateGIMaterials(r);
        }
    }

    void ApplyToMaterial(Material m, float strength)
    {
        if (!cache.TryGetValue(m, out var p)) p = cache[m] = DetectProps(m);

        // Cor
        if (!string.IsNullOrEmpty(p.colorProp))
            m.SetColor(p.colorProp, emissionColor);

        // Força/intensidade (caso exista)
        if (!string.IsNullOrEmpty(p.strengthProp))
            m.SetFloat(p.strengthProp, strength);

        // Fallback: alguns shaders usam só cor * intensidade
        if (string.IsNullOrEmpty(p.strengthProp) && !string.IsNullOrEmpty(p.colorProp))
            m.SetColor(p.colorProp, emissionColor * Mathf.Max(1f, strength));
    }

    [NaughtyAttributes.Button]
    public void EnableIdle() => SetEmission(idleStrength);

    [NaughtyAttributes.Button]
    public void DisableEmission() => SetEmission(0f);

    [NaughtyAttributes.Button]
    public void AttackFlashTest() => StartCoroutine(FlashRoutine());

    public void AttackFlash() => StartCoroutine(FlashRoutine());

    IEnumerator FlashRoutine()
    {
        SetEmission(attackStrength);
        yield return new WaitForSeconds(flashTime);
        SetEmission(idleStrength);
    }

    // Extra útil: listar nomes reais das propriedades no Console
    [NaughtyAttributes.Button]
    void DumpProps()
    {
        foreach (var r in goldRenderers)
        {
            if (!r) continue;
            foreach (var m in r.sharedMaterials)
            {
                if (!m) continue;
                var sh = m.shader;
                Debug.Log($"[DUMP] Mat: {m.name} Shader: {sh.name}");
                int count = sh.GetPropertyCount();
                for (int i = 0; i < count; i++)
                    Debug.Log($"  - {sh.GetPropertyName(i)} ({sh.GetPropertyType(i)})");
            }
        }
    }
}