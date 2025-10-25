using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLia : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float runMultiplier = 1.6f;              // quanto mais rápido ao correr
    public bool moveRelativeToCamera = true;        // opcional
    public bool faceMoveDirection = true;           // gira para onde anda (opcional)

    [Header("Input")]
    public KeyCode runKey = KeyCode.LeftShift;      // tecla para correr

    [Header("Animator")]
    public Animator animator;
    public string walkBoolParam = "Speed";          // BOOL (Idle->Walk quando true)
    public string runBoolParam  = "RunMultiplier";  // BOOL (Walk->Run quando true)

    private CharacterController controller;
    private int walkHash, runHash;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        walkHash = Animator.StringToHash(walkBoolParam);
        runHash  = Animator.StringToHash(runBoolParam);
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Direção (mundo ou relativa à câmera)
        Vector3 move;
        if (moveRelativeToCamera && Camera.main != null)
        {
            Vector3 fwd = Camera.main.transform.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = Camera.main.transform.right; right.y = 0f; right.Normalize();
            move = fwd * v + right * h;
        }
        else
        {
            move = new Vector3(h, 0f, v);
        }

        bool isWalking = move.sqrMagnitude > 0.0001f;
        bool isRunning = isWalking && Input.GetKey(runKey); // só corre se estiver andando

        // Normaliza e aplica movimento
        if (move.sqrMagnitude > 1f) move.Normalize();
        float currentSpeed = speed * (isRunning ? runMultiplier : 1f);
        controller.SimpleMove(move * currentSpeed);

        // Rotaciona para a direção do movimento
        if (faceMoveDirection && isWalking)
        {
            Quaternion target = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 12f);
        }

        // Animator (BOOLs)
        if (animator)
        {
            animator.SetBool(walkHash, isWalking);
            animator.SetBool(runHash,  isRunning);
        }
    }
}
