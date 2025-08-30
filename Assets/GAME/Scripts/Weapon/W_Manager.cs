using UnityEngine;
using System;

[DisallowMultipleComponent]
public class W_Manager : MonoBehaviour
{
    public enum Slot { Melee, Ranged, Magic }

    [Header("Owner / Root")]
    public Transform weaponRoot; // where weapon prefabs spawn

    [Header("Equipped Blueprints (drag SOs here)")]
    public W_SO meleeSO;
    public W_SO rangedSO;
    public W_SO magicSO; // placeholder

    [Header("Runtime Instances")]
    public W_Melee  meleeInst;
    public W_Base   rangedInst; // placeholder for later
    public W_Base   magicInst;  // placeholder

    [Header("Targets (set in Inspector)")]
    public LayerMask playerWeaponTargets; // e.g. Enemy
    public LayerMask enemyWeaponTargets;  // e.g. Player

    public event Action<Slot, W_SO> OnEquipped;

    // Requests that belong to the weapon system (no proxy)
    public static event Action<W_SO, Slot> EquipRequested;
    public static event Action<Slot>      AttackRequested;

    public static void RequestEquip(W_SO so, Slot slot) => EquipRequested?.Invoke(so, slot);
    public static void RequestAttack(Slot slot) => AttackRequested?.Invoke(slot);

    void Awake()
    {
        if (weaponRoot == null) weaponRoot = transform;
    }

    void OnEnable()
    {
        EquipRequested  += HandleEquipRequested;
        AttackRequested += HandleAttackRequested;
    }

    void OnDisable()
    {
        EquipRequested  -= HandleEquipRequested;
        AttackRequested -= HandleAttackRequested;
    }

    void Start()
    {
        if (meleeSO) Equip(meleeSO, Slot.Melee); // equip at start
    }

    public void Equip(W_SO so, Slot slot)
    {
        if (so == null || so.prefab == null) return;

        // Destroy old
        switch (slot)
        {
            case Slot.Melee:  if (meleeInst)  Destroy(meleeInst.gameObject);  break;
            case Slot.Ranged: if (rangedInst) Destroy(rangedInst.gameObject); break;
            case Slot.Magic:  if (magicInst)  Destroy(magicInst.gameObject);  break;
        }

        // Spawn new
        var go = Instantiate(so.prefab, weaponRoot);
        var baseComp = go.GetComponent<W_Base>();
        if (baseComp == null) { Debug.LogError($"Prefab {so.prefab.name} missing W_Base"); Destroy(go); return; }

        baseComp.data  = so;
        baseComp.owner = transform; // the character
        bool isPlayer = GetComponent<P_Stats>() != null;
        baseComp.targetMask = isPlayer ? playerWeaponTargets : enemyWeaponTargets; // ensure hits register  :contentReference[oaicite:4]{index=4}

        switch (slot)
        {
            //case Slot.Melee:  meleeInst  = baseComp as W_Melee;  break;
            case Slot.Ranged: rangedInst = baseComp;             break;
            case Slot.Magic:  magicInst  = baseComp;             break;
        }

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.enabled = false; // hidden until attack or preview

        OnEquipped?.Invoke(slot, so);
    }

    void HandleEquipRequested(W_SO so, Slot slot) => Equip(so, slot);

    void HandleAttackRequested(Slot slot)
    {
        // For now we only wire melee
        if (slot == Slot.Melee) meleeInst?.Attack();
    }
}
