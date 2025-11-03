using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    public Transform hitOrigin;             // empty na ponta da espada
    public float hitRadius = 1.2f;
    public LayerMask targetMask;            // inclua "Enemy"
    public int damage = 15;

    [Header("Janela de ataque")]
    public float activeSeconds = 0.12f;

    [Header("Detecção")]
    [Tooltip("Collide: considera triggers; Ignore: ignora triggers.")]
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Collide;

    [Header("Controle")]
    public float perTargetCooldown = 0.15f; // evita múltiplos hits no mesmo alvo
    public bool debugLog = false;

    bool _active;
    readonly Dictionary<Transform, float> _lastHitAt = new();

    void OnDisable()
    {
        _active = false;
        _lastHitAt.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = new Color(0, 1, 0, .5f);
        Gizmos.DrawWireSphere(hitOrigin.position, hitRadius);
    }

    // chame via Animation Event no frame do impacto
    public void BeginSwordHit()
    {
        if (!_active && isActiveAndEnabled)
            StartCoroutine(HitWindow());
        if (debugLog) Debug.Log("[SwordHit] Begin");
    }

    // opcional via evento no fim do swing
    public void EndSwordHit()
    {
        _active = false;
        if (debugLog) Debug.Log("[SwordHit] End");
    }

    IEnumerator HitWindow()
    {
        _active = true;
        _lastHitAt.Clear();

        float t = 0f;
        while (_active && t < activeSeconds)
        {
            DoHit();
            t += Time.deltaTime;
            yield return null;
        }

        _active = false;
    }

    void DoHit()
    {
        if (!hitOrigin) return;

        var hits = Physics.OverlapSphere(
            hitOrigin.position,
            hitRadius,
            targetMask,
            triggerMode);

        if (debugLog) Debug.Log($"[SwordHit] Overlap {hits.Length} targets");

        foreach (var h in hits)
        {
            var root = h.attachedRigidbody ? h.attachedRigidbody.transform : h.transform.root;
            if (!CanHitNow(root)) continue;

            Vector3 hitPoint = h.ClosestPoint(hitOrigin.position);
            Vector3 dir = (root.position - hitOrigin.position); dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward; else dir.Normalize();

            // 1) IDamageable (avançado)
            var adv = root.GetComponentInParent<IDamageable>();
            if (adv != null)
            {
                adv.Damage(damage, hitPoint, dir);
                if (debugLog) Debug.Log($"[SwordHit] IDamageable -> {root.name}");
                MarkHit(root);
                continue;
            }

            // 2) IDamageableSimple
            var simple = root.GetComponentInParent<IDamageableSimple>();
            if (simple != null)
            {
                simple.TakeDamage(damage);
                if (debugLog) Debug.Log($"[SwordHit] IDamageableSimple -> {root.name}");
                MarkHit(root);
                continue;
            }

            // 3) CombatHealth direto
            var ch = root.GetComponentInParent<CombatHealth>();
            if (ch != null)
            {
                ch.TryDamage(damage, hitPoint, dir);
                if (debugLog) Debug.Log($"[SwordHit] CombatHealth.TryDamage -> {root.name}");
                MarkHit(root);
                continue;
            }

            // 4) fallback
            root.gameObject.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
            if (debugLog) Debug.Log($"[SwordHit] SendMessage Damage -> {root.name}");
            MarkHit(root);
        }
    }

    bool CanHitNow(Transform t)
    {
        if (!_lastHitAt.TryGetValue(t, out var last)) return true;
        return Time.time >= last + perTargetCooldown;
    }

    void MarkHit(Transform t) => _lastHitAt[t] = Time.time;
}
