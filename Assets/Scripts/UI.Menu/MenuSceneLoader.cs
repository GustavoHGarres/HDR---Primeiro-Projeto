using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    [Header("Nome da cena do menu (UI)")]
    public string menuSceneName = "SCN_UI_Lab";

    [Header("Tecla para abrir/fechar")]
    public KeyCode toggleKey = KeyCode.M;

    bool isOpen;
    AsyncOperation loadOp;

    // coloque num script da SCN_Gameplay (ex.: no _MenuLoader ou GameManager)
    void Start()
    {
         if (GameProgress.Instance != null)
         GameProgress.Instance.ResetChestProgress(); // zera: 0/3 e locked
    }


    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (!isOpen) OpenMenu();
            else CloseMenu();
        }
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;

        // pausa o jogo e mostra o cursor
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // carrega a cena do menu em modo aditivo
        loadOp = SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        // retoma o jogo e esconde o cursor (ajuste se quiser manter visível)
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // descarrega a cena do menu
        if (SceneManager.GetSceneByName(menuSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(menuSceneName);
    }
}
