// AttackEndSMB.cs
using UnityEngine;
public class AttackEndSMB : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var ai = animator.GetComponentInParent<AIController>(); // �� �θ𿡼� AI ã��
        ai?.OnAttackAnimationEnd();
    }
}
