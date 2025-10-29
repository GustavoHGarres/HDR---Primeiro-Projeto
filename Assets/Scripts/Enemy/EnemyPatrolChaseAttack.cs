using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class EnemyPatrolChaseAttack : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("Refs")]
    public Transform[] waypoints;           // defina na ordem (ou filhos do GO)
    public Transform attackPoint;           // onde nasce o golpe (mão/peito)
    public LayerMask targetMask;            // layer do Player

    [Header("Movimento")]
    public float patrolSpeed = 2.2f;
    public float chaseSpeed  = 3.6f;
    public float waypointPause = 0.75f;
    public bool loopPatrol = true;

    [Header("Percepção & Ataque")]
    public float detectionRadius = 10f;     // entra em CHASE
    public float giveUpRadius    = 14f;     // sai de CHASE
    public float attackRadius    = 2.2f;    // entra em ATTACK
    public float attackHitRadius = 2.0f;    // raio do acerto em torno do attackPoint
    public float attackWindup    = 0.25f;   // “preparo” do golpe (sincronize com animação)
    public float attackCooldown  = 0.9f;    // intervalo entre golpes

    [Header("Feedback de Ataque (sem vida/dano)")]
    public UnityEvent<Transform> OnHit;     // invocado para cada alvo atingido
    public bool applyKnockback = true;
    public float knockbackForce = 9f;

    [Header("Animação (opcional)")]
    public Animator animator;
    public string animParamSpeed     = "Speed";
    public string animParamChasing   = "IsChasing";
    public string animTriggerAttack  = "Attack";

    [Header("Debug")]
    public bool drawPaths = true;

    // === runtime ===
    NavMeshAgent _agent;
    Transform _target;
    int _wpIndex;
    State _state;
    bool _isAttacking;

    void Reset()
    {
        var col = GetComponent<CapsuleCollider>();
        if (col) { col.isTrigger = false; col.center = new Vector3(0,1,0); col.height = 2f; }
        if (!attackPoint) attackPoint = this.transform;
        targetMask = LayerMask.GetMask("Player"); // ajuste se precisar
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // Coleta filhos como waypoints se vazio
        if ((waypoints == null || waypoints.Length == 0) && transform.childCount > 0)
        {
            var list = new System.Collections.Generic.List<Transform>();
            foreach (Transform t in transform) if (t != transform) list.Add(t);
            waypoints = list.ToArray();
        }

        _state = State.Patrol;
        _agent.speed = patrolSpeed;

        if (waypoints != null && waypoints.Length > 0)
        {
            _wpIndex = 0;
            _agent.SetDestination(waypoints[_wpIndex].position);
        }

        StartCoroutine(PerceptionLoop());
    }

    void Update()
    {
        if (animator)
        {
            // damping suave para locomotion
            float spd = _agent.velocity.magnitude;
            animator.SetFloat(animParamSpeed, spd, 0.15f, Time.deltaTime);
            animator.SetBool(animParamChasing, _state == State.Chase);
        }

        switch (_state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase:  TickChase();  break;
            case State.Attack: break;        // controlado pela rotina
        }
    }

    IEnumerator PerceptionLoop()
    {
        var wait = new WaitForSeconds(0.15f);
        while (true) { FindTarget(); yield return wait; }
    }

    void FindTarget()
    {
        if (_target)
        {
            float dist = Vector3.Distance(transform.position, _target.position);
            if (_state == State.Chase || _state == State.Patrol)
            {
                if (dist <= attackRadius) TryStartAttack();
                else if (dist <= detectionRadius) SetState(State.Chase);
                else if (dist > giveUpRadius) { _target = null; SetState(State.Patrol); }
            }
            return;
        }

        var hits = Physics.OverlapSphere(transform.position, detectionRadius, targetMask, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0) { _target = hits[0].transform; SetState(State.Chase); }
        else if (_state != State.Attack) SetState(State.Patrol);
    }

    void TickPatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        _agent.speed = patrolSpeed;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            StartCoroutine(NextWaypointAfterPause());
    }

    IEnumerator NextWaypointAfterPause()
    {
        if (_agent.isStopped) yield break;
        _agent.isStopped = true;
        yield return new WaitForSeconds(waypointPause);

        _wpIndex++;
        if (_wpIndex >= waypoints.Length)
            _wpIndex = loopPatrol ? 0 : waypoints.Length - 1;

        _agent.isStopped = false;
        _agent.SetDestination(waypoints[_wpIndex].position);
    }

    void TickChase()
    {
        if (!_target) { SetState(State.Patrol); return; }
        _agent.speed = chaseSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(_target.position);

        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist <= attackRadius) TryStartAttack();
        else if (dist > giveUpRadius) { _target = null; SetState(State.Patrol); }
    }

    void TryStartAttack()
    {
        if (_isAttacking || !_target) return;
        SetState(State.Attack);
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _agent.isStopped = true;

        FaceTarget(_target.position);

        if (animator && !string.IsNullOrEmpty(animTriggerAttack))
            animator.SetTrigger(animTriggerAttack);

        yield return new WaitForSeconds(attackWindup);

        // “acerto” sem causar dano: evento + knockback opcional
        Vector3 center = attackPoint ? attackPoint.position : transform.position + transform.forward * 1f;
        var hits = Physics.OverlapSphere(center, attackHitRadius, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            // evento para você conectar depois (UI, som, dano, etc.)
            OnHit?.Invoke(h.transform);

            if (applyKnockback)
            {
                var rb = h.attachedRigidbody ?? h.GetComponentInParent<Rigidbody>();
                if (rb)
                {
                    Vector3 dir = (h.transform.position - center).normalized;
                    dir.y = 0f;
                    rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        _isAttacking = false;
        _agent.isStopped = false;

        if (_target)
        {
            float dist = Vector3.Distance(transform.position, _target.position);
            if (dist <= attackRadius) TryStartAttack();
            else if (dist <= detectionRadius) SetState(State.Chase);
            else { _target = null; SetState(State.Patrol); }
        }
        else SetState(State.Patrol);
    }

    void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = (worldPos - transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            var look = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 12f * Time.deltaTime);
        }
    }

    void SetState(State s)
    {
        if (_state == s) return;
        _state = s;
        switch (_state)
        {
            case State.Patrol:
                _agent.speed = patrolSpeed;
                if (waypoints != null && waypoints.Length > 0)
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(waypoints[_wpIndex].position);
                }
                break;
            case State.Chase:
                _agent.speed = chaseSpeed;
                break;
            case State.Attack:
                // rotina decide
                break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.25f);
        Vector3 ap = attackPoint ? attackPoint.position : transform.position + transform.forward * 1f;
        Gizmos.DrawWireSphere(ap, attackHitRadius);

        if (drawPaths && waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length - 1; i++)
                if (waypoints[i] && waypoints[i+1])
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            if (loopPatrol && waypoints[0] && waypoints[^1])
                Gizmos.DrawLine(waypoints[^1].position, waypoints[0].position);
        }
    }
#endif
}
