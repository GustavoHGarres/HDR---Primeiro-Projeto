using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyArcher : MonoBehaviour
{
    [Header("Referências")]
    public string playerTag = "Player";     // Tag para detectar o jogador
    public Transform arrowSpawnPoint;       // ponto de onde a flecha será instanciada
    public GameObject arrowPrefab;          // prefab da flecha (com script ProjectileArrow)

    [Header("Comportamento")]
    public float lookRadius = 30f;          // raio para olhar
    public float shootRadius = 20f;         // raio para atacar
    public float shootCooldown = 2f;        // intervalo entre tiros

    Animator anim;
    float lastShootTime;
    bool isShooting;
    bool isLooking;
    Transform target;                       // armazenará o player ativo

    Collider[] myColliders;

    void Start()
    {
        anim = GetComponent<Animator>();
        myColliders = GetComponentsInChildren<Collider>();
        BuscarPlayer(); // busca inicial
    }

    void Update()
    {
        // se o player foi trocado, tenta reencontrar
        if (!target)
        {
            BuscarPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // --- 1️⃣ Olhar para o jogador dentro do raio de detecção ---
        if (distance <= lookRadius)
        {
            isLooking = true;
            LookAtTarget();
        }
        else
        {
            isLooking = false;
        }

        // --- 2️⃣ Atacar dentro do raio de ataque ---
        if (distance <= shootRadius)
        {
            if (!isShooting && Time.time >= lastShootTime + shootCooldown)
            {
                isShooting = true;
                anim.SetTrigger("Shoot"); // Dispara animação (evento chamará o Spawn)
                lastShootTime = Time.time;
            }
        }
        else
        {
            isShooting = false;
        }
    }

    void BuscarPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void LookAtTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    // --- 3️⃣ Evento da animação chama isto ---
    public void Anim_SpawnArrow()
    {
        if (!arrowPrefab || !arrowSpawnPoint || !target) return;

        // Instancia a flecha e ignora colisões com a arqueira
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        ProjectileArrow pa = arrow.GetComponent<ProjectileArrow>();

        // Alinha a direção inicial
        Vector3 dir = (target.position - arrowSpawnPoint.position).normalized;
        //arrow.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (pa)
            pa.Launch(dir, myColliders);
    }

    // Gizmos de debug no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRadius);
    }
}
