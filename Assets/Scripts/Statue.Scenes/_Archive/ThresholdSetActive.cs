using UnityEngine;
using UnityEngine.SceneManagement;

public class ThresholdSetActive : MonoBehaviour
{
    [Header("Chave / Origem")]
    [Tooltip("Filtra pelo nome da cena enviada pelo CoinCollectible. Se vazio, usa a cena ativa.")]
    public string sceneName = "";

    [Header("Condição")]
    [Min(0)] public int requiredCoins = 2;
    public bool oneShot = true;         // ativa uma vez só

    [Header("Alvo")]
    [Tooltip("Se vazio, usa este próprio GameObject.")]
    public GameObject target;
    public bool hideOnStart = true;

    private bool _alreadyFired = false;

    private string SceneFilter =>
        string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;

    private void Awake()
    {
        if (target == null) target = gameObject;
        if (hideOnStart && target) target.SetActive(false);
    }

    private void OnEnable()
    {
        CoinEvents.OnCoinCollected += OnCoinCollected;
        // confere PlayerPrefs para estado inicial (ex: voltou à cena)
        int total = PlayerPrefs.GetInt($"coins_{SceneFilter}", 0);
        TryApply(total);
    }

    private void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
    }

    private void OnCoinCollected(string scene, int total)
    {
        if (scene != SceneFilter) return;
        TryApply(total);
    }

    private void TryApply(int total)
    {
        if (total >= requiredCoins)
        {
            if (target && !target.activeSelf) target.SetActive(true);
            if (oneShot) _alreadyFired = true;
        }
        else if (hideOnStart && !_alreadyFired)
        {
            if (target && target.activeSelf) target.SetActive(false);
        }
    }
}
