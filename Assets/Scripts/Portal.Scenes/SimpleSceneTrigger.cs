using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[RequireComponent(typeof(Collider))]
public class SimpleSceneTrigger : MonoBehaviour
{
    public enum Mode { ByBuildIndex, BySceneName }
    [Header("Modo")]
    public Mode mode = Mode.ByBuildIndex;

    [Header("ByBuildIndex")]
    public int offset = 1;               // +1 = próxima cena, -1 = anterior

    [Header("BySceneName")]
    public string sceneName;             // usado se Mode = BySceneName

    [Header("Opcional: EntryPoint")]
    public bool useEntryPoint = false;   // se quiser spawnar num EntryPoint específico
    public string targetEntryId = "fromPrev";

    [Header("Geral")]
    public string playerTag = "Player";

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Descobre o nome da cena alvo
        string target = sceneName;

        if (mode == Mode.ByBuildIndex)
        {
            int idx = SceneManager.GetActiveScene().buildIndex + offset;
            if (idx < 0 || idx >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogWarning($"[SimpleSceneTrigger] buildIndex {idx} inválido. Confira o Build Settings.");
                return;
            }
            string path = SceneUtility.GetScenePathByBuildIndex(idx);
            target = Path.GetFileNameWithoutExtension(path);
        }

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("[SimpleSceneTrigger] Cena alvo vazia.");
            return;
        }

       var gm = GameManager.Instance;
if (gm != null)
{
    if (useEntryPoint && !string.IsNullOrEmpty(targetEntryId))
        gm.TrocarCena(target, targetEntryId);
    else
        gm.TravelTo(target, null); // não grava pp_target_entry_id
}
else
{
    Debug.LogWarning("[SimpleSceneTrigger] GameManager.Instance == null. Usando SceneManager.LoadScene.");
    UnityEngine.SceneManagement.SceneManager.LoadScene(target);
}

    }
}
