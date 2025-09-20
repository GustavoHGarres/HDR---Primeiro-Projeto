// RevealOnCoins.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class RevealOnCoins : MonoBehaviour
{
    [Header("Fonte da contagem")]
    [Tooltip("Se vazio, usa a cena ativa (coins_{Cena}).")]
    public string sceneName = "";
    [Tooltip("Se preenchido, ignora sceneName e usa esta chave diretamente.")]
    public string prefsKeyOverride = "";

    [Header("Condição")]
    [Min(0)] public int requiredCoins = 10;

    [Header("Alvo")]
    [Tooltip("Objeto a ser ativado quando atingir a meta. Se vazio, usa este GameObject.")]
    public GameObject target;
    public bool hideOnStart = true;

    [Header("Debug")]
    public bool debugLogs = false;
    public bool resetCoinsOnStart = false; // útil pra teste rápido

    string Key
    {
        get
        {
            if (!string.IsNullOrEmpty(prefsKeyOverride)) return prefsKeyOverride;
            var s = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            return $"coins_{s}";
        }
    }

    void Awake()
    {
        if (target == null) target = gameObject;
        if (hideOnStart && target) target.SetActive(false);
        if (resetCoinsOnStart) { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
    }

    void OnEnable()
    {
        CoinEvents.OnCoinCollected += OnCoinCollected;
        Refresh(); // checa já no começo (caso já tenha moedas salvas)
    }

    void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
    }

    void OnCoinCollected(string scene, int total)
    {
        // se você setou sceneName, só reage quando for a cena certa
        if (string.IsNullOrEmpty(prefsKeyOverride) && !string.IsNullOrEmpty(sceneName))
            if (scene != sceneName) return;

        if (debugLogs) Debug.Log($"[RevealOnCoins] coins={total}/{requiredCoins} -> {Key}");
        Apply(total);
    }

    [ContextMenu("Refresh (Read PlayerPrefs)")]
    public void Refresh()
    {
        int total = PlayerPrefs.GetInt(Key, 0);
        if (debugLogs) Debug.Log($"[RevealOnCoins.Refresh] {Key} = {total}");
        Apply(total);
    }

    void Apply(int total)
    {
        if (!target) return;
        if (total >= requiredCoins) target.SetActive(true);
        else if (hideOnStart) target.SetActive(false);
    }
}
