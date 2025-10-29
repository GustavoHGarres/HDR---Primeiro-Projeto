using UnityEngine;

[RequireComponent(typeof(CombatHealth))]
public class DamageableReceiver : MonoBehaviour, IDamageable
{
    public Animator animator;                 // opcional (Hit/Death)
    public string animTriggerHit = "Hit";
    public string animTriggerDeath = "Death";

    [Header("Refs (opcionais)")]
    public Rigidbody rb;                      // caso use física
    public CharacterController characterController; // caso use CC

    CombatHealth _health;

    void Awake()
    {
        _health = GetComponent<CombatHealth>();
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!characterController) characterController = GetComponent<CharacterController>();

        _health.OnDeath.AddListener(HandleDeath);
    }

    public void Damage(float amount, Vector3 hitPoint, Vector3 hitDir)
    {
        if (!_health.IsAlive) return;

        bool applied = _health.TryDamage(amount, hitPoint, hitDir, rb, characterController);
        if (applied && animator && !string.IsNullOrEmpty(animTriggerHit))
            animator.SetTrigger(animTriggerHit);
    }

    void HandleDeath()
    {
        if (animator && !string.IsNullOrEmpty(animTriggerDeath))
            animator.SetTrigger(animTriggerDeath);
        // opcional: Destroy após X s (se não houver anima de morte completa)
        // Destroy(gameObject, 2f);
    }
}
