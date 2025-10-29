using UnityEngine;

[RequireComponent(typeof(CombatHealth))]
public class EnemyDropItemOnDeath : MonoBehaviour
{
    [Header("Item de recompensa")]
    public GameObject itemPrefab;          // arraste o prefab do item a dropar
    public Transform dropPoint;            // onde vai aparecer (geralmente nos pés)
    public float dropDelay = 0.5f;         // segundos após morrer

    [Header("Efeitos (opcionais)")]
    public bool alignWithGround = true;    // ajusta o item pro chão
    public Vector3 offset = Vector3.up * 0.2f;

    [Header("Som e partículas")]
    public AudioClip dropSound;
    public ParticleSystem dropVfx;

    CombatHealth _health;
    bool _dropped;

    void Awake()
    {
        _health = GetComponent<CombatHealth>();
        _health.OnDeath.AddListener(OnDeath);
    }

    void OnDeath()
    {
        if (_dropped || !itemPrefab) return;
        _dropped = true;
        Invoke(nameof(SpawnItem), dropDelay);
    }

    void SpawnItem()
    {
        Transform origin = dropPoint ? dropPoint : transform;
        Vector3 pos = origin.position + offset;

        if (alignWithGround && Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, 3f))
            pos = hit.point + offset;

        GameObject go = Instantiate(itemPrefab, pos, Quaternion.identity);

        if (dropVfx)
        {
            Instantiate(dropVfx, pos, Quaternion.identity);
        }

        if (dropSound)
        {
            AudioSource.PlayClipAtPoint(dropSound, pos);
        }
    }
}
