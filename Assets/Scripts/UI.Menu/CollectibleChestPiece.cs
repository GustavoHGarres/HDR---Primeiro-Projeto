using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleChestPiece : MonoBehaviour
{
    [Tooltip("Tag do player que vai coletar (ex.: Player)")]
    public string playerTag = "Player";
    public AudioSource sfxOnPickup; // opcional
    public GameObject vfxOnPickup;  // opcional

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        GameProgress.Instance?.AddChestPiece(1);

        if (sfxOnPickup) sfxOnPickup.Play();
        if (vfxOnPickup) Instantiate(vfxOnPickup, transform.position, Quaternion.identity);

        // destrói o cubo (ou desativa)
        Destroy(gameObject);
    }
}
