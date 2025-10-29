using System.Collections;
using UnityEngine;

public class PlayerSwordHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    public Transform hitOrigin;      // um empty na ponta da espada
    public float hitRadius = 1.2f;
    public LayerMask targetMask;     // inclua "Enemy"
    public int damage = 15;

    [Header("Janela de ataque")]
    public float activeSeconds = 0.12f;

    bool _active;

    void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = new Color(0,1,0,.5f);
        Gizmos.DrawWireSphere(hitOrigin.position, hitRadius);
    }

    // chame via Animation Event: BeginSwordHit()
    public void BeginSwordHit() { if (!_active) StartCoroutine(HitWindow()); }

    IEnumerator HitWindow()
    {
        _active = true;
        float t = 0f;
        while (t < activeSeconds)
        {
            DoHit();
            t += Time.deltaTime;
            yield return null; // varre a cada frame
        }
        _active = false;
    }

    void DoHit()
    {
        if (!hitOrigin) return;
        var hits = Physics.OverlapSphere(hitOrigin.position, hitRadius, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var dmg = h.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector3 dir = (h.transform.position - hitOrigin.position); dir.y = 0f;
                dmg.Damage(damage, hitOrigin.position, dir.normalized);
            }
        }
    }
}
