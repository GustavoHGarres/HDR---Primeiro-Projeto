using UnityEngine;

public class AttackStateSync : StateMachineBehaviour
{
    static readonly int AttackingH  = Animator.StringToHash("Attacking");
    static readonly int CanChainH   = Animator.StringToHash("CanChain");
    static readonly int AtkAStartH  = Animator.StringToHash("AtkA_Start");
    static readonly int AtkANextH   = Animator.StringToHash("AtkA_Next");
    static readonly int AtkBStartH  = Animator.StringToHash("AtkB_Start");
    static readonly int AtkBNextH   = Animator.StringToHash("AtkB_Next");

    public override void OnStateEnter(Animator anim, AnimatorStateInfo st, int layer)
    {
        anim.SetBool(AttackingH, true);
    }

    public override void OnStateExit(Animator anim, AnimatorStateInfo st, int layer)
    {
        anim.SetBool(AttackingH, false);
        anim.SetBool(CanChainH, false);
        anim.ResetTrigger(AtkAStartH);
        anim.ResetTrigger(AtkANextH);
        anim.ResetTrigger(AtkBStartH);
        anim.ResetTrigger(AtkBNextH);
    }
}

