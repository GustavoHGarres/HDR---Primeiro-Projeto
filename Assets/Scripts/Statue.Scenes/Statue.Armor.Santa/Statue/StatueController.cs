using UnityEngine;

public class StatueController : MonoBehaviour
{
    public string sceneName = "SCN_Fase1";
    public int statueIndex = 0; // id único por estátua/phase
    public GameObject[] armorParts; // ordem: pernas, corpo, braços, etc.
    private int currentPartsActive = 0;
    private string PartsKey => $"statue_{sceneName}_{statueIndex}_parts";

    private void Start()
    {
        currentPartsActive = PlayerPrefs.GetInt(PartsKey, 0);
        ApplyPartsState();
    }

    // Ativa a próxima parte (usado quando coleta moeda)
    public void ActivateNextPart()
    {
        if (currentPartsActive >= armorParts.Length) return;
        armorParts[currentPartsActive].SetActive(true);
        currentPartsActive++;
        PlayerPrefs.SetInt(PartsKey, currentPartsActive);
        PlayerPrefs.Save();
    }

    // Completa todas (usado quando pega emblema)
    public void UnlockAllParts()
    {
        for (int i = 0; i < armorParts.Length; i++)
        {
            armorParts[i].SetActive(true);
        }
        currentPartsActive = armorParts.Length;
        PlayerPrefs.SetInt(PartsKey, currentPartsActive);
        PlayerPrefs.Save();
    }

    void ApplyPartsState()
    {
        for (int i = 0; i < armorParts.Length; i++)
            armorParts[i].SetActive(i < currentPartsActive);
    }

    // Para o protótipo: registrar a moeda coletada diretamente ativa a parte
    private void OnEnable()
    {
        CoinEvents.OnCoinCollected += OnCoinCollected;
    }

    private void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
    }

    private void OnCoinCollected(string scene, int total)
    {
        if (scene != sceneName) return;
        // método simples: cada vez que uma moeda é coletada, ativa a próxima parte
        // no protótipo, ajuste a lógica (ex.: 3 moedas = 1 parte) conforme quiser
        ActivateNextPart();
    }
}
//