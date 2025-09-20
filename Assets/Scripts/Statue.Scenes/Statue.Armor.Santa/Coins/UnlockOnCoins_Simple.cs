// UnlockOnCoins_Simple.cs
using UnityEngine;
using UnityEngine.Events;

public class UnlockOnCoins_Simple : MonoBehaviour
{
    [Min(0)] public int requiredCoins = 10;
    public bool once = true;

    [Header("Debug/Teste")]
    public bool resetOnPlay = false;    // zera no início (ótimo pra testar)
    public bool debugLogs = false;

    public UnityEvent OnReached;

    bool fired = false;

    void Awake()
    {
        if (resetOnPlay)
            CoinsCounter.Reset();
    }

    void OnEnable()
    {
        CoinsCounter.OnChanged += Handle;
        // Checa imediatamente o valor salvo (caso já esteja >= alvo)
        // e também cobre o caso de OnChanged não ter disparado ainda.
        Handle(CoinsCounter.Total);
    }

    void OnDisable()
    {
        CoinsCounter.OnChanged -= Handle;
    }

    void Handle(int total)
    {
        if (fired && once) return;

        if (debugLogs) Debug.Log($"[UnlockOnCoins] {total}/{requiredCoins}");

        if (total >= requiredCoins)
        {
            fired = true;
            if (debugLogs) Debug.Log("[UnlockOnCoins] threshold atingido → Invoke");
            OnReached?.Invoke();
        }
    }

    // Botões de teste
    [ContextMenu("Debug: Reset Coins")]
    void DebugReset() => CoinsCounter.Reset();

    [ContextMenu("Debug: Add 1")]
    void DebugAdd() => CoinsCounter.Add(1);
}
