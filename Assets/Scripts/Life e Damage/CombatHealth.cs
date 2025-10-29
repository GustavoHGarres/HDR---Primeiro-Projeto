using UnityEngine;
using UnityEngine.Events;

public class CombatHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Invencibilidade pós-hit")]
    public float hitInvulSeconds = 0.25f;

    [Header("Knockback (opcional)")]
    public bool applyKnockback = true;
    public float knockbackForce = 7f;

    [Header("Eventos")]
    public UnityEvent<float, float> OnDamage; // (current, max)
    public UnityEvent OnDeath;

    float _invulUntil;

    void Awake() => currentHealth = Mathf.Clamp(maxHealth, 1f, 999999f);

    public bool IsAlive => currentHealth > 0f;

    public void Heal(float value)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Abs(value));
    }

    public void Kill()
    {
        if (!IsAlive) return;
        currentHealth = 0f;
        OnDeath?.Invoke();
    }

    public bool TryDamage(float amount, Vector3 hitPoint, Vector3 hitDir, Rigidbody optionalRb = null, CharacterController optionalCC = null)
    {
        if (Time.time < _invulUntil || !IsAlive) return false;

        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Abs(amount));
        _invulUntil = Time.time + hitInvulSeconds;

        if (applyKnockback && (optionalRb || optionalCC))
        {
            var dir = new Vector3(hitDir.x, 0f, hitDir.z).normalized;
            if (optionalRb)
                optionalRb.AddForce(dir * knockbackForce, ForceMode.Impulse);
            else if (optionalCC)
                optionalCC.Move(dir * (knockbackForce * 0.08f)); // empurrãozinho
        }

        OnDamage?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) OnDeath?.Invoke();

        return true;
    }
}
