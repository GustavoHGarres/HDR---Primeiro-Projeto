using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CoinCollectible_Simple : MonoBehaviour
{
    // 🔔 BUS interno (sem CoinEvents)
    // Notifica: (sceneName, counterKey, totalAtual)
    public static event Action<string, string, int> OnAnyCounterChanged;

    [Header("Coleta")]
    public string playerTag = "Player";
    public int value = 1;

    [Tooltip("Canal/Grupo deste conjunto de esferas: ARMS / CHEST / LEGS / etc.")]
    public string counterKey = "GLOBAL";

    [Tooltip("Se vazio, usa a cena atual")]
    public string sceneName = "";

    [Header("FX")]
    public ParticleSystem pickupVFX;
    public AudioClip pickupSfx;
    public bool destroyOnPickup = true;

    string SceneResolved => string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
    string PrefsKey => $"coins_{SceneResolved}_{counterKey}";

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        int total = PlayerPrefs.GetInt(PrefsKey, 0) + Mathf.Max(1, value);
        PlayerPrefs.SetInt(PrefsKey, total);
        PlayerPrefs.Save();

        // avisa TODOS os ouvintes desse canal
        OnAnyCounterChanged?.Invoke(SceneResolved, counterKey, total);

        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    // Utilitário opcional para testes no Inspector
    [ContextMenu("Debug: Add 1")]
    void DebugAddOne()
    {
        int total = PlayerPrefs.GetInt(PrefsKey, 0) + 1;
        PlayerPrefs.SetInt(PrefsKey, total);
        PlayerPrefs.Save();
        OnAnyCounterChanged?.Invoke(SceneResolved, counterKey, total);
    }
}
