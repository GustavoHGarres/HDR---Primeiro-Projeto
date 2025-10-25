using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class PlayerSwitcher_ByController : MonoBehaviour
{
    [Header("Players (1..5)")]
    public GameObject[] players;

    [Header("Câmera (opcional)")]
    public CinemachineFreeLook freeLook;
    public bool faceCameraOnSwap = true;

    [Header("Teclas de troca")]
    public KeyCode key1 = KeyCode.Alpha1;
    public KeyCode key2 = KeyCode.Alpha2;
    public KeyCode key3 = KeyCode.Alpha3;
    public KeyCode key4 = KeyCode.Alpha4;
    public KeyCode key5 = KeyCode.Alpha5;

    private int activeIndex = 0;

    void Awake()
    {
        // Garante que só um player esteja ativo no início
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i]) players[i].SetActive(i == activeIndex);
        }

        UpdateCameraTarget();
    }

    void Update()
    {
        if (Input.GetKeyDown(key1)) Swap(0);
        if (Input.GetKeyDown(key2)) Swap(1);
        if (Input.GetKeyDown(key3)) Swap(2);
        if (Input.GetKeyDown(key4)) Swap(3);
        if (Input.GetKeyDown(key5)) Swap(4);
    }

    void Swap(int newIndex)
    {
        if (newIndex == activeIndex || newIndex < 0 || newIndex >= players.Length)
            return;

        GameObject current = players[activeIndex];
        GameObject next = players[newIndex];
        if (!current || !next) return;

        // 🔹 Pega o CharacterController atual (onde está a posição real)
        CharacterController currentCC = current.GetComponent<CharacterController>();
        Transform refTransform = currentCC ? currentCC.transform : current.transform;

        Vector3 currentPos = refTransform.position;
        Quaternion currentRot = refTransform.rotation;

        // 🔹 Alinha a rotação com a câmera, se desejar
        if (faceCameraOnSwap && freeLook)
        {
            Vector3 camFwd = freeLook.transform.forward;
            camFwd.y = 0f;
            if (camFwd.sqrMagnitude > 0.01f)
                currentRot = Quaternion.LookRotation(camFwd.normalized, Vector3.up);
        }

        // 🔹 Desativa o player atual
        current.SetActive(false);

        // 🔹 Move e ativa o novo
        CharacterController nextCC = next.GetComponent<CharacterController>();
        NavMeshAgent nextAgent = next.GetComponent<NavMeshAgent>();

        if (nextCC) nextCC.enabled = false;
        if (nextAgent) nextAgent.enabled = false;

        next.transform.position = currentPos;
        next.transform.rotation = currentRot;

        if (nextAgent) nextAgent.Warp(currentPos);
        next.SetActive(true);

        if (nextCC) nextCC.enabled = true;
        if (nextAgent) nextAgent.enabled = true;

        activeIndex = newIndex;
        UpdateCameraTarget();
    }

    void UpdateCameraTarget()
    {
        if (!freeLook || players.Length == 0) return;

        GameObject current = players[activeIndex];
        if (!current) return;

        Transform camTarget = current.transform;

        // se houver um filho "CamTarget", usa ele
        var found = current.transform.Find("CamTarget");
        if (found) camTarget = found;

        freeLook.Follow = camTarget;
        freeLook.LookAt = camTarget;
    }
}
