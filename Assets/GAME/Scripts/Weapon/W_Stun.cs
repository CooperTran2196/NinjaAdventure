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

    // Stun Old-Enemy
    public static IEnumerator Apply(E_Movement m, float time)
    {
        m.SetDisabled(true);
        yield return new WaitForSeconds(time);
        m.SetDisabled(false);
    }

    // Stun New-Enemy (preferred)
    // Stun New-Enemy (NEW system does stun inside controller)
    public static IEnumerator Apply(E_Controller ec, float time)
    {
        if (!ec) yield break;
        ec.Stun(time);           // controller owns the logic + timing
        yield break;             // no need to wait here; controller handles duration
    }
}
