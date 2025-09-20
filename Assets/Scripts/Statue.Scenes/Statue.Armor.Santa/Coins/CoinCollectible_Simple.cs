// CoinCollectible_Simple.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinCollectible_Simple : MonoBehaviour
{
    public string playerTag = "Player";
    public int value = 1;
    public ParticleSystem pickupVFX;
    public AudioClip pickupSfx;
    public bool destroyOnPickup = true;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        CoinsCounter.Add(value);

        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (destroyOnPickup) Destroy(gameObject); else gameObject.SetActive(false);
    }
}
