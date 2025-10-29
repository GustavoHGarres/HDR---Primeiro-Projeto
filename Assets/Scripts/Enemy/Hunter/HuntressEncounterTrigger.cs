using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class HuntressEncounterTrigger : MonoBehaviour
{
    public enum Mode { EnableExistingInScene, InstantiatePrefab }

    [Header("Modo")]
    public Mode mode = Mode.EnableExistingInScene;

    [Header("Hunter (uma das duas opções)")]
    public EnemyPatrolChaseAttackTriple existingHunter; // arraste a Hunter da cena (desativada)
    public GameObject hunterPrefab;                     // ou arraste um Prefab

    [Header("Spawn")]
    public Transform spawnPoint;        // onde ela deve aparecer
    public bool facePlayerOnSpawn = true;

    [Header("Animação de entrada (opcional)")]
    public bool playAppearAnimation = false;
    public string appearTrigger = "Appear";

    [Header("Vida & Reset")]
    public bool resetHealthOnSpawn = true;

    [Header("Uso do trigger")]
    public bool oneShot = true;         // dispara uma única vez
    public bool rearm = false;          // se quiser rearmar…
    public float rearmSeconds = 10f;

    bool _used;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_used && oneShot && !rearm) return;

        // aceita qualquer variante de player
        var sw = other.GetComponentInParent<PlayerSwitcherChildren>();
        bool isPlayer = sw != null || other.CompareTag("Player");
        if (!isPlayer) return;

        var player = sw ? (sw.ActiveTransform ? sw.ActiveTransform : sw.transform) : other.transform;
        StartEncounter(player);

        _used = true;
        if (oneShot && !rearm) Destroy(gameObject, 0.1f);
        else if (rearm) Invoke(nameof(Rearm), rearmSeconds);
    }

    void Rearm() => _used = false;

    void StartEncounter(Transform player)
    {
        GameObject go = null;
        EnemyPatrolChaseAttackTriple ai = null;

        if (mode == Mode.EnableExistingInScene)
        {
            if (!existingHunter)
            {
                Debug.LogWarning("[HuntressEncounterTrigger] existingHunter não atribuído.");
                return;
            }
            go = existingHunter.gameObject;
            ai = existingHunter;
        }
        else // InstantiatePrefab
        {
            if (!hunterPrefab)
            {
                Debug.LogWarning("[HuntressEncounterTrigger] hunterPrefab não atribuído.");
                return;
            }
            go = Instantiate(hunterPrefab);
            ai = go.GetComponentInChildren<EnemyPatrolChaseAttackTriple>(true);
            if (!ai) ai = go.GetComponent<EnemyPatrolChaseAttackTriple>();
        }

        if (!go || !ai)
        {
            Debug.LogWarning("[HuntressEncounterTrigger] Hunter/AI ausente.");
            return;
        }

        // posiciona & warp
        if (spawnPoint)
        {
            go.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent && agent.isOnNavMesh) agent.Warp(spawnPoint.position);
        }

        // ativa GO (se estava desativado)
        if (!go.activeSelf) go.SetActive(true);

        // reseta vida (cheio)
        if (resetHealthOnSpawn)
        {
            var hp = go.GetComponentInChildren<CombatHealth>(true);
            if (hp != null) hp.Heal(hp.maxHealth); // nosso CombatHealth faz clamp no max
        }

        // opcional: olhar para o player
        if (facePlayerOnSpawn && player)
        {
            var p = player.position;
            p.y = go.transform.position.y;
            go.transform.LookAt(p);
        }

        // opcional: anima de entrada
        if (playAppearAnimation && !string.IsNullOrEmpty(appearTrigger))
        {
            var anim = go.GetComponentInChildren<Animator>(true);
            if (anim) anim.SetTrigger(appearTrigger);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        var c = GetComponent<Collider>() as SphereCollider;
        if (c && c.isTrigger)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(c.center, c.radius);
        }

        if (spawnPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
    }
#endif
}
