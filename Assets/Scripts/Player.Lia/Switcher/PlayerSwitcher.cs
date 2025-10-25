using UnityEngine;
using UnityEngine.AI;

public class PlayerSwitcher : MonoBehaviour
{
    [Header("Players 1..5 (raiz de cada variante)")]
    public Transform[] players;

    [Header("Âncora mundial (Empty na cena)")]
    public Transform swapAnchor;

    [SerializeField] int activeIndex = 0;

    Vector3[] lastPos;
    Quaternion[] lastRot;
    bool[] hasMemory;
    Transform[] moveRoots;

     void Awake()
    {
        if (players == null || players.Length == 0) { enabled = false; return; }
        if (!swapAnchor)
        {
            var go = new GameObject("SwapAnchor");
            swapAnchor = go.transform;
        }

        int n = players.Length;
        lastPos   = new Vector3[n];
        lastRot   = new Quaternion[n];
        hasMemory = new bool[n];
        moveRoots = new Transform[n];

        for (int i = 0; i < n; i++)
        {
            moveRoots[i] = FindMoveRoot(players[i]);
            var mr = moveRoots[i] ? moveRoots[i] : players[i];
            lastPos[i] = mr.position;
            lastRot[i] = mr.rotation;

            if (players[i]) players[i].gameObject.SetActive(false);
        }

        // >>> NOVO: alinhar a ancora ao moveRoot do player inicial
        var startMR = moveRoots[activeIndex] ? moveRoots[activeIndex] : players[activeIndex];
        if (swapAnchor && startMR)
        {
            swapAnchor.SetParent(startMR, false);
            swapAnchor.localPosition = Vector3.zero;
            swapAnchor.localRotation = Quaternion.identity;
            // solta no mundo mantendo posição — assim Activate usa exatamente esse ponto
            swapAnchor.SetParent(null, true);
        }

        ActivateAtIndex(activeIndex, firstTime:true);
        hasMemory[activeIndex] = true;
    }

    void Update()
    {
        // hotkeys
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTo(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTo(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTo(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTo(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchTo(4);

        // ⬅⬅ PONTO-CHAVE: sempre memorizar a posição do ativo
        var mr = CurrentMoveRoot();
        if (mr)
        {
            lastPos[activeIndex] = mr.position;
            lastRot[activeIndex] = mr.rotation;
        }
    }

    void LateUpdate()
    {
        // mantém a âncora parentada no moveRoot atual (anda junto)
        var mr = CurrentMoveRoot();
        if (swapAnchor && mr && swapAnchor.parent != mr)
        {
            swapAnchor.SetParent(mr, false);
            swapAnchor.localPosition = Vector3.zero;
            swapAnchor.localRotation = Quaternion.identity;
        }
    }

    Transform CurrentMoveRoot()
    {
        if (activeIndex < 0 || activeIndex >= players.Length) return null;
        var mr = moveRoots[activeIndex];
        return mr ? mr : players[activeIndex];
    }

    public void SwitchTo(int newIndex)
    {
        if (newIndex < 0 || newIndex >= players.Length) return;
        if (newIndex == activeIndex) return;

        // fixa âncora no ponto atual (mundo)
        var curMR = CurrentMoveRoot();
        if (swapAnchor && curMR)
        {
            swapAnchor.SetParent(null, true);
            swapAnchor.SetPositionAndRotation(curMR.position, curMR.rotation);
        }

        // desativa o atual (já memorizei a pos no Update)
        if (players[activeIndex]) players[activeIndex].gameObject.SetActive(false);

        // ativa o novo
        activeIndex = newIndex;
        ActivateAtIndex(activeIndex, firstTime:!hasMemory[activeIndex]);
        hasMemory[activeIndex] = true;
    }

    void ActivateAtIndex(int index, bool firstTime)
    {
        var root = players[index];
        if (!root) return;

        var move = moveRoots[index] ? moveRoots[index] : root;

        // alvo: 1ª vez => âncora; senão => última posição memorada
        Vector3 targetPos = firstTime && swapAnchor ? swapAnchor.position : lastPos[index];
        Quaternion targetRot = firstTime && swapAnchor ? swapAnchor.rotation : lastRot[index];

        // Teleporte robusto
        var cc    = move.GetComponent<CharacterController>();
        var agent = move.GetComponent<NavMeshAgent>();

        if (cc) cc.enabled = false;
        if (agent) agent.enabled = false;

        // se o moveRoot é filho, movo a RAIZ pela diferença para alinhar o filho
        Vector3 delta = targetPos - move.position;
        root.position += delta;

        // (opcional) só yaw
        Vector3 fwd = targetRot * Vector3.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude > 0.0001f) root.rotation = Quaternion.LookRotation(fwd, Vector3.up);

        root.gameObject.SetActive(true);

        if (agent)
        {
            // garante que o agente "aceite" a nova posição
            agent.Warp(move.position); // já alinhado
            agent.enabled = true;
        }
        if (cc) cc.enabled = true;

        // regruda a âncora no novo moveRoot
        if (swapAnchor && move)
        {
            swapAnchor.SetParent(move, false);
            swapAnchor.localPosition = Vector3.zero;
            swapAnchor.localRotation = Quaternion.identity;
        }
    }

    Transform FindMoveRoot(Transform root)
    {
        if (!root) return null;

        var cc = root.GetComponentInChildren<CharacterController>(true);
        if (cc) return cc.transform;

        var agent = root.GetComponentInChildren<NavMeshAgent>(true);
        if (agent) return agent.transform;

        var rb = root.GetComponentInChildren<Rigidbody>(true);
        if (rb) return rb.transform;

        var anim = root.GetComponentInChildren<Animator>(true);
        if (anim && anim.applyRootMotion) return anim.transform;

        return root;
    }
}
