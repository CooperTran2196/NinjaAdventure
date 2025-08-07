using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponSpriteHelper : MonoBehaviour
{
    SpriteRenderer sr;
    DirOffset bank;                     // cached from WeaponActionSO
    WeaponSO data;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Configure(WeaponSO so)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();  // safety
        sr.sprite  = so.spriteInHand;
        bank       = so.dirOffset;
        sr.enabled = false;
        data       = so;
    }

    public void Show(Vector2 dir, float autoHide = -1f)
    {
        if (sr.sprite == null) return;  // bare-hand → nothing to draw
        sr.enabled = true;

        transform.rotation = Quaternion.FromToRotation(Vector2.down, dir);
        // position – choose override or fallback to distance * dir
        Vector2 offset = ChooseOffset(dir);
        if (offset == Vector2.zero)             // no override supplied
            offset = dir * data.offsetDistance; // use single distance

        transform.localPosition = offset;
        if (autoHide > 0) Invoke(nameof(Hide), autoHide);
    }
    public void Hide() => sr.enabled = false;

    Vector2 ChooseOffset(Vector2 d) => d.y > 0.1f ? bank.up
                                   : d.y < -0.1f ? bank.down
                                   : d.x < 0 ? bank.left
                                   : bank.right;
                                   
    
}
