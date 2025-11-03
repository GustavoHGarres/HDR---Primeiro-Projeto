using UnityEngine;
using UnityEngine.Events;

public class CombatHealth : MonoBehaviour, IDamageable, IDamageableSimple
{
    [Header("Vida")]
    [Min(1f)] public float maxHealth = 100f;
    public float currentHealth = 100f; // alterado apenas pelos métodos

    [Header("Invencibilidade pós-hit")]
    [Min(0f)] public float hitInvulSeconds = 0.25f;

    [Header("Knockback (opcional)")]
    public bool applyKnockback = true;
    public float knockbackForce = 7f;

    [Header("Eventos")]
    public UnityEvent<float, float> OnDamage; // (current, max)
    public UnityEvent OnDeath;

    float _invulUntil;

    // caches para knockback automático
    Rigidbody _rb;
    CharacterController _cc;

    public bool IsAlive => currentHealth > 0f;

    void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;

        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CharacterController>();
    }

    public void Heal(float value)
    {
        if (value <= 0f || !IsAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + value);
        OnDamage?.Invoke(currentHealth, maxHealth);
    }

    public void Kill()
    {
        if (!IsAlive) return;
        currentHealth = 0f;
        OnDamage?.Invoke(currentHealth, maxHealth);
        OnDeath?.Invoke();
    }

    /// <summary>Lógica central de dano. Retorna true se aplicou dano.</summary>
    public bool TryDamage(
        float amount,
        Vector3 hitPoint,
        Vector3 hitDir,
        Rigidbody optionalRb = null,
        CharacterController optionalCC = null)
    {
        if (!IsAlive) return false;
        if (Time.time < _invulUntil) return false;

        float dmg = Mathf.Abs(amount);
        if (dmg <= 0f) return false;

        currentHealth = Mathf.Max(0f, currentHealth - dmg);
        _invulUntil = Time.time + hitInvulSeconds;

        // Knockback opcional
        if (applyKnockback && (optionalRb != null || optionalCC != null))
        {
            var dir = new Vector3(hitDir.x, 0f, hitDir.z).normalized;
            if (dir.sqrMagnitude > 0.0001f)
            {
                if (optionalRb != null)
                    optionalRb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                else if (optionalCC != null)
                    optionalCC.Move(dir * (knockbackForce * 0.08f));
            }
        }

        OnDamage?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) OnDeath?.Invoke();

        return true;
    }

    // ===== Integrações =====

    public void Damage(float amount, Vector3 hitPoint, Vector3 hitDir)
    {
        TryDamage(amount, hitPoint, hitDir, _rb, _cc);
    }

    public void TakeDamage(float amount)
    {
        TryDamage(amount, transform.position, Vector3.zero, _rb, _cc);
    }

    // Overload para SendMessage("Damage", amount)
    public void Damage(float amount)
    {
        TakeDamage(amount);
    }

#if UNITY_EDITOR
    // Utilidades de teste no Inspector (não requer using UnityEditor)
    [ContextMenu("Teste/Aplicar 10 de dano")]
    void __TestDamage10() { TakeDamage(10f); Debug.Log($"[CombatHealth] -10 | {currentHealth}/{maxHealth}", this); }

    [ContextMenu("Teste/Kill (matar)")]
    void __TestKill() { Kill(); Debug.Log("[CombatHealth] Kill()", this); }

    [ContextMenu("Teste/Curar 10")]
    void __TestHeal10() { Heal(10f); Debug.Log($"[CombatHealth] +10 | {currentHealth}/{maxHealth}", this); }
#endif
}
