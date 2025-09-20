using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChestController : MonoBehaviour
{
    public string sceneName = "SCN_Fase1";
    public int requiredCoins = 3; // prototype
    public GameObject emblemPrefab; // spawn do emblema quando abrir
    public Transform emblemSpawnPoint;
    public Animator animator; // opcional, anim chest open
    public bool autoCheckOnStart = true;

    private bool opened = false;

    private void Reset() { GetComponent<Collider>().isTrigger = true; }

    private void OnEnable()
    {
        CoinEvents.OnCoinCollected += OnCoinCollected;
    }

    private void OnDisable()
    {
        CoinEvents.OnCoinCollected -= OnCoinCollected;
    }

    private void Start()
    {
        if (autoCheckOnStart) CheckOpenable();
    }

    void OnCoinCollected(string scene, int total)
    {
        if (scene != sceneName) return;
        CheckOpenable();
    }

    void CheckOpenable()
    {
        if (opened) return;
        int total = PlayerPrefs.GetInt($"coins_{sceneName}", 0);
        if (total >= requiredCoins)
        {
            // anim/efeito de estar "aberto" (pode mostrar ícone)
            if (animator) animator.SetBool("Openable", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TryOpen();
    }

    public void TryOpen()
    {
        if (opened) return;
        int total = PlayerPrefs.GetInt($"coins_{sceneName}", 0);
        if (total < requiredCoins)
        {
            Debug.Log("[Chest] Ainda precisa de mais moedas!");
            return;
        }

        OpenChest();
    }

    void OpenChest()
    {
        opened = true;
        if (animator) animator.SetTrigger("Open");
        // spawn emblem
        if (emblemPrefab)
        {
            var spawnPos = emblemSpawnPoint ? emblemSpawnPoint.position : transform.position + Vector3.up * 1f;
            Instantiate(emblemPrefab, spawnPos, Quaternion.identity);
        }
        // opcional: marcar baú aberto pra não respawnar (PlayerPrefs)
        PlayerPrefs.SetInt($"chest_opened_{sceneName}_{transform.GetInstanceID()}", 1);
        PlayerPrefs.Save();
    }
}
