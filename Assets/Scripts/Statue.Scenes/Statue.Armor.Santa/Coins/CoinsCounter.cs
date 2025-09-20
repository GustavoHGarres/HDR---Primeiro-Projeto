// CoinsCounter.cs
using System;
using UnityEngine;

public static class CoinsCounter
{
    public const string Key = "coins_global";

    public static int Total { get; private set; }
    public static event Action<int> OnChanged;

    // Garante que sempre carregamos ao iniciar o jogo
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoLoad()
    {
        Load();
    }

    public static void Load()
    {
        Total = PlayerPrefs.GetInt(Key, 0);
        OnChanged?.Invoke(Total);
    }

    public static void Add(int value = 1)
    {
        Total += Mathf.Max(1, value);
        PlayerPrefs.SetInt(Key, Total);
        PlayerPrefs.Save();
        OnChanged?.Invoke(Total);
    }

    public static void Reset()
    {
        Total = 0;
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        OnChanged?.Invoke(Total);
    }
}
