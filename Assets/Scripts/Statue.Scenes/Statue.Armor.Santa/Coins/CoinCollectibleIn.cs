using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CoinCollectibleIn : MonoBehaviour
{
    [Tooltip("Se vazio, usa a cena ativa.")]
    public string sceneName = "";

    [Tooltip("Canal do grupo (ex.: ARMS, CHEST, LEGS). Se vazio, usa GLOBAL.")]
    public string counterKey = "GLOBAL";

    public int value = 1;
    public ParticleSystem pickupVFX;
    public AudioClip pickupSfx;
    public bool destroyOnPickup = true;

    string SceneResolved => string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
    string PrefsKey      => $"coins_{SceneResolved}_{counterKey}";

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        int total = PlayerPrefs.GetInt(PrefsKey, 0) + Mathf.Max(1, value);
        PlayerPrefs.SetInt(PrefsKey, total);
        PlayerPrefs.Save();

        CoinEvents.Invoke(SceneResolved, counterKey, total);

        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (destroyOnPickup) Destroy(gameObject); else gameObject.SetActive(false);
    }
}
