using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldFlashPerSubmesh : MonoBehaviour
{
    [Header("Renderer com os 3 materiais (Shape.Golden)")]
    public SkinnedMeshRenderer smr;

    [Header("Quais Elements aplicar (vazio = todos)")]
    public int[] materialIndices;

    [ColorUsage(true,true)] 
    public Color emissionColor = new Color(1f, 0.9f, 0.25f, 1f);

    [Range(0f, 50f)] 
    public float idleStrength = 2f;

    [Range(0f, 80f)] 
    public float attackStrength = 20f;

    [Range(0.05f, 1f)] 
    public float flashTime = 0.18f;

    static readonly string[] COLOR_KEYS    = { "_EmissionColor", "_EmissiveColor", "_EmissiveColorLDR" };
    static readonly string[] STRENGTH_KEYS = { "_EmissionStrength", "_EmissiveIntensity", "_EmissiveExposureWeight" };

    string colorProp = "_EmissionColor";
    string strengthProp = "_EmissionStrength";

    MaterialPropertyBlock mpb;

    void Awake()
    {
        if (!smr) smr = GetComponent<SkinnedMeshRenderer>();
        mpb = new MaterialPropertyBlock();

        // Detecta propriedades do primeiro material válido
        var m0 = smr.sharedMaterials != null && smr.sharedMaterials.Length > 0 ? smr.sharedMaterials[0] : null;
        if (m0)
        {
            foreach (var k in COLOR_KEYS)    if (m0.HasProperty(k)) { colorProp = k; break; }
            foreach (var k in STRENGTH_KEYS) if (m0.HasProperty(k)) { strengthProp = k; break; }

            // Liga keywords comuns
            m0.EnableKeyword("_EMISSION");
            if (m0.HasProperty("_EmissiveColor")) 
                m0.EnableKeyword("_EMISSIVE_COLOR");
        }

        SetIdle();
    }

    void ApplyToIndex(int idx, float strength)
    {
        if (!smr || idx < 0 || idx >= smr.sharedMaterials.Length) return;

        smr.GetPropertyBlock(mpb, idx);
        mpb.SetColor(colorProp, emissionColor);
        mpb.SetFloat(strengthProp, strength);
        smr.SetPropertyBlock(mpb, idx);
    }

    void SetStrength(float s)
    {
        if (materialIndices != null && materialIndices.Length > 0)
        {
            foreach (var i in materialIndices) 
                ApplyToIndex(i, s);
        }
        else
        {
            for (int i = 0; i < smr.sharedMaterials.Length; i++) 
                ApplyToIndex(i, s);
        }
    }

    [NaughtyAttributes.Button]
    public void SetIdle() => SetStrength(idleStrength);

    [NaughtyAttributes.Button]
    public void DisableEmission() => SetStrength(0f);

    [NaughtyAttributes.Button]
    public void AttackFlashTest() => StartCoroutine(Flash());

    public void AttackFlash() => StartCoroutine(Flash());

    IEnumerator Flash()
    {
        SetStrength(attackStrength);
        yield return new WaitForSeconds(flashTime);
        SetStrength(idleStrength);
    }
}