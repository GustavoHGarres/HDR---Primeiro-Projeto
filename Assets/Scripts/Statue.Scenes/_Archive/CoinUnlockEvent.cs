using UnityEngine;
using UnityEngine.Events;

public class CoinUnlockEvent : MonoBehaviour
{
    [Header("Condição")]
    public string sceneName = "SCN_Fase1";
    public int requiredCoins = 10;

    [Header("Evento ao desbloquear")]
    public UnityEvent OnUnlocked;

    private bool alreadyUnlocked = false;

    private void OnEnable()
    {
        CoinEvents.OnCoinCollected += HandleCoinCollected;
    }

    private void OnDisable()
    {
        CoinEvents.OnCoinCollected -= HandleCoinCollected;
    }

    private void HandleCoinCollected(string scene, int totalFromEvent)
    {
        if (scene != sceneName) return;
        if (alreadyUnlocked) return;

        // 🔎 lê direto do PlayerPrefs para garantir valor atualizado
        int total = PlayerPrefs.GetInt($"coins_{sceneName}", 0);

        if (total >= requiredCoins)
        {
            Debug.Log($"[CoinUnlockEvent] Meta atingida! {total}/{requiredCoins}");
            OnUnlocked?.Invoke();
            alreadyUnlocked = true;
        }
        else
        {
            Debug.Log($"[CoinUnlockEvent] Ainda faltam moedas... {total}/{requiredCoins}");
        }
    }
}
