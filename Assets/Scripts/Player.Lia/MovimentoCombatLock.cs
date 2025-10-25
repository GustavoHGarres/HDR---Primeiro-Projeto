using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovimentoCombatLock : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    private CharacterController controller;
    private Transform cam;

    [Header("Velocidades (m/s)")]
    public float walkSpeed = 1.5f;
    public float runSpeed  = 4f;

    [Header("Suavização")]
    public float accel = 12f;
    public float decel = 14f;
    public float turnSpeed = 12f;
    public float lockTurnSpeed = 14f;
    public float animDamp = 0.12f;
    public float deadZone = 0.08f;

    [Header("Gravidade e Pulo")]
    public float gravity = -25f;
    public float jumpForce = 7.5f;
    public float doubleJumpForce = 7.0f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float doubleTapWindow = 0.25f;

    [Header("Lock / Strafe")]
    public bool lockOn = false;
    public string enemyTag = "Enemy";
    public float lockMaxDistance = 25f;
    public Transform lockTarget;

    [Header("Head Look")]
    [Range(0f, 1f)] public float headWeight = 0.85f;
    [Range(0f, 1f)] public float bodyWeight = 0.35f;
    public float lookSmooth = 4f;

    [Header("Input")]
    public KeyCode runKey  = KeyCode.LeftShift;
    public KeyCode lockKey = KeyCode.Tab;
    public KeyCode jumpKey = KeyCode.Space;
    // Mapeamento dos botões de ataque
    public int attackAButton = 0; // Mouse0
    public int attackBButton = 1; // Mouse1

    [Header("Pulo durante ataque")]
    [Tooltip("Se falso, o pulo é bloqueado enquanto Attacking==true (recomendado).")]
    public bool allowJumpDuringAttack = false;

    // Animator params
    static readonly int MoveXHash   = Animator.StringToHash("Move X");
    static readonly int MoveYHash   = Animator.StringToHash("Move Y");
    static readonly int SpeedHash   = Animator.StringToHash("Speed");
    static readonly int LockOnHash  = Animator.StringToHash("LockOn");
    static readonly int JumpHash    = Animator.StringToHash("Jump");
    static readonly int DoubleJHash = Animator.StringToHash("DoubleJump");
    static readonly int IsGroundedH = Animator.StringToHash("IsGrounded");
    static readonly int YVelHash    = Animator.StringToHash("YVel");
    static readonly int LandTypeH   = Animator.StringToHash("LandType");

    // Combo gate
    static readonly int AttackingH  = Animator.StringToHash("Attacking");
    static readonly int CanChainH   = Animator.StringToHash("CanChain");

    static readonly int AtkAStartH  = Animator.StringToHash("AtkA_Start");
    static readonly int AtkANextH   = Animator.StringToHash("AtkA_Next");
    static readonly int AtkBStartH  = Animator.StringToHash("AtkB_Start");
    static readonly int AtkBNextH   = Animator.StringToHash("AtkB_Next");

    // Estado interno
    Vector3 planarVel;
    float yVel;

    float lastGroundTime;
    float lastJumpPressedTime = -999f;
    float lastTapTime        = -999f;
    bool  wantDoubleJumpQueued = false;
    bool  canDoubleJump;
    int   lastJumpType;
    bool  wasGrounded;

    // Buffers de clique
    bool queuedAtkA = false;
    bool queuedAtkB = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        cam = Camera.main ? Camera.main.transform : null;
    }

    // Considera tag "Attack" no estado atual/próximo
    bool IsInAttackState()
    {
        if (!animator) return false;
        var cur = animator.GetCurrentAnimatorStateInfo(0);
        if (cur.IsTag("Attack")) return true;
        if (animator.IsInTransition(0))
        {
            var nxt = animator.GetNextAnimatorStateInfo(0);
            if (nxt.IsTag("Attack")) return true;
        }
        return animator.GetBool(AttackingH);
    }

    void Update()
    {
        // Lock toggle
        if (Input.GetKeyDown(lockKey)) { if (!lockOn) TryLockTarget(); else ClearLock(); }

        // Ataques
        if (Input.GetMouseButtonDown(attackAButton)) TryAttackA();
        if (Input.GetMouseButtonDown(attackBButton)) TryAttackB();

        // Pulo (buffer + double tap)
        if (Input.GetKeyDown(jumpKey))
        {
            if (allowJumpDuringAttack || !IsInAttackState())
            {
                lastJumpPressedTime = Time.time;
                if (Time.time - lastTapTime <= doubleTapWindow) wantDoubleJumpQueued = true;
                lastTapTime = Time.time;
            }
        }

        // Consumo de buffers assim que a janela abre (prioriza o último clique)
        if (animator.GetBool(CanChainH))
        {
            if (queuedAtkB) { queuedAtkB = false; queuedAtkA = false;
                animator.ResetTrigger(JumpHash);
                animator.SetTrigger(AtkBNextH);
                animator.SetBool(AttackingH, true);
            }
            else if (queuedAtkA) { queuedAtkA = false;
                animator.ResetTrigger(JumpHash);
                animator.SetTrigger(AtkANextH);
                animator.SetBool(AttackingH, true);
            }
        }

        // Movimento relativo à câmera
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 rgt = cam ? Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized : Vector3.right;
        Vector3 inputDir = (fwd * v + rgt * h);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        bool wantRun   = Input.GetKey(runKey);
        float targetMax = wantRun ? runSpeed : walkSpeed;
        Vector3 targetPlanar = inputDir * targetMax;

        float rate = (targetPlanar.sqrMagnitude > planarVel.sqrMagnitude) ? accel : decel;
        planarVel = Vector3.MoveTowards(planarVel, targetPlanar, rate * Time.deltaTime);
        if (planarVel.magnitude < deadZone) planarVel = Vector3.zero;

        bool grounded = controller.isGrounded;
        if (grounded) lastGroundTime = Time.time;

        bool wantJump = (Time.time - lastJumpPressedTime) <= jumpBufferTime;

        if (!allowJumpDuringAttack && IsInAttackState())
        {
            animator.ResetTrigger(JumpHash);
            wantJump = false;
            wantDoubleJumpQueued = false;
        }

        bool canCoyote = (Time.time - lastGroundTime) <= coyoteTime;

        if (wantJump && (grounded || canCoyote))
        {
            yVel = jumpForce;
            animator.SetTrigger(JumpHash);
            lastJumpType = 0;
            canDoubleJump = true;
            lastJumpPressedTime = -999f;
        }

        if (!grounded && canDoubleJump && wantDoubleJumpQueued)
        {
            yVel = doubleJumpForce;
            animator.SetTrigger(DoubleJHash);
            lastJumpType = 1;
            canDoubleJump = false;
            wantDoubleJumpQueued = false;
        }

        if (grounded && yVel < 0f) yVel = -2f; else yVel += gravity * Time.deltaTime;

        Vector3 velocity = new Vector3(planarVel.x, yVel, planarVel.z);
        controller.Move(velocity * Time.deltaTime);

        if (lockOn && lockTarget)
        {
            Vector3 faceDir = lockTarget.position - transform.position; faceDir.y = 0f;
            if (faceDir.sqrMagnitude > 0.0001f)
            {
                var t = Quaternion.LookRotation(faceDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, t, lockTurnSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (lockOn && !lockTarget && cam)
            {
                Vector3 faceDir = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                var t = Quaternion.LookRotation(faceDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, t, lockTurnSpeed * Time.deltaTime);
            }
            else if (planarVel.sqrMagnitude > 0.0001f)
            {
                var t = Quaternion.LookRotation(new Vector3(planarVel.x, 0, planarVel.z), Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, t, turnSpeed * Time.deltaTime);
            }
        }

        if (!wasGrounded && grounded)
        {
            animator.SetInteger(LandTypeH, lastJumpType);
            canDoubleJump = false;
            wantDoubleJumpQueued = false;
        }
        wasGrounded = grounded;

        float normTop = wantRun ? runSpeed : walkSpeed;
        Vector3 localVel = transform.InverseTransformDirection(new Vector3(planarVel.x, 0f, planarVel.z));
        float mx = Mathf.Clamp(localVel.x / (normTop + 0.0001f), -1f, 1f);
        float my = Mathf.Clamp(localVel.z / (normTop + 0.0001f), -1f, 1f);
        float speed01 = wantRun ? 1f : 0f;

        animator.SetFloat(MoveXHash, mx, animDamp, Time.deltaTime);
        animator.SetFloat(MoveYHash, my, animDamp, Time.deltaTime);
        animator.SetFloat(SpeedHash, speed01, animDamp, Time.deltaTime);
        animator.SetBool (LockOnHash, lockOn);
        animator.SetBool (IsGroundedH, grounded);
        animator.SetFloat(YVelHash, yVel);
    }

    // ===== Attack A =====
    void TryAttackA()
    {
        bool attacking = IsInAttackState();
        bool canChain  = animator.GetBool(CanChainH);

        animator.ResetTrigger(JumpHash);

        if (!attacking)
        {
            animator.SetTrigger(AtkAStartH);
            animator.SetBool(AttackingH, true);
            queuedAtkA = false; queuedAtkB = false;
        }
        else if (canChain)
        {
            animator.SetTrigger(AtkANextH);
            animator.SetBool(AttackingH, true);
            queuedAtkA = queuedAtkB = false;
        }
        else
        {
            // prefere o último clique: sobrescreve o buffer do outro
            queuedAtkA = true; queuedAtkB = false;
        }
    }

    // ===== Attack B =====
    void TryAttackB()
    {
        bool attacking = IsInAttackState();
        bool canChain  = animator.GetBool(CanChainH);

        animator.ResetTrigger(JumpHash);

        if (!attacking)
        {
            animator.SetTrigger(AtkBStartH);
            animator.SetBool(AttackingH, true);
            queuedAtkA = false; queuedAtkB = false;
        }
        else if (canChain)
        {
            animator.SetTrigger(AtkBNextH);
            animator.SetBool(AttackingH, true);
            queuedAtkA = queuedAtkB = false;
        }
        else
        {
            queuedAtkB = true; queuedAtkA = false;
        }
    }

    // ===== LOCK =====
    void TryLockTarget()
    {
        Transform best = null; float bestSqr = Mathf.Infinity;
        foreach (var go in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            float d2 = (go.transform.position - transform.position).sqrMagnitude;
            if (d2 < bestSqr && d2 <= lockMaxDistance * lockMaxDistance)
            { bestSqr = d2; best = go.transform; }
        }
        lockTarget = best; lockOn = true;
    }
    void ClearLock() { lockOn = false; lockTarget = null; }

    // ===== HEAD LOOK =====
    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;
        if (lockOn)
        {
            Vector3 lookPos;
            if (lockTarget) lookPos = lockTarget.position + Vector3.up * 1.5f;
            else if (cam)   lookPos = cam.position + cam.forward * 10f;
            else            lookPos = transform.position + transform.forward * 10f;

            animator.SetLookAtWeight(headWeight, bodyWeight, 0.5f, 1f, 0.5f);
            animator.SetLookAtPosition(lookPos);
        }
        else animator.SetLookAtWeight(0f);
    }
}
