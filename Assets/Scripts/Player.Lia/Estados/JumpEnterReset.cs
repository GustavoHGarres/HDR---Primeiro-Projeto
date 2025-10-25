using UnityEngine;
public class JumpEnterReset : StateMachineBehaviour
{
    static readonly int JumpH = Animator.StringToHash("Jump");
    static readonly int AttackingH = Animator.StringToHash("Attacking");
    public override void OnStateEnter(Animator anim, AnimatorStateInfo st, int layer)
    {
        anim.ResetTrigger(JumpH);
        anim.SetBool(AttackingH, false);
    }
}
