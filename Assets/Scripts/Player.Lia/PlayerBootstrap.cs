using UnityEngine;

public class PlayerBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // arraste seu prefab aqui

    void Start()
    {
        // Se não há Player na cena nem em DontDestroyOnLoad, cria um
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null && playerPrefab != null)
        {
            var go = Instantiate(playerPrefab);
            if (!go.CompareTag("Player")) go.tag = "Player";

            // já garante persistência
            if (go.GetComponent<PlayerPersist>() == null)
                go.AddComponent<PlayerPersist>();
        }
    }
}
