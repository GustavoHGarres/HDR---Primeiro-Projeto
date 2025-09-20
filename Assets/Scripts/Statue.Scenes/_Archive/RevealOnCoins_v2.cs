using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RevealOnCoins_v2 : MonoBehaviour
{
    [Header("Fonte da contagem")]
    [Tooltip("Se vazio, usa a cena ativa (coins_{Cena}).")]
    public string sceneName = "";
    [Tooltip("Se preenchido, ignora sceneName e usa essa chave diretamente (ex.: coins_SCN_Fase1 ou coins_global).")]
    public string prefsKeyOverride = "";

    [Header("Condição")]
    [Min(0)] public int requiredCoins = 2;

    [Header("Alvo")]
    [Tooltip("Objeto que será ativado quando atingir a meta. Se vazio, usa este GameObject.")]
    public GameObject target;
    public bool hideOnStart = true;

    [Header("Debug / Testes")]
    public bool resetCoinsOnPlay = false;
    public bool debugLogs = false;
    [Tooltip("Intervalo para checar PlayerPrefs mesmo sem eventos.")]
    public float pollInterval = 0.25f;

    private int _lastTotal = -1;
    private string Key => !string.IsNullOrEmpty(prefsKeyOverride)
        ? prefsKeyOverride
        : $"coins_{(string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName)}";

    private void Awake()
    {
        if (target == null) target = gameObject;
        if (hideOnStart && target) target.SetActive(false);
        if (resetCoinsOnPlay)
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            if (debugLogs) Debug.Log($"[RevealOnCoins] reset {Key}");
        }
    }

    private void OnEnable()
    {
        // assina o evento (se suas moedas o disparam)
        CoinEvents.OnCoinCollected += OnCoinCollected;
        // aplica estado atual e inicia polling
        Refresh();
        StartCoroutine(PollRoutine());
    }

    private void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
        StopAllCoroutines();
    }

    private IEnumerator PollRoutine()
    {
        var wait = new WaitForSeconds(pollInterval);
        while (true)
        {
            Refresh();
            yield return wait;
        }
    }

    private void OnCoinCollected(string scene, int total)
    {
        // se estiver usando override, não filtramos por cena; senão garantimos a cena correta
        if (string.IsNullOrEmpty(prefsKeyOverride))
        {
            var s = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            if (scene != s) return;
        }
        Apply(total);
    }

    [ContextMenu("Refresh (Read PlayerPrefs)")]
    public void Refresh()
    {
        int total = PlayerPrefs.GetInt(Key, 0);
        if (total != _lastTotal && debugLogs) Debug.Log($"[RevealOnCoins] {Key} = {total}/{requiredCoins}");
        Apply(total);
    }

    private void Apply(int total)
    {
        _lastTotal = total;
        if (!target) return;

        if (total >= requiredCoins)
            target.SetActive(true);
        else if (hideOnStart)
            target.SetActive(false);
    }
}
