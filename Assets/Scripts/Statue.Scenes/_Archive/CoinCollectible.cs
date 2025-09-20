using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinCollectible : MonoBehaviour
{
    public string sceneName = "SCN_Fase1"; 
    public int value = 1; 
    public ParticleSystem pickupVFX;
    public AudioClip pickupSfx;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // grava moedas acumuladas
        var key = $"coins_{sceneName}";
        int current = PlayerPrefs.GetInt(key, 0);
        current += value;
        PlayerPrefs.SetInt(key, current);
        PlayerPrefs.Save();

        // 🔔 avisa para todo mundo que escuta
        CoinEvents.InvokeOnCoinCollected(sceneName, current);

        // feedback
        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        Destroy(gameObject);
    }
}
