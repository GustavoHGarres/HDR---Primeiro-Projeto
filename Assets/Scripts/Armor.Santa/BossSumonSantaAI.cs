using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class BossSumonSantaAI : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Se não for atribuído, eu encontro pela tag Player")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Refs")]
    public SwordHitbox swordHit;
    public GameObject magicA;   // magia 1 (75%) – objeto filho, desativado
    public GameObject magicB;   // magia 2 (25%) – objeto filho, desativado

    [Header("Detecção/Distâncias")]
    public float detectRadius = 25f;
    public float stopDistance = 6f;
    public float orbitAmplitude = 2f;
    public float orbitSpeed = 2f;

    [Header("Ritmo de ataque")]
    public float comboCooldown = 1.4f;

    [Header("Lógica de Combos")]
    public bool chooseComboByHP = true;
    [Range(0f, 1f)] public float combo2BelowHpPct = 0.50f;
    public bool alternateWhenBothValid = false;
    bool _nextComboIs1 = false;

    [Header("Magias automáticas")]
    public float magicLifetime = 6f;
    public bool allowMagic = true;

    [Header("Morte")]
    public float destroyDelayAfterDeath = 2f;

    // Animator params
    static readonly int MoveX  = Animator.StringToHash("MoveX");
    static readonly int MoveY  = Animator.StringToHash("MoveY");
    static readonly int Speed  = Animator.StringToHash("Speed");
    static readonly int Combo1 = Animator.StringToHash("Combo1");
    static readonly int Combo2 = Animator.StringToHash("Combo2");
    static readonly int Death  = Animator.StringToHash("Death");

    Animator anim;
    NavMeshAgent agent;
    Vector3 startPos;
    float nextComboAt;
    bool active;

    // Controle de vida
    float hpPct = 1f;
    CombatHealth _hp;
    bool isDying = false;
    bool usedMagicA = false;
    bool usedMagicB = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = stopDistance;
        startPos = transform.position;

        if (magicA) magicA.SetActive(false);
        if (magicB) magicB.SetActive(false);
    }

    void OnEnable()
    {
        if (_hp == null) _hp = GetComponent<CombatHealth>();
        if (_hp != null) _hp.OnDamage.AddListener(OnHealthChanged);
    }

    void OnDisable()
    {
        if (_hp != null) _hp.OnDamage.RemoveListener(OnHealthChanged);
    }

    void Start()
    {
        if (!target)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) target = go.transform;
        }
    }

    public void OnHealthChanged(float current, float max)
    {
        hpPct = Mathf.Clamp01(max > 0f ? current / max : 0f);

        // ativa magias conforme a vida
        if (allowMagic)
        {
            if (!usedMagicA && hpPct <= 0.75f && hpPct > 0.25f)
            {
                usedMagicA = true;
                Invoke(nameof(ActivateMagicA), 0.5f);
            }

            if (!usedMagicB && hpPct <= 0.25f)
            {
                usedMagicB = true;
                Invoke(nameof(ActivateMagicB), 0.5f);
            }
        }
    }

    void ActivateMagicA()
    {
        if (!magicA) return;
        magicA.SetActive(true);
        Invoke(nameof(DeactivateMagicA), magicLifetime);
    }

    void ActivateMagicB()
    {
        if (!magicB) return;
        magicB.SetActive(true);
        Invoke(nameof(DeactivateMagicB), magicLifetime);
    }

    void DeactivateMagicA() { if (magicA) magicA.SetActive(false); }
    void DeactivateMagicB() { if (magicB) magicB.SetActive(false); }

    void Update()
    {
        if (!target || isDying) return;

        float d = Vector3.Distance(transform.position, target.position);
        active = d <= detectRadius || active;

        if (!active)
        {
            GoTo(startPos);
            DriveAnim();
            return;
        }

        if (hpPct <= 0f)
        {
            StartDeath();
            return;
        }

        if (d > stopDistance + 0.5f)
        {
            GoTo(target.position);
        }
        else
        {
            OrbitAround(target.position);

            if (Time.time >= nextComboAt)
            {
                nextComboAt = Time.time + comboCooldown;
                int chosen = ChooseComboByRules();
                anim.ResetTrigger(Combo1);
                anim.ResetTrigger(Combo2);
                anim.SetTrigger(chosen == 1 ? Combo1 : Combo2);
            }
        }

        FaceTarget(target.position);
        DriveAnim();
    }

    void StartDeath()
    {
        if (isDying) return;
        isDying = true;

        agent.ResetPath();
        agent.isStopped = true;
        anim.ResetTrigger(Combo1);
        anim.ResetTrigger(Combo2);
        anim.SetTrigger(Death);

        Invoke(nameof(DestroyAfterDeath), 6f);
    }

    void DestroyAfterDeath()
    {
        Destroy(gameObject);
    }

    int ChooseComboByRules()
    {
        if (!chooseComboByHP)
            return (Random.value < 0.5f) ? 1 : 2;

        if (hpPct > combo2BelowHpPct) return 1;
        if (hpPct < combo2BelowHpPct) return 2;

        if (alternateWhenBothValid)
        {
            _nextComboIs1 = !_nextComboIs1;
            return _nextComboIs1 ? 1 : 2;
        }
        return 2;
    }

    void GoTo(Vector3 pos) => agent.SetDestination(pos);

    void OrbitAround(Vector3 pivot)
    {
        Vector3 to = (transform.position - pivot).normalized;
        Vector3 tang = new Vector3(-to.z, 0, to.x);
        Vector3 p = pivot + to * stopDistance + tang * Mathf.Sin(Time.time * orbitSpeed) * orbitAmplitude;
        agent.SetDestination(p);
    }

    void FaceTarget(Vector3 pos)
    {
        var dir = pos - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            var rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }
    }

    void DriveAnim()
    {
        Vector3 local = transform.InverseTransformDirection(agent.desiredVelocity);
        anim.SetFloat(MoveX, Mathf.Clamp(local.x, -1, 1), 0.15f, Time.deltaTime);
        anim.SetFloat(MoveY, Mathf.Clamp(local.z, -1, 1), 0.15f, Time.deltaTime);
        anim.SetFloat(Speed, agent.velocity.magnitude);
    }
}
