using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Player HUD (optional)")]
    public WeaponSlot[] slotUIs = new WeaponSlot[10];

    [Header("Startup Load-out (new)")]
    public WeaponSO barehandAction;
    public WeaponSO[] autoUnlock;

    [Header("Helpers")]
    public WeaponSpriteHelper spriteHelper;
    public LayerMask enemyMask = ~0;         // default “everything”
    public Transform attackPoint;
    public GameObject meleeHitboxPrefab;

    // runtime
    WeaponSO currentAction;
    IWeaponStrategy currentStrategy;
    public WeaponSO GetCurrentAction() => currentAction;

    readonly Dictionary<int, WeaponSO> inventory = new();
    Animator bodyAnim;
    bool isPlayer;

    public static event System.Action<WeaponSO> OnWeaponEquipped;   // HUD update

    void Awake()
    {
        isPlayer = CompareTag("Player");
        bodyAnim = GetComponent<Animator>();

        if (isPlayer)
        {
            for (int i = 1; i < slotUIs.Length; i++)
                slotUIs[i].gameObject.SetActive(false);
        }
    }

    // defer until every Awake has finished
    void Start()
    {
        if (barehandAction != null)
        {
            inventory[barehandAction.slotIndex] = barehandAction;          // add
            if (isPlayer) slotUIs[0].SetWeapon(barehandAction);            // fill HUD
            Equip(barehandAction);                                         // arm fist
        }
        foreach (var w in autoUnlock) if (w != null) AddWeapon(w);
        if (isPlayer && barehandAction != null)
        slotUIs[barehandAction.slotIndex].SetWeapon(barehandAction);   // fill slot 0

    }

    // Loot / shop calls this
    public void AddWeapon(WeaponSO a)
    {
        inventory[a.slotIndex] = a;
        if (isPlayer) slotUIs[a.slotIndex].SetWeapon(a);  // reveal
        Equip(a);                                        // auto-equip
    }

    // Switch active weapon
    public void Equip(WeaponSO a)
    {
        currentAction = a;
        currentStrategy = StrategyFactory.Get(a.style);
        spriteHelper.Configure(a);

        OnWeaponEquipped?.Invoke(a);    // notify all slots
    }

    // input
    void Update()
    {
        if (!isPlayer) return;
        HandleHotkeys();
        HandleAttack();
    }

    void HandleHotkeys()
    {
        for (int i = 0; i <= 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) && inventory.ContainsKey(i))
                Equip(inventory[i]);
    }

    void HandleAttack()
    {
        if (currentAction == null) return;

        Vector2 dir = GetInputDir();
        if (Input.GetButtonDown("Attack"))
        {
            var ctx = new WeaponContext(transform, currentAction,
                                        spriteHelper, bodyAnim, enemyMask);
            currentStrategy.Use(ctx, dir);
        }
    }

    // Same helper your old logic used
    public Vector2 GetInputDir()
    {
        float h = Input.GetAxisRaw("Horizontal"),
              v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) + Mathf.Abs(v) < 0.01f
            ? GetComponent<PlayerMovement>().lastMove
            : new Vector2(h, v).normalized;
    }

}
