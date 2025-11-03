using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
    [Header("Nome da cena do menu (UI)")]
    public string menuSceneName = "SCN_UI_Lab";

    [Header("Tecla para abrir/fechar")]
    public KeyCode toggleKey = KeyCode.M;

    bool isOpen;

    void Start()
    {
        // Se quiser resetar progressos de chest aqui, mantenha; senão, remova:
        // GameProgress.Instance?.ResetChestProgress();
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

        // Antes de abrir, aplica o salvo para o gameplay refletir corretamente
        EquipmentManager.Instance?.ApplySavedStateForAllExisting();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        // Ao fechar, também aplica o salvo (garante que o mundo fique coerente)
        EquipmentManager.Instance?.ApplySavedStateForAllExisting();

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (SceneManager.GetSceneByName(menuSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(menuSceneName);
    }
}
