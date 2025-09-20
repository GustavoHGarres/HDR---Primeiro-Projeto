using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivateStatueOnTouch : MonoBehaviour
{
    [Header("Geral")]
    public string playerTag = "Player";
    public bool oneShot = true;

    [Header("Ação")]
    public StatueController statue; // arraste aqui a estátua
    public enum Action { UnlockAllParts, ActivateNextPart }
    public Action action = Action.UnlockAllParts;

    private bool done = false;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (done && oneShot) return;
        if (!other.CompareTag(playerTag)) return;
        if (!statue) return;

        switch (action)
        {
            case Action.UnlockAllParts: statue.UnlockAllParts(); break;
            case Action.ActivateNextPart: statue.ActivateNextPart(); break;
        }

        done = true;
    }
}
