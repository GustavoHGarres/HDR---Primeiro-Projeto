// CoinCollectible.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CoinCollectibleIn : MonoBehaviour
{
    [Tooltip("Se vazio, usa a cena ativa.")]
    public string sceneName = "";
    public int value = 1;
    public ParticleSystem pickupVFX;
    public AudioClip pickupSfx;
    public bool destroyOnPickup = true;

    string Key
    {
        get
        {
            var s = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            return $"coins_{s}";
        }
    }

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        int total = PlayerPrefs.GetInt(Key, 0) + Mathf.Max(1, value);
        PlayerPrefs.SetInt(Key, total);
        PlayerPrefs.Save();

        // avisa watchers
        var s = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
        CoinEvents.InvokeOnCoinCollected(s, total);

        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (destroyOnPickup) Destroy(gameObject); else gameObject.SetActive(false);
    }
}
