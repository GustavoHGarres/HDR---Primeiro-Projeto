using UnityEngine;

public class StatueController : MonoBehaviour
{
    [Header("Identificação da Estátua")]
    public string sceneName = "SCN_Gameplay";
    public int statueIndex = 0; // id único da estátua na cena

    [Header("Peças da Armadura (ordem: braços, corpo, pernas, etc.)")]
    public GameObject[] armorParts;

    private int currentPartsActive = 0;
    private string PartsKey => $"statue_{sceneName}_{statueIndex}_parts";

    void Start()
    {
        #if UNITY_EDITOR
        PlayerPrefs.DeleteKey(PartsKey);
        #endif

        currentPartsActive = PlayerPrefs.GetInt(PartsKey, 0);
        ApplyPartsState();
    }

    // Ativa a próxima peça da sequência (0..N-1)
    public void ActivateNextPart()
    {
        ActivatePart(currentPartsActive);
    }

    // Ativa uma peça específica pelo índice (0=Arms,1=Body,2=Legs...)
    public void ActivatePart(int partIndex)
    {
        if (armorParts == null || armorParts.Length == 0) return;
        if (partIndex < 0 || partIndex >= armorParts.Length) return;

        // liga a peça se ainda não estiver ativa
        if (armorParts[partIndex] && !armorParts[partIndex].activeSelf)
            armorParts[partIndex].SetActive(true);

        // atualiza o "progresso" como o maior índice + 1 já ativo
        int maxIdx = -1;
        for (int i = 0; i < armorParts.Length; i++)
            if (armorParts[i] && armorParts[i].activeSelf) maxIdx = i;

        currentPartsActive = Mathf.Max(currentPartsActive, maxIdx + 1);
        Save();
    }

    public void UnlockAllParts()
    {
        if (armorParts == null) return;
        foreach (var p in armorParts)
            if (p) p.SetActive(true);

        currentPartsActive = armorParts.Length;
        Save();
    }

    void ApplyPartsState()
    {
        if (armorParts == null) return;
        for (int i = 0; i < armorParts.Length; i++)
            if (armorParts[i]) armorParts[i].SetActive(i < currentPartsActive);
    }

    void Save()
    {
        PlayerPrefs.SetInt(PartsKey, currentPartsActive);
        PlayerPrefs.Save();
    }

    #if UNITY_EDITOR
    [ContextMenu("🧹 Resetar progresso desta estátua")]
    void DebugResetStatue()
    {
        PlayerPrefs.DeleteKey(PartsKey);
        PlayerPrefs.Save();
        currentPartsActive = 0;
        ApplyPartsState();
        Debug.Log($"[StatueController] Resetou progresso da estátua ({PartsKey})");
    }
#endif

}
