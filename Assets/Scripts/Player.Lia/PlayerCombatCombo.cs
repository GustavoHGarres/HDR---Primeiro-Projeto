using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerCombatCombo : MonoBehaviour
{
    [Header("Input")]
    public KeyCode buttonA = KeyCode.Mouse0;
    public KeyCode buttonB = KeyCode.Mouse1;

    [Header("Combo Limits")]
    [Range(1,5)] public int maxComboA = 5;
    [Range(1,5)] public int maxComboB = 5;

    [Header("Timings")]
    public float resetComboDelay = 0.75f; // tempo sem apertar → reseta
    public float minChainGap = 0.05f;     // anti-spam: bloq. multitriggers no mesmo frame

    // Animator params
    static readonly int AtkAHash   = Animator.StringToHash("AtkA");
    static readonly int AtkBHash   = Animator.StringToHash("AtkB");
    static readonly int ComboAHash = Animator.StringToHash("ComboA");
    static readonly int ComboBHash = Animator.StringToHash("ComboB");
    static readonly int CanChainH  = Animator.StringToHash("CanChain");
    static readonly int AttackingH = Animator.StringToHash("Attacking");

    Animator anim;

    // estado
    bool canChain;         // espelhando CanChain do animator (via eventos)
    bool attacking;        // true enquanto uma animação de ataque está tocando
    float lastPressTimeA;  // anti-spam
    float lastPressTimeB;
    float lastActionTime;  // p/ reset por inatividade

    int comboA; // 0..maxComboA
    int comboB; // 0..maxComboB

    void Awake()
    {
        anim = GetComponent<Animator>();
        ResetCombos();
    }

    void Update()
    {
        // Leitura de input
        if (Input.GetKeyDown(buttonA)) HandlePressA();
        if (Input.GetKeyDown(buttonB)) HandlePressB();

        // Reset por inatividade (não apertou nada por um tempo)
        if (attacking == false && (Time.time - lastActionTime) > resetComboDelay)
            ResetCombos();
    }

    void HandlePressA()
    {
        if (Time.time - lastPressTimeA < minChainGap) return; // anti-spam
        lastPressTimeA = Time.time;
        lastActionTime = Time.time;

        // Se outra cadeia (B) está “armada”, zera B antes de iniciar A
        if (comboB > 0 && !attacking) comboB = 0;

        if (!attacking)
        {
            // começar cadeia A
            comboA = Mathf.Clamp(comboA + 1, 1, maxComboA);
            anim.SetInteger(ComboAHash, comboA);
            anim.ResetTrigger(AtkBHash);
            anim.SetTrigger(AtkAHash);
            attacking = true;
        }
        else
        {
            // engatar próximo só se a janela estiver aberta
            if (canChain && comboA > 0 && comboA < maxComboA)
            {
                comboA++;
                anim.SetInteger(ComboAHash, comboA);
                anim.SetTrigger(AtkAHash);
            }
        }
    }

    void HandlePressB()
    {
        if (Time.time - lastPressTimeB < minChainGap) return;
        lastPressTimeB = Time.time;
        lastActionTime = Time.time;

        if (comboA > 0 && !attacking) comboA = 0;

        if (!attacking)
        {
            comboB = Mathf.Clamp(comboB + 1, 1, maxComboB);
            anim.SetInteger(ComboBHash, comboB);
            anim.ResetTrigger(AtkAHash);
            anim.SetTrigger(AtkBHash);
            attacking = true;
        }
        else
        {
            if (canChain && comboB > 0 && comboB < maxComboB)
            {
                comboB++;
                anim.SetInteger(ComboBHash, comboB);
                anim.SetTrigger(AtkBHash);
            }
        }
    }

    // ======= Chamados por Animation Events =======

    // Abre janela para engatar próximo golpe
    public void OpenComboWindow()
    {
        canChain = true;
        anim.SetBool(CanChainH, true);
    }

    // Fecha janela
    public void CloseComboWindow()
    {
        canChain = false;
        anim.SetBool(CanChainH, false);
    }

    // Ativação da hitbox (opcional – ligue colisor/trigger do arma)
    public void EnableHitbox()  { /* TODO: seu código de dano ON */ }
    public void DisableHitbox() { /* TODO: seu código de dano OFF */ }

    // Chamado no fim do clipe (ou em OnStateExit via StateMachineBehaviour)
    public void OnAttackAnimationEnd()
    {
        attacking = false;
        anim.SetBool(AttackingH, false);

        // se não “engatou” o próximo durante a janela, zera os contadores
        if (!canChain)
            ResetCombos();
        // se engatou, Update cuidará do próximo trigger quando você apertar
    }

    void ResetCombos()
    {
        comboA = 0; comboB = 0;
        canChain = false; attacking = false;
        anim.SetInteger(ComboAHash, 0);
        anim.SetInteger(ComboBHash, 0);
        anim.SetBool(CanChainH, false);
        anim.SetBool(AttackingH, false);
    }
}
