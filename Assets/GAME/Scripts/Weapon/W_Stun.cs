using System.Collections;
using UnityEngine;

public static class W_Stun
{
    // Stun Player
    public static IEnumerator Apply(P_Movement m, float time)
    {
        m.SetDisabled(true);
        yield return new WaitForSeconds(time);
        m.SetDisabled(false);
    }

    // Stun Enemy
    public static IEnumerator Apply(E_Movement m, float time)
    {
        m.SetDisabled(true);
        yield return new WaitForSeconds(time);
        m.SetDisabled(false);
    }
}
