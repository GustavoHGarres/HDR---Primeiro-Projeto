using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EmblemPickup : MonoBehaviour
{
    public string sceneName = "SCN_Fase1";
    public int statueIndex = 0; // com qual estátua se relaciona
    public string emblemId => $"emblem_{sceneName}_{statueIndex}";

    private void Reset() { GetComponent<Collider>().isTrigger = true; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // adiciona no inventário
        InventoryManager.Instance.AddEmblem(emblemId);

        // salva
        PlayerPrefs.SetInt(emblemId, 1);
        PlayerPrefs.Save();

        // procura a estátua na cena e manda completar
        var statue = FindObjectOfType<StatueController>();
        if (statue != null && statue.sceneName == sceneName && statue.statueIndex == statueIndex)
            statue.UnlockAllParts();

        // feedback e destrói
        Destroy(gameObject);
    }
}
