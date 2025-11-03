using UnityEngine;
using System.Collections;

public class BossSantaFireBlade : MonoBehaviour
{
    [Header("Prefabs de Magia")]
    [Tooltip("Explosão grande/colunas de fogo (ex.: Effect_51_Burst-OK)")]
    public GameObject attackA_Prefab;
    [Tooltip("Anel de fogo no chão (ex.: Effect_31_FireRing_A)")]
    public GameObject attackB_Prefab;

    [Header("Ajustes de Spawn")]
    public Vector3 groundOffset = new Vector3(0, 0.05f, 0);
    public float   a_delay = 0.00f;   // delay opcional antes do spawn A
    public float   b_delay = 0.00f;   // delay opcional antes do spawn B
    public float   a_lifetime = 6f;   // destruição automática
    public float   b_lifetime = 6f;

    [Header("Extras")]
    public bool faceToTargetOnCast = true; // gira o boss para olhar o alvo no momento do cast

    Transform _t;

    void Awake() => _t = transform;

    /// <summary>
    /// Ataque A (75%): explosão / colunas de fogo.
    /// </summary>
    public void CastAttackA(Vector3 center)
    {
        if (faceToTargetOnCast) Face(center);
        if (attackA_Prefab) StartCoroutine(SpawnAfterDelay(attackA_Prefab, center + groundOffset, a_delay, a_lifetime));
    }

    /// <summary>
    /// Ataque B (30%): grande anel de fogo no chão.
    /// </summary>
    public void CastAttackB(Vector3 center)
    {
        if (faceToTargetOnCast) Face(center);
        if (attackB_Prefab) StartCoroutine(SpawnAfterDelay(attackB_Prefab, center + groundOffset, b_delay, b_lifetime));
    }

    IEnumerator SpawnAfterDelay(GameObject prefab, Vector3 pos, float delay, float lifetime)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (lifetime > 0f) Destroy(go, lifetime);
    }

    void Face(Vector3 worldPos)
    {
        var dir = worldPos - _t.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            _t.rotation = Quaternion.LookRotation(dir);
    }
}
