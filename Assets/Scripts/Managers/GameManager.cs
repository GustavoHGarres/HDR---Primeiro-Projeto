using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Ebac.Core.Singleton;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { BOOT, TITLE, MENU, GAMEPLAY, LOADING }
    public GameState Current { get; private set; } = GameState.BOOT;

    [Header("Fade (opcional)")]
    [SerializeField] private CanvasGroup fade;          // arraste um CanvasGroup (FadeCanvas) se quiser
    [SerializeField] private float fadeTime = .35f;

    [Header("Travel/Respawn")]
    [SerializeField] private string playerTag = "Player";
    private const string PP_TargetEntryId = "pp_target_entry_id";
    private const string PP_LastScene     = "pp_last_scene";

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;      // posiciona no EntryPoint e salva última cena
        StartCoroutine(BootFlow());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ---- Fluxo inicial / navegação básica ----
    private IEnumerator BootFlow()
    {
        yield return null; // deixa a cena estabilizar
        YieldTo(GameState.TITLE);
        LoadSceneByName("SCN_Title");
    }

    public void GoToMenu()
    {
        YieldTo(GameState.MENU);
        LoadSceneByName("SCN_Menu");
    }

    public void StartNewGame()
    {
        YieldTo(GameState.LOADING);
        // se quiser já nascer num ponto específico do prólogo/gameplay:
        SetNextEntryPoint("fromMenu");
        LoadSceneByName("SCN_Gameplay", GameState.GAMEPLAY);
    }

    public void ContinueGame()
    {
        var scene = PlayerPrefs.GetString(PP_LastScene, "SCN_Gameplay");
        YieldTo(GameState.LOADING);
        LoadSceneByName(scene, GameState.GAMEPLAY);
    }

    public void BackToTitle()
    {
        YieldTo(GameState.TITLE);
        LoadSceneByName("SCN_Title");
    }

    private void YieldTo(GameState next) => Current = next;

    // ---- Carregamento com fade ----
    private void LoadSceneByName(string sceneName, GameState? stateAfterLoad = null)
    {
        StartCoroutine(LoadRoutine(sceneName, stateAfterLoad));
    }

    private IEnumerator LoadRoutine(string sceneName, GameState? next)
    {
        if (fade != null) yield return Fade(1f);

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // Reposicionamento acontece no OnSceneLoaded

        if (fade != null) yield return Fade(0f);

        if (next.HasValue) Current = next.Value;
    }

    private IEnumerator Fade(float target)
    {
        if (fade == null) yield break;
        fade.blocksRaycasts = true;
        float t = 0f;
        float start = fade.alpha;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        fade.alpha = target;
        fade.blocksRaycasts = target > 0.001f;
    }

    // ---- Travel / EntryPoint ----
    /// <summary>Define o EntryPoint de spawn para a próxima cena.</summary>
    public void SetNextEntryPoint(string entryId)
    {
        PlayerPrefs.SetString(PP_TargetEntryId, entryId);
        PlayerPrefs.Save();
    }

    /// <summary>Viaja para outra cena. Se targetEntryId for null ou vazio, não usa EntryPoint.</summary>
    public void TravelTo(string targetScene, string targetEntryId = null, GameState stateAfter = GameState.GAMEPLAY)
    {
        YieldTo(GameState.LOADING);
        if (!string.IsNullOrEmpty(targetEntryId))
            SetNextEntryPoint(targetEntryId);

        LoadSceneByName(targetScene, stateAfter);
    }

    /// <summary>Compatibilidade com scripts antigos (PortalTransition): chama TravelTo.</summary>
    public void TrocarCena(string targetScene, string targetEntryId)
    {
        TravelTo(targetScene, targetEntryId);
    }

    // Salva última cena e teleporta (seguro) para o EntryPoint definido
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1) Salvar para Continue (evita salvar menus)
        if (scene.name != "SCN_Title" && scene.name != "SCN_Menu")
        {
            PlayerPrefs.SetString(PP_LastScene, scene.name);
            PlayerPrefs.Save();
        }

        // 2) Ler e limpar a chave do EntryPoint
        var entryId = PlayerPrefs.GetString(PP_TargetEntryId, string.Empty);
        PlayerPrefs.DeleteKey(PP_TargetEntryId);

        // 3) Se veio um id válido, teleporta com segurança
        if (!string.IsNullOrEmpty(entryId))
            StartCoroutine(TeleportPlayerToEntryPoint_Coro(entryId));

            Debug.Log($"[GM] Loaded {scene.name} | targetEntryId='{entryId}'");

            
    }

    private IEnumerator TeleportPlayerToEntryPoint_Coro(string entryId)
    {
        // Espera todos os Awake/Start terminarem para evitar que outro script sobrescreva a posição
        yield return null;
        yield return new WaitForEndOfFrame();

        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) yield break;

        var allEntryPoints = GameObject.FindObjectsOfType<EntryPoint>();
        EntryPoint target = null;
        foreach (var ep in allEntryPoints)
        {
            if (ep.EntryId == entryId) { target = ep; break; }
        }

        if (target == null)
        {
            Debug.LogWarning($"[GameManager] EntryPoint '{entryId}' não encontrado na cena '{SceneManager.GetActiveScene().name}'.");
            yield break;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);

        if (cc) cc.enabled = true;
    }
}
