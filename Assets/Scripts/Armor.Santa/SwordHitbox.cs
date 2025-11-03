using UnityEngine;
using System.Linq;
using System.Reflection;

[RequireComponent(typeof(Collider))]
public class SwordHitbox : MonoBehaviour
{
    [Header("Configuração de Dano")]
    public float damage = 10f;
    public LayerMask hitMask = ~0; // por padrão atinge tudo

    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        EnableHit(false);
    }

    /// <summary>Ativa ou desativa o hitbox (usado via evento de animação)</summary>
    public void EnableHit(bool on)
    {
        if (col) col.enabled = on;
    }

    void OnTriggerEnter(Collider other)
    {
        // Se não for uma layer atingível, sai
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        var targetGO = other.GetComponentInParent<Transform>()?.gameObject;
        if (!targetGO || targetGO == gameObject) return;

        // 1️⃣ Caso o alvo use o sistema avançado IDamageable
        var dmgAdvanced = targetGO.GetComponentInParent<IDamageable>();
        if (dmgAdvanced != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDir = (targetGO.transform.position - transform.position).normalized;
            dmgAdvanced.Damage(damage, hitPoint, hitDir);
            return;
        }

        // 2️⃣ Caso o alvo use o sistema simples (IDamageableSimple)
        var dmgSimple = targetGO.GetComponentInParent<IDamageableSimple>();
        if (dmgSimple != null)
        {
            dmgSimple.TakeDamage(damage);
            return;
        }

        // 3️⃣ Caso tenha um script de vida chamado CombatHealth (ou similar)
        if (TryInvokeCombatHealth(targetGO, damage))
            return;

        // 4️⃣ Fallback universal — envia mensagem
        targetGO.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
        targetGO.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    bool TryInvokeCombatHealth(GameObject go, float amount)
    {
        var comp = go.GetComponentsInParent<Component>(true)
            .FirstOrDefault(c => c != null && c.GetType().Name.Contains("CombatHealth"));

        if (comp == null) return false;

        var t = comp.GetType();
        var method = t.GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(comp, new object[] { amount });
            return true;
        }
        return false;
    }
}
