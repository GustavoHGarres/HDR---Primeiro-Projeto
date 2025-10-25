using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class PlayerSwitcherShared : MonoBehaviour
{
    [Header("Players (1..5) – raízes dos personagens")]
    public Transform[] players;

    [Header("Mover roots (opcional)")]
    [Tooltip("Se o CharacterController/NavMeshAgent fica num FILHO, arraste o filho correspondente aqui. Se vazio, usa a própria raiz.")]
    public Transform[] moveRoots;

    [Header("Câmera (opcional)")]
    public CinemachineFreeLook freeLook;
    public bool faceCameraOnSwap = true; // virar para direção da câmera ao trocar

    [Header("Teclas")]
    public KeyCode key1 = KeyCode.Alpha1;
    public KeyCode key2 = KeyCode.Alpha2;
    public KeyCode key3 = KeyCode.Alpha3;
    public KeyCode key4 = KeyCode.Alpha4;
    public KeyCode key5 = KeyCode.Alpha5;

    int active = 0;

    // POSIÇÃO COMPARTILHADA GLOBAL (sempre atualizada pela Lia ativa)
    Vector3 sharedPos;
    Quaternion sharedRot;

    Transform MR(int i)
    {
        if (moveRoots != null && i < moveRoots.Length && moveRoots[i] != null) return moveRoots[i];
        return players[i];
    }

    void Awake()
    {
        if (players == null || players.Length == 0) { enabled = false; return; }

        // desativa todos, liga só o primeiro
        for (int i = 0; i < players.Length; i++)
            if (players[i]) players[i].gameObject.SetActive(i == active);

        // inicia posição compartilhada a partir do mover atual
        var mr = MR(active);
        sharedPos = mr.position;
        sharedRot = mr.rotation;

        // vincula câmera
        if (freeLook)
        {
            freeLook.Follow = mr;
            freeLook.LookAt = mr;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(key1)) Swap(0);
        if (Input.GetKeyDown(key2)) Swap(1);
        if (Input.GetKeyDown(key3)) Swap(2);
        if (Input.GetKeyDown(key4)) Swap(3);
        if (Input.GetKeyDown(key5)) Swap(4);

        // atualiza SEMPRE o ponto compartilhado com a pose da Lia ativa
        var mr = MR(active);
        sharedPos = mr.position;
        sharedRot = mr.rotation;
    }

    public void Swap(int to)
    {
        if (to < 0 || to >= players.Length || to == active) return;

        // rotação opcional alinhada à câmera (só yaw)
        Quaternion spawnRot = sharedRot;
        if (faceCameraOnSwap && freeLook)
        {
            Vector3 fwd = freeLook.transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 1e-4f) spawnRot = Quaternion.LookRotation(fwd.normalized, Vector3.up);
        }

        // desativa atual
        if (players[active]) players[active].gameObject.SetActive(false);

        // ativa novo exatamente na posição COMPARTILHADA
        var newRoot = players[to];
        var toMR = MR(to);
        if (!newRoot || !toMR) return;

        var cc  = toMR.GetComponent<CharacterController>();
        var nma = toMR.GetComponent<NavMeshAgent>();

        if (cc)  cc.enabled  = false;
        if (nma) nma.enabled = false;

        newRoot.gameObject.SetActive(true);

        toMR.position = sharedPos;
        toMR.rotation = spawnRot;
        if (nma) nma.Warp(sharedPos);

        if (cc)  cc.enabled  = true;
        if (nma) nma.enabled = true;

        // rebind câmera
        if (freeLook)
        {
            freeLook.Follow = toMR;
            freeLook.LookAt = toMR;
        }

        active = to;
    }
}
