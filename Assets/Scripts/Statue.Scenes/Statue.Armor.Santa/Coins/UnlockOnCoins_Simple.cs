using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnlockOnCoins_Simple : MonoBehaviour
{
    [Header("Filtro")]
    [Tooltip("Se vazio, usa a cena atual")]
    public string sceneName = "";

    [Tooltip("Canal/Grupo observado por este evento: ARMS / CHEST / LEGS / etc.")]
    public string counterKey = "GLOBAL";

    [Min(0)] public int requiredCoins = 3;
    public bool once = true;

    [Header("Debug/Teste")]
    public bool resetThisKeyOnPlay = false;
    public bool debugLogs = false;

    [Header("Ação ao atingir a meta")]
    public UnityEvent OnReached;

    bool fired;

    string SceneResolved => string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
    string PrefsKey => $"coins_{SceneResolved}_{counterKey}";

    void Awake()
    {
        if (resetThisKeyOnPlay)
        {
            PlayerPrefs.SetInt(PrefsKey, 0);
            PlayerPrefs.Save();
        }
    }

    void OnEnable()
    {
        CoinCollectible_Simple.OnAnyCounterChanged += Handle;
        // Checa estado atual (pode já ter salvo)
        Handle(SceneResolved, counterKey, PlayerPrefs.GetInt(PrefsKey, 0));
    }

    void OnDisable()
    {
        CoinCollectible_Simple.OnAnyCounterChanged -= Handle;
    }

    void Handle(string scene, string key, int total)
    {
        if (scene != SceneResolved) return;
        if (!string.Equals(key, counterKey)) return;
        if (fired && once) return;

        if (debugLogs) Debug.Log($"[Unlock:{counterKey}] {total}/{requiredCoins}");

        if (total >= requiredCoins)
        {
            fired = true;
            if (debugLogs) Debug.Log($"[Unlock:{counterKey}] Atingido → Invoke()");
            OnReached?.Invoke();
        }
    }

    // Utilitário opcional p/ testes
    [ContextMenu("Debug: Force Invoke")]
    void DebugForceInvoke()
    {
        OnReached?.Invoke();
        fired = true;
    }
}
