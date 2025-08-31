using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class W_Base : MonoBehaviour
{
    [Header("Data")]
    public W_SO data;

    [Header("Owner + Targets")]
    public Transform owner;
    public LayerMask targetMask;

    // Cached
    protected SpriteRenderer sprite;
    protected BoxCollider2D hitbox;
    protected Animator ownerAnimator;
    protected P_Stats pStats;
    protected E_Stats eStats;

    protected virtual void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        hitbox ??= GetComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        owner ??= transform.root;
        ownerAnimator ??= owner ? owner.GetComponent<Animator>() : null;
        pStats ??= owner ? owner.GetComponent<P_Stats>() : null;
        eStats ??= owner ? owner.GetComponent<E_Stats>() : null;

        if (data && sprite) sprite.sprite = data.sprite;
    }

    public virtual void Equip(Transform newOwner)
    {
        owner = newOwner;
        ownerAnimator = owner ? owner.GetComponent<Animator>() : null;   // reassign, no ??=
        pStats       = owner ? owner.GetComponent<P_Stats>() : null;
        eStats       = owner ? owner.GetComponent<E_Stats>() : null;
        if (data && sprite) sprite.sprite = data.sprite;
    }

    public abstract void Attack();
}
