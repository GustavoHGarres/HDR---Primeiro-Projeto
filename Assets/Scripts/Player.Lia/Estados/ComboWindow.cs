using UnityEngine;

public class ComboWindow : StateMachineBehaviour
{
    [Header("Janela (tempo normalizado 0..1)")]
    [Range(0f,1f)] public float openAt  = 0.35f;
    [Range(0f,1f)] public float closeAt = 0.75f;

    static readonly int CanChainH = Animator.StringToHash("CanChain");

    public override void OnStateEnter(Animator anim, AnimatorStateInfo st, int layerIndex)
    {
        anim.SetBool(CanChainH, false);
    }

    public override void OnStateUpdate(Animator anim, AnimatorStateInfo st, int layerIndex)
    {
        // tempo normalizado do ciclo atual (suporta loops)
        float t = st.normalizedTime - Mathf.Floor(st.normalizedTime);
        bool inWindow = (t >= openAt && t <= closeAt);
        anim.SetBool(CanChainH, inWindow);
    }

    public override void OnStateExit(Animator anim, AnimatorStateInfo st, int layerIndex)
    {
        anim.SetBool(CanChainH, false);
    }
}
