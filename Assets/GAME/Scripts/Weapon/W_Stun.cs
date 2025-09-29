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
    public static IEnumerator Apply(E_Controller ec, float time)
    {
        if (!ec) yield break;
        ec.SetDisabled(true);
        yield return new WaitForSeconds(time);
        ec.SetDisabled(false);
    }
}
