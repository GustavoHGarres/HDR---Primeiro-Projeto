using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class PlayerSwitcherSimple : MonoBehaviour
{
    [Header("Players na ordem 1..5")]
    public Transform[] players;          // raízes dos 5 players
    public Transform[] moveRoots;        // opcional: se o CC/Agent fica num filho (senão deixe vazio)

    [Header("Câmera (opcional)")]
    public CinemachineFreeLook freeLook;
    public bool faceCameraOnSwap = true; // gira para a direção da câmera ao trocar

    int active = 0;

    Transform MR(int i) => (moveRoots != null && i < moveRoots.Length && moveRoots[i] != null) ? moveRoots[i] : players[i];

    void Awake()
    {
        if (players == null || players.Length == 0) { enabled = false; return; }

        // liga só o primeiro
        for (int i = 0; i < players.Length; i++)
            if (players[i]) players[i].gameObject.SetActive(i == active);

        // vincula câmera
        var m = MR(active);
        if (freeLook && m) { freeLook.Follow = m; freeLook.LookAt = m; }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Swap(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Swap(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Swap(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Swap(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) Swap(4);
    }

    public void Swap(int to)
    {
        if (to < 0 || to >= players.Length || to == active) return;

        // pegar posição/rotação do ATUAL
        var fromMR = MR(active);
        if (!fromMR) return;

        Vector3 spawnPos = fromMR.position;
        Quaternion spawnRot = fromMR.rotation;

        // alinhar ao heading da FreeLook (opcional)
        if (faceCameraOnSwap && freeLook)
        {
            Vector3 fwd = freeLook.transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 1e-4f) spawnRot = Quaternion.LookRotation(fwd.normalized, Vector3.up);
        }

        // desligar atual
        if (players[active]) players[active].gameObject.SetActive(false);

        // ligar novo
        var newRoot = players[to];
        var toMR = MR(to);
        if (!newRoot || !toMR) return;

        // travas seguras pra CharacterController/NavMeshAgent
        var cc  = toMR.GetComponent<CharacterController>();
        var nma = toMR.GetComponent<NavMeshAgent>();
        if (cc)  cc.enabled  = false;
        if (nma) nma.enabled = false;

        newRoot.gameObject.SetActive(true);

        toMR.position = spawnPos;
        toMR.rotation = spawnRot;
        if (nma) nma.Warp(spawnPos); // cola no NavMesh se tiver

        if (cc)  cc.enabled  = true;
        if (nma) nma.enabled = true;

        // câmera segue o novo
        if (freeLook)
        {
            freeLook.Follow = toMR;
            freeLook.LookAt = toMR;
        }

        active = to;
    }
}
