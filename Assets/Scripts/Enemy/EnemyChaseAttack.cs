using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseAttack : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Se vazio, pega o objeto com tag 'Player' no Awake.")]
    public Transform player;

    private NavMeshAgent agent;
    private Animator anim;

    [Header("Raios (m)")]
    public float detectRadius = 10f;   // perseguir
    public float attackRadius = 2.5f;  // atacar
    public float leashRadius  = 14f;   // “coleira” (volta ao spawn)

    [Header("Ataque")]
    public float attackCooldown = 1.2f;
    private float nextAttackTime;

    [Header("Rotação")]
    [Tooltip("Velocidade de giro para encarar o alvo (deg/s).")]
    public float turnSpeed = 540f;

    // internos
    private Vector3 spawnPos;
    private bool placedOnMesh;

    // animator param names (precisam existir no Animator)
    const string SpeedParam   = "Speed";     // float
    const string Attack1Trig  = "Attack_1";  // trigger
    const string Attack2Trig  = "Attack_2";  // trigger

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponentInChildren<Animator>();
        spawnPos = transform.position;

        // auto-encontra Player se não foi setado
        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        // cola no NavMesh caso tenha nascido fora
        placedOnMesh = TryPlaceOnNavMesh(spawnPos);

        // o agente controla o deslocamento, nós controlamos o giro
        agent.updateRotation = false;

        // para não atravessar o player quando aproximar para atacar
        agent.stoppingDistance = attackRadius * 0.6f;

        // começa liberado
        agent.isStopped = false;
    }

    bool TryPlaceOnNavMesh(Vector3 pos)
    {
        // tenta achar um ponto válido próximo (até 5m)
        if (NavMesh.SamplePosition(pos, out var hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);              // teleporta já “colado” na malha
            return agent.isOnNavMesh;
        }
        return false;
    }

    void Update()
    {
        if (!player) return;

        // se ainda não está no navmesh, tenta novamente
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            if (!placedOnMesh) placedOnMesh = TryPlaceOnNavMesh(transform.position);
            return;
        }

        // velocidade -> parâmetro do Animator (suavizado)
        float targetSpeed = agent.velocity.magnitude;
        float curSpeed = anim ? anim.GetFloat(SpeedParam) : 0f;
        float smooth = Mathf.Lerp(curSpeed, targetSpeed, 10f * Time.deltaTime);
        anim?.SetFloat(SpeedParam, smooth);

        float dist = Vector3.Distance(transform.position, player.position);

        // saiu da coleira → volta pro spawn
        if (dist > leashRadius)
        {
            SetDestSafe(spawnPos);
            Face(spawnPos);
            return;
        }

        // dentro do raio de detecção e fora do de ataque → perseguir
        if (dist <= detectRadius && dist > attackRadius)
        {
            SetDestSafe(player.position);
            Face(player.position);
            return;
        }

        // dentro do raio de ataque → parar, encarar e atacar com cooldown
        if (dist <= attackRadius)
        {
            agent.isStopped = true;
            Face(player.position);

            if (Time.time >= nextAttackTime)
            {
                if (Random.value < 0.5f) anim?.SetTrigger(Attack1Trig);
                else                     anim?.SetTrigger(Attack2Trig);

                nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        // idle (dentro da coleira, fora de detecção) → volta pro spawn
        SetDestSafe(spawnPos);
        Face(spawnPos);
    }

    /// <summary>
    /// Define destino apenas quando for significativamente diferente,
    /// evita spam de SetDestination por frame.
    /// </summary>
    void SetDestSafe(Vector3 dest)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // só reemite se destino mudou “de verdade”
        if ((agent.destination - dest).sqrMagnitude > 0.1f)
            agent.SetDestination(dest);

        agent.isStopped = false;
    }

    /// <summary>Gira suavemente para olhar para worldPoint.</summary>
    void Face(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            target,
            turnSpeed * Time.deltaTime
        );
    }

    // ----- Hook de dano sincronizado com Animation Event (opcional) -----
    // Crie um evento no clipe (no frame do impacto) chamando este método.
    public void OnAttackHitEvent()
    {
        // Exemplo: checar se player está no raio e aplicar dano
        // if (Vector3.Distance(transform.position, player.position) <= attackRadius + 0.5f) { ... }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;   Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, leashRadius);
    }
#endif
}
