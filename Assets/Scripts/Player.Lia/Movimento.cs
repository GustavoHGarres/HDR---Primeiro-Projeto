using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movimento : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;              // arraste se não achar sozinho
    private CharacterController controller;
    private Transform cam;

    [Header("Velocidades (m/s)")]
    public float walkSpeed = 1.5f;
    public float runSpeed  = 4f;

    [Header("Suavização")]
    public float accel = 12f;          // aceleração para atingir alvo
    public float decel = 14f;          // desaceleração ao soltar
    public float turnSpeed = 12f;      // giro do corpo
    public float animDamp = 0.12f;     // damping dos SetFloat do Animator
    public float deadZone = 0.08f;     // zera perto do centro p/ cair no Idle certinho

    [Header("Gravidade")]
    public float gravity = -20f;

    // Parâmetros do Animator (nomes precisam bater com o Animator)
    private static readonly int MoveXHash = Animator.StringToHash("Move X");
    private static readonly int MoveYHash = Animator.StringToHash("Move Y");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    // Estado interno
    private Vector3 planarVel;   // velocidade horizontal suavizada (m/s)
    private float yVel;          // velocidade vertical

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        cam = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        // ---------- INPUT (relativo à câmera) ----------
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 rgt = cam ? Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized : Vector3.right;

        Vector3 inputDir = (fwd * v + rgt * h);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize(); // diagonais

        // Decide se quer correr
        bool wantRun = Input.GetKey(KeyCode.LeftShift);

        // Alvo de velocidade no plano (walk ou run)
        float targetMax = wantRun ? runSpeed : walkSpeed;
        Vector3 targetPlanar = inputDir * targetMax;

        // ---------- SUAVIZAÇÃO (reduz trancos) ----------
        float rate = (targetPlanar.sqrMagnitude > planarVel.sqrMagnitude) ? accel : decel;
        planarVel = Vector3.MoveTowards(planarVel, targetPlanar, rate * Time.deltaTime);

        // Zona morta para parar “redondo”
        if (planarVel.magnitude < deadZone) planarVel = Vector3.zero;

        // ---------- GRAVIDADE / MOVE ----------
        if (controller.isGrounded) yVel = -2f; else yVel += gravity * Time.deltaTime;

        Vector3 velocity = new Vector3(planarVel.x, yVel, planarVel.z);
        controller.Move(velocity * Time.deltaTime);

        // ---------- ROTACIONAR (comente para strafe puro) ----------
        if (planarVel.sqrMagnitude > 0.0001f)
        {
            Quaternion t = Quaternion.LookRotation(new Vector3(planarVel.x, 0, planarVel.z), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, t, turnSpeed * Time.deltaTime);
        }

        // ---------- ANIMATOR ----------
        // Normalize MoveX/MoveY pelo topo ATUAL (walk ou run) => corrige o limite ~0.37
        float normTop = wantRun ? runSpeed : walkSpeed;

        Vector3 localVel = transform.InverseTransformDirection(new Vector3(planarVel.x, 0f, planarVel.z));
        float mx = Mathf.Clamp(localVel.x / (normTop + 0.0001f), -1f, 1f);
        float my = Mathf.Clamp(localVel.z / (normTop + 0.0001f), -1f, 1f);

        // "Speed": escolha UMA das opções abaixo:

        // A) Toggle por Shift (0 = walk, 1 = run)
        float speed01 = wantRun ? 1f : 0f;

        // B) Suave por velocidade atual (descomente esta e comente a A se preferir contínuo)
        // float currentPlanar = new Vector3(planarVel.x, 0f, planarVel.z).magnitude;
        // float speed01 = Mathf.InverseLerp(walkSpeed, runSpeed, currentPlanar);

        animator.SetFloat(MoveXHash, mx, animDamp, Time.deltaTime);
        animator.SetFloat(MoveYHash, my, animDamp, Time.deltaTime);
        animator.SetFloat(SpeedHash, speed01, animDamp, Time.deltaTime);
    }
}
