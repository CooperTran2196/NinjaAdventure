using UnityEngine;

public class AttackSpriteSMB : StateMachineBehaviour
{
    WeaponManager wm;

    // called the very first frame the state becomes active
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (wm == null) wm = animator.GetComponent<WeaponManager>();
        Vector2 dir = new Vector2(animator.GetFloat("attackX"), animator.GetFloat("attackY"));
        if (dir.sqrMagnitude < 0.01f) dir = wm.GetInputDir();   // fallback
        wm.spriteHelper.Show(dir, -1f);                         // stay until Exit
    }

    //called the very first frame the state is *no longer* active
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (wm == null) wm = animator.GetComponent<WeaponManager>();
        wm.spriteHelper.Hide();
    }
}
