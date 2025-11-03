using System;

public static class CoinEvents
{
    // Evento principal (cena + canal + total)
    public static event Action<string, string, int> OnChanged;

    // Compatível com seu sistema antigo (sem canal)
    public static Action<string, int> OnCoinCollected;

    // Novo método com canal (ex: ARMS, CHEST, LEGS)
    public static void Invoke(string sceneName, string counterKey, int total)
    {
        OnChanged?.Invoke(sceneName, counterKey, total);
        OnCoinCollected?.Invoke(sceneName, total);
    }

    // Mantém compatibilidade com o nome antigo usado nos scripts antigos
    public static void InvokeOnCoinCollected(string sceneName, int total)
    {
        OnCoinCollected?.Invoke(sceneName, total);
        OnChanged?.Invoke(sceneName, "GLOBAL", total);
    }
}
