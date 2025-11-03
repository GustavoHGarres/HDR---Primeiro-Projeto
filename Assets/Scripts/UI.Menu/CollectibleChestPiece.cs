using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class CollectibleChestPiece : MonoBehaviour
{
    [Header("Coleta")]
    public string playerTag = "Player";
    public bool destroyOnPickup = true;

    [Header("Ação na Estátua")]
    public StatueController targetStatue;    // arraste o Statue.Scene.X aqui (com StatueController)
    public bool activateNext = true;         // se true: chama ActivateNextPart()
    [Tooltip("Usado se 'activateNext' estiver desmarcado. 0=Arms,1=Body,2=Legs...")]
    public int partIndex = 0;

    [Header("FX (opcional)")]
    public AudioSource sfxOnPickup;
    public GameObject vfxOnPickup;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Estátua (igual antes)
        if (!targetStatue) targetStatue = FindObjectOfType<StatueController>();
        if (targetStatue)
        {
            if (activateNext) targetStatue.ActivateNextPart();
            else              targetStatue.ActivatePart(partIndex);
        }
        else
        {
            Debug.LogWarning("[CollectibleChestPiece] Nenhum StatueController encontrado na cena.");
        }

        // FX
        if (sfxOnPickup) sfxOnPickup.Play();
        if (vfxOnPickup) Instantiate(vfxOnPickup, transform.position, Quaternion.identity);

        // Progresso (desbloqueio do Chest)
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.AddChestPiece(1);

            // Tenta abrir menu focado no Chest SE existir MenuController na UI
            if (GameProgress.Instance.chestUnlocked)
                TryOpenChestTabInMenu();
        }
        else
        {
            Debug.LogWarning("[CollectibleChestPiece] GameProgress não encontrado.");
        }

        if (destroyOnPickup) Destroy(gameObject); else gameObject.SetActive(false);
    }

    // Abre a aba de Chest no menu por reflexão (sem depender do tipo em compile-time)
    void TryOpenChestTabInMenu()
    {
        // procura qualquer MonoBehaviour chamado "MenuController" ativo na cena (inclusive cenas additive)
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        var menu = allBehaviours.FirstOrDefault(m => m && m.GetType().Name == "MenuController");
        if (menu == null) return;

        var method = menu.GetType().GetMethod("OpenCollectiblesChest", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (method != null)
        {
            method.Invoke(menu, null);
        }
    }
}
