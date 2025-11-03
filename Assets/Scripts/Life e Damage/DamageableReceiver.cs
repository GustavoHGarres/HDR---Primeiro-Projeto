using UnityEngine;

/// <summary>
/// Adaptador universal de dano para personagens/chefs.
/// - Encaminha dano para CombatHealth (com invul e knockback já tratados lá).
/// - Aceita tanto IDamageable (amount, hitPoint, hitDir) quanto IDamageableSimple (amount).
/// - Dispara Stagger/Death no Animator (opcional).
/// </summary>
[DisallowMultipleComponent]
public class DamageableReceiver : MonoBehaviour, IDamageable, IDamageableSimple
{
    [Header("Refs")]
    [Tooltip("Se vazio, procura no mesmo GO.")]
    public CombatHealth combatHealth;
    [Tooltip("Animator que terá os triggers (Stagger/Death) disparados.")]
    public Animator animator;

    [Header("Animação (opcional)")]
    public bool triggerStaggerOnHit = true;
    [Tooltip("Dano mínimo no golpe para considerar Stagger.")]
    public float staggerMinDamage = 1f;
    [Tooltip("Cooldown entre staggers (s).")]
    public float staggerCooldown = 0.35f;
    public string staggerTrigger = "Stagger";
    public string deathTrigger   = "Death";

    [Header("Reação (opcional)")]
    [Tooltip("Gira o personagem levemente para a direção do golpe ao tomar dano.")]
    public bool faceHitDirection = true;
    public float faceTurnSpeed = 18f;

    // caches
    Rigidbody _rb;
    CharacterController _cc;
    float _lastStaggerTime = -999f;

    void Awake()
    {
        if (!combatHealth) combatHealth = GetComponent<CombatHealth>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CharacterController>();

        // Garante valores mínimos
        if (combatHealth == null)
            Debug.LogWarning($"[DamageableReceiver] CombatHealth não encontrado em {name}.", this);

        // Assina eventos do CombatHealth para animar morte automaticamente
        if (combatHealth)
        {
            combatHealth.OnDeath.AddListener(OnDeath);
            // OnDamage(current,max) já é chamado dentro do CombatHealth;
            // aqui animamos stagger via TryDamage retorno (abaixo).
        }
    }

    // ===== Implementações das interfaces =====

    /// <summary>
    /// Dano "avançado": usa hitPoint e direção do impacto.
    /// </summary>
    public void Damage(float amount, Vector3 hitPoint, Vector3 hitDir)
    {
        if (!combatHealth) return;

        // Encaminha para o núcleo do health (já respeita invul, knockback etc.)
        bool applied = combatHealth.TryDamage(
            amount,
            hitPoint,
            hitDir,
            _rb,
            _cc
        );

        if (!applied) return;

        // Reação visual/anim
        TryFaceHit(hitDir);
        TryStagger(amount);

        // Se morreu, OnDeath() será chamado via evento do CombatHealth
    }

    /// <summary>
    /// Dano "simples": sem posição/direção (útil para área, DoT, espada básica).
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!combatHealth) return;

        // Usa o centro do transform e dir zero
        Vector3 hitPoint = transform.position;
        Vector3 hitDir   = Vector3.zero;

        bool applied = combatHealth.TryDamage(
            amount,
            hitPoint,
            hitDir,
            _rb,
            _cc
        );

        if (!applied) return;

        TryStagger(amount);
        // morte é tratada por OnDeath via evento
    }

    // ===== Reações auxiliares =====

    void TryStagger(float amount)
    {
        if (!triggerStaggerOnHit || animator == null) return;
        if (amount < staggerMinDamage) return;
        if (Time.time < _lastStaggerTime + staggerCooldown) return;
        if (!combatHealth || !combatHealth.IsAlive) return;

        _lastStaggerTime = Time.time;
        if (!string.IsNullOrEmpty(staggerTrigger))
            animator.SetTrigger(staggerTrigger);
    }

    void TryFaceHit(Vector3 hitDir)
    {
        if (!faceHitDirection || hitDir.sqrMagnitude < 0.0001f) return;

        hitDir.y = 0f;
        if (hitDir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(-hitDir.normalized, Vector3.up); // olha contra a direção do impacto
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * faceTurnSpeed);
    }

    // Chamado pelo evento do CombatHealth
    void OnDeath()
    {
        if (animator && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        // Opcional: desabilitar colisores/AI aqui se quiser
        // GetComponent<NavMeshAgent>()?.ResetPath();
        // GetComponent<MonoBehaviourQualquer>()?.enabled = false;
    }
}
