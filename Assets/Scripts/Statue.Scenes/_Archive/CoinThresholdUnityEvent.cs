using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CoinThresholdUnityEvent : MonoBehaviour
{
    [Header("Fonte / chave")]
    [Tooltip("Se vazio usa a cena ativa. Deve bater com o sceneName usado pelas moedas.")]
    public string sceneName = "";

    [Header("Condição")]
    [Min(0)] public int requiredCoins = 10;
    public bool once = true;                 // dispara uma vez só

    public int total = 2;

    [Header("Evento")]
    public UnityEvent OnReached;              // arraste aqui SetActive(true), Enable, etc.

    [Header("Debug / Teste")]
    public bool resetOnPlay = false;
    public bool debugLogs = false;

    string SceneKey => string.IsNullOrEmpty(sceneName)
        ? SceneManager.GetActiveScene().name
        : sceneName;

    string CoinsKey => $"coins_{SceneKey}";

    bool _fired = false;

    void Awake()
    {
        if (resetOnPlay) { PlayerPrefs.DeleteKey(CoinsKey); PlayerPrefs.Save(); }
    }

    void OnEnable()
    {
        CoinEvents.OnCoinCollected += OnCoinCollected;
        // confere estado atual (ex.: voltou pra cena já com progresso salvo)
        CheckAndMaybeInvoke();
    }

    void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
    }

    void OnCoinCollected(string scene, int _)
    {
        if (scene != SceneKey) return;
        CheckAndMaybeInvoke();
    }

    [ContextMenu("Check Now")]
    public void CheckAndMaybeInvoke()
    {
        int total = PlayerPrefs.GetInt(CoinsKey, 0);
        if (debugLogs) Debug.Log($"[CoinThresholdUnityEvent] {CoinsKey} = {total}/{requiredCoins}");

        if (total>= requiredCoins)
        {
            if (once && _fired) return;
            _fired = true;
            if (debugLogs) Debug.Log("[CoinThresholdUnityEvent] Threshold reached -> Invoke");
            OnReached?.Invoke();
        }
    }

    [ContextMenu("Reset Coins Key")]
    public void ResetCoinsKey()
    {
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
        _fired = false;
        if (debugLogs) Debug.Log($"[CoinThresholdUnityEvent] Reset {CoinsKey}");
    }
}
