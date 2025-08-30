using UnityEngine;

public abstract class W_Base : MonoBehaviour
{
    [Header("Data")]
    public W_SO data;

    [Header("Owner / Targeting")]
    public Transform owner;         // set by W_Manager
    public LayerMask targetMask;    // who this weapon damages

    protected P_Movement pMove;
    protected E_Movement eMove;
    protected P_Stats    pStats;
    protected E_Stats    eStats;
    protected SpriteRenderer sprite;

    protected virtual void Awake()
    {
        sprite ??= GetComponentInChildren<SpriteRenderer>();
        if (owner == null) owner = transform.root;

        if (owner)
        {
            pMove  = owner.GetComponent<P_Movement>();
            eMove  = owner.GetComponent<E_Movement>();
            pStats = owner.GetComponent<P_Stats>();
            eStats = owner.GetComponent<E_Stats>();
        }

        if (data == null) Debug.LogWarning($"{name}: W_SO missing.");
        if (sprite == null) Debug.LogWarning($"{name}: SpriteRenderer missing.");
    }

    public abstract void Attack();

    protected Vector2 FacingDir
    {
        get
        {
            Vector2 d = pMove ? pMove.lastMove : (eMove ? eMove.lastMove : Vector2.down);
            if (d.sqrMagnitude < 0.0001f) d = Vector2.down;
            return d.normalized;
        }
    }

    protected int AttackerBaseDamage => pStats ? pStats.attackDmg : (eStats ? eStats.attackDmg : 0);

    protected int FinalDamage => AttackerBaseDamage + (data ? data.weaponDamage : 0);

    protected float FacingAngleDeg()
    {
        return Vector2.SignedAngle(Vector2.down, FacingDir);
    }

    protected Vector3 WorldPosAtOffset(float extra = 0f)
    {
        float off = (data ? data.offsetDistance : 0f) + extra;
        return (owner ? owner.position : transform.position) + (Vector3)(FacingDir * off);
    }
}
