// CoinDebugTools.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinDebugTools : MonoBehaviour
{
    [Tooltip("Se vazio, usa a cena ativa.")]
    public string sceneName = "";

    string Key => $"coins_{(string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName)}";

    [ContextMenu("Coins: +1")]
    public void AddOne()
    {
        int t = PlayerPrefs.GetInt(Key, 0) + 1;
        PlayerPrefs.SetInt(Key, t); PlayerPrefs.Save();
        CoinEvents.InvokeOnCoinCollected(sceneName == "" ? SceneManager.GetActiveScene().name : sceneName, t);
        Debug.Log($"[CoinDebug] {Key} = {t}");
    }

    [ContextMenu("Coins: Reset")]
    public void ResetCoins()
    {
        PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save();
        Debug.Log($"[CoinDebug] reset {Key}");
    }

    [ContextMenu("Coins: Log")]
    public void LogCoins()
    {
        Debug.Log($"[CoinDebug] {Key} = {PlayerPrefs.GetInt(Key,0)}");
    }
}
