using UnityEngine;

public class GameplayStarter : MonoBehaviour
{
    void Start()
    {
        int lvl = PlayerPrefs.GetInt("TARGET_LEVEL", 1);
        Debug.Log($"[Gameplay] Entrou no Level {lvl}");

        // TODO: carregar cena/segmento do level, spawns, etc.
        // Exemplo de “progrediu”:
        // SaveSystem.WriteProgress(lvl); // chame ao terminar uma missão
    }
}
