using System;

public static class CoinEvents
{
    // já compatível com seu CoinCollectible atual
    public static Action<string,int> OnCoinCollected;

    public static void InvokeOnCoinCollected(string scene, int total)
        => OnCoinCollected?.Invoke(scene, total);
}
