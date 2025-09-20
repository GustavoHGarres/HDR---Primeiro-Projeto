using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnClickPlay()     => GameManager.Instance.StartNewGame();
    public void OnClickContinue() => GameManager.Instance.ContinueGame();
    public void OnClickLoad()     { /* abrir tela de loads futuramente */ }
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
