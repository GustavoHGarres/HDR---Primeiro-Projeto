using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyArcherPatrol : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";

    [Header("Ranges (m)")]
    public float chaseRadius = 40f;
    public float attackB_Radius = 30f;
    public float attackA_Radius = 20f;

    [Header("Cadência")]
    public float attackA_Cooldown = 2.0f;
    public float attackB_Cooldown = 2.2f;

    [Header("Arrow")]
    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;

    [Header("Patrulha")]
    public Transform[] waypoints;
    public float waypointTolerance = 0.5f;

    [Header("Movimento/Estabilidade")]
    public float turnSpeed = 7f;
    public float stopWhileAttackingTime = 0.6f;
    public float hysteresis = 0.6f;

    [Header("Stopping Distance")]
    public float patrolStopping = 0.2f;
    public float chaseStopping  = 18f;

    // Animator params
    static readonly int SpeedHash   = Animator.StringToHash("Speed");
    static readonly int AttackAHash = Animator.StringToHash("AttackA");
    static readonly int AttackBHash = Animator.StringToHash("AttackB");

    Animator anim;
    NavMeshAgent agent;

    // 🔒 trava o controller correto
    RuntimeAnimatorController lockedController;

    // flags de existência (se algum controller “cru” entrar, evitamos erros)
    bool hasSpeed, hasAttackA, hasAttackB;

    Transform target;
    Collider[] myCols;

    Vector3 startPos;
    int wp;
    float lastA, lastB, unstopAt;

    enum State { Patrol, Chase, AttackB, AttackA }
    State state = State.Patrol;

    void Awake()
    {
        anim  = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        myCols = GetComponentsInChildren<Collider>();

        // culling off para não pausar
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.applyRootMotion = false;

        // trava controller atual
        lockedController = anim.runtimeAnimatorController;

        // descobre se os parâmetros existem
        foreach (var p in anim.parameters)
        {
            if (p.nameHash == SpeedHash)   hasSpeed = true;
            if (p.nameHash == AttackAHash) hasAttackA = true;
            if (p.nameHash == AttackBHash) hasAttackB = true;
        }

        agent.updateRotation = true;
        agent.autoBraking = true;
        agent.speed = Mathf.Max(agent.speed, 3.2f);
        agent.acceleration = Mathf.Max(agent.acceleration, 12f);
        agent.stoppingDistance = patrolStopping;

        // fallback se waypoints não forem atribuídos (evita UnassignedReference)
        if (waypoints == null) waypoints = new Transform[0];
    }

    void Start()
    {
        startPos = transform.position;
        BuscarPlayer();
        EnterPatrol();
    }

    void Update()
    {
        // 🔒 se algum script trocar o controller, recolocamos o certo
        if (anim.runtimeAnimatorController != lockedController)
        {
            anim.runtimeAnimatorController = lockedController;
            anim.Rebind(); // ressincroniza
            Debug.LogWarning("[ARCHER] Controller foi trocado em runtime — restaurei o correto.");
        }

        if (!target) BuscarPlayer();
        if (!target) return;

        if (agent.isStopped && Time.time >= unstopAt)
            agent.isStopped = false;

        float d = Vector3.Distance(transform.position, target.position);

        switch (state)
        {
            case State.Patrol:
                TickPatrol();
                if (d <= chaseRadius - hysteresis) EnterChase();
                break;

            case State.Chase:
                TickChase();
                if (d > chaseRadius + hysteresis) { EnterPatrol(); break; }
                if (d <= attackA_Radius - hysteresis && Time.time >= lastA + attackA_Cooldown) { EnterAttackA(); break; }
                if (d <= attackB_Radius - hysteresis && d > attackA_Radius + hysteresis
                    && Time.time >= lastB + attackB_Cooldown) { EnterAttackB(); break; }
                break;

            case State.AttackB:
                FaceTarget();
                if (Time.time >= unstopAt) DecideNextState(d);
                break;

            case State.AttackA:
                FaceTarget();
                if (Time.time >= unstopAt) DecideNextState(d);
                break;
        }

        if (hasSpeed) anim.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    // ===== Entradas de estado =====
    void EnterPatrol()
    {
        state = State.Patrol;
        agent.isStopped = false;
        agent.ResetPath();
        agent.stoppingDistance = patrolStopping;

        if (hasAttackA) anim.ResetTrigger(AttackAHash);
        if (hasAttackB) anim.ResetTrigger(AttackBHash);
        if (hasSpeed)   anim.SetFloat(SpeedHash, 0f);

        if (HasWPs()) agent.SetDestination(waypoints[wp].position);
        else          agent.SetDestination(startPos);

        // zera timers (garante ciclo limpo na 3ª, 4ª… vez)
        lastA = lastB = 0f;
        unstopAt = 0f;
    }

    void EnterChase()
    {
        state = State.Chase;
        agent.isStopped = false;
        agent.ResetPath();
        agent.stoppingDistance = chaseStopping;

        if (hasAttackA) anim.ResetTrigger(AttackAHash);
        if (hasAttackB) anim.ResetTrigger(AttackBHash);

        if (target) agent.SetDestination(target.position);
    }

    void EnterAttackB()
    {
        state = State.AttackB;
        lastB = Time.time;
        agent.isStopped = true;
        agent.ResetPath();
        unstopAt = Time.time + stopWhileAttackingTime;
        if (hasAttackA) anim.ResetTrigger(AttackAHash);
        if (hasAttackB) anim.SetTrigger(AttackBHash);
        FaceTarget();
    }

    void EnterAttackA()
    {
        state = State.AttackA;
        lastA = Time.time;
        agent.isStopped = true;
        agent.ResetPath();
        unstopAt = Time.time + stopWhileAttackingTime;
        if (hasAttackB) anim.ResetTrigger(AttackBHash);
        if (hasAttackA) anim.SetTrigger(AttackAHash);
        FaceTarget();
    }

    void DecideNextState(float d)
    {
        if (d > chaseRadius + hysteresis) { EnterPatrol(); }
        else if (d > attackB_Radius + hysteresis) { EnterChase(); }
        else if (d <= attackA_Radius - hysteresis && Time.time >= lastA + attackA_Cooldown) { EnterAttackA(); }
        else if (d > attackA_Radius + hysteresis && d <= attackB_Radius - hysteresis && Time.time >= lastB + attackB_Cooldown) { EnterAttackB(); }
        else { EnterChase(); }
    }

    // ===== Comportamentos =====
    void TickPatrol()
    {
        if (HasWPs())
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                wp = (wp + 1) % waypoints.Length;
                agent.SetDestination(waypoints[wp].position);
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
                agent.SetDestination(startPos);
        }
    }

    void TickChase()
    {
        if (!target) return;
        agent.isStopped = false;
        agent.SetDestination(target.position); // respeita stoppingDistance=18
        FaceTarget();
    }

    // ===== Suporte =====
    void FaceTarget()
    {
        if (!target) return;
        Vector3 dir = target.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        }
    }

    bool HasWPs() => waypoints != null && waypoints.Length > 0;

    void BuscarPlayer()
    {
        var obj = GameObject.FindGameObjectWithTag(playerTag);
        if (obj) target = obj.transform;
    }

    // Eventos de animação
    public void Anim_SpawnArrow_A1() => SpawnArrow();
    public void Anim_SpawnArrow_B1() => SpawnArrow();

    void SpawnArrow()
    {
        if (!arrowPrefab || !arrowSpawnPoint || !target) return;
        Vector3 dir = (target.position - arrowSpawnPoint.position).normalized;
        var go = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(dir, Vector3.up));
        var pa = go.GetComponent<ProjectileArrow>();
        if (pa) pa.Launch(dir, myCols);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, .65f, 0f); Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, attackB_Radius);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackA_Radius);
    }
}
