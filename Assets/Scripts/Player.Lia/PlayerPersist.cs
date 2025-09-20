using UnityEngine;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist _instance;

    void Awake()
    {
        // Garante 1 único Player
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);              // <- sobrevive à troca de cena

        // (opcional) garanta a tag
        if (!CompareTag("Player")) gameObject.tag = "Player";
    }
}
