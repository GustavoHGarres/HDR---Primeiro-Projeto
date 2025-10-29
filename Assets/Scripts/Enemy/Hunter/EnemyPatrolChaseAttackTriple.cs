using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrolChaseAttackTriple : MonoBehaviour
{
    [Header("Refs")]
    public Transform[] waypoints;
    public Transform attackPoint;
    public LayerMask targetMask; // inclua a Layer do Player

    [Header("Movimento")]
    public float patrolSpeed = 2.2f;
    public float chaseSpeed  = 3.8f;
    public float waypointPause = 0.5f;
    public bool loopPatrol = true;

    [Header("Percepção")]
    public float detectionRadius = 12f;
    public float giveUpRadius    = 16f;

    [Header("Faixas de Ataque (distâncias horizontais)")]
    public float attackRadius  = 2.3f; // Attack1 (curto)
    public float attackRadius2 = 4.8f; // Attack2 (médio)
    public float attackRadius3 = 7.0f; // Attack3 (longo)

    [Header("Tempos")]
    public float attackWindup   = 0.12f;
    public float attackCooldown = 0.55f;

    [Header("Hit")]
    public float attackHitRadius = 1.6f;
    public int damage = 12;

    [Header("Animator")]
    public Animator anim;
    public string paramSpeed     = "Speed";
    public string paramIsChasing = "IsChasing";
    public string trigAttack1    = "Attack1";
    public string trigAttack2    = "Attack2";
    public string trigAttack3    = "Attack3";

    NavMeshAgent _agent;
    Transform _target;
    int _wpIndex;
    bool _attacking;
    bool _chasing;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (waypoints != null && waypoints.Length > 0 && waypoints[0])
            _agent.SetDestination(waypoints[0].position);

        _agent.acceleration = 20f;
        _agent.angularSpeed  = 720f;
        _agent.autoBraking   = true;
    }

    void Update()
    {
        if (_attacking) return;

        DetectTarget();

        if (_target)
        {
            float dist = HorizontalDist(_target.position);

            if (dist <= attackRadius3)
            {
                _agent.isStopped = true;
                StartCoroutine(AttackRoutine(dist));
            }
            else if (dist <= giveUpRadius)
            {
                ChaseTarget();
            }
            else
            {
                _target = null;
                Patrol();
            }
        }
        else
        {
            Patrol();
        }

        if (anim)
        {
            anim.SetFloat(paramSpeed, _agent.velocity.magnitude);
            anim.SetBool(paramIsChasing, _chasing);
        }
    }

    void DetectTarget()
    {
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, targetMask, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0) _target = hits[0].transform;
    }

    void Patrol()
    {
        _chasing = false;
        _agent.isStopped = false;
        _agent.speed = patrolSpeed;

        if (waypoints == null || waypoints.Length == 0) return;

        if (!_agent.pathPending && _agent.remainingDistance <= 0.3f)
        {
            _wpIndex = (_wpIndex + 1);
            if (loopPatrol) _wpIndex %= waypoints.Length;
            else _wpIndex = Mathf.Min(_wpIndex, waypoints.Length - 1);
            StartCoroutine(NextWaypointAfterPause());
        }
    }

    IEnumerator NextWaypointAfterPause()
    {
        yield return new WaitForSeconds(waypointPause);
        if (waypoints != null && waypoints.Length > 0)
            _agent.SetDestination(waypoints[_wpIndex].position);
    }

    void ChaseTarget()
    {
        _chasing = true;
        _agent.isStopped = false;
        _agent.speed = chaseSpeed;
        if (_target) _agent.SetDestination(_target.position);
    }

    IEnumerator AttackRoutine(float dist)
    {
        _attacking = true;
        _chasing = false;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        string trigger = trigAttack1;
        if      (dist > attackRadius  && dist <= attackRadius2) trigger = trigAttack2;
        else if (dist > attackRadius2 && dist <= attackRadius3) trigger = trigAttack3;

        if (anim && !string.IsNullOrEmpty(trigger)) anim.SetTrigger(trigger);

        yield return new WaitForSeconds(attackWindup);

        // dano real
        Vector3 center = attackPoint ? attackPoint.position : transform.position + transform.forward * 1f;
        var hits = Physics.OverlapSphere(center, attackHitRadius, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var dmg = h.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector3 dir = (h.transform.position - center); dir.y = 0f;
                dmg.Damage(damage, center, dir.normalized);
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        _attacking = false;
        _agent.isStopped = false;
    }

    float HorizontalDist(Vector3 other)
    {
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = other; b.y = 0f;
        return Vector3.Distance(a, b);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.gray;   Gizmos.DrawWireSphere(transform.position, giveUpRadius);
        Gizmos.color = Color.red;     Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, attackRadius2);
        Gizmos.color = Color.cyan;    Gizmos.DrawWireSphere(transform.position, attackRadius3);
        if (attackPoint)
        {
            Gizmos.color = new Color(1, .5f, 0, .7f);
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRadius);
        }
    }
#endif
}
