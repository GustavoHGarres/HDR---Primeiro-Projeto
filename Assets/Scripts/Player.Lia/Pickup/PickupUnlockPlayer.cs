using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class PickupUnlockPlayer : MonoBehaviour
{
    [Header("Slot a desbloquear (1..5)")]
    [Range(1,5)] public int slot = 2;

    [Header("Referências")]
    public PlayerSwitcherChildren switcher; // arraste o GO PlayerSwitcher
    public Image iconToEnable;              // arraste o Image_Player_X (desativado na cena)

    [Header("Comportamento")]
    public bool switchImmediately = true;
    public bool destroyAfterPickup = true;

    bool consumed;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        // Garantimos que quem encostou é o player (usa algo que só o player tem)
        if (!other.GetComponentInParent<PlayerSwitcherChildren>() &&
            !other.GetComponentInParent<PlayerCombatCombo>()) return;

        // Pega o switcher (campo ou cena)
        var sw = switcher ? switcher : FindObjectOfType<PlayerSwitcherChildren>();
        if (!sw) return;

        consumed = true;

        // Desbloqueia o slot
        sw.UnlockOneBased(slot);

        // Atualiza UI
        if (iconToEnable) iconToEnable.gameObject.SetActive(true);

        // Troca imediatamente (agora permitido)
        if (switchImmediately) sw.SwitchToOneBased(slot);

        if (destroyAfterPickup) Destroy(gameObject);
    }
}
