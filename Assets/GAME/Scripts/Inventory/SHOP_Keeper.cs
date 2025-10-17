using System;
using System.Collections.Generic;
using UnityEngine;

public class SHOP_Keeper : MonoBehaviour
{
    public static SHOP_Keeper currentShopKeeper;

    [Header("References")]
    Animator   anim;
    Collider2D trigger;

    [Header("MUST wire MANUALLY in Inspector")]
    public SHOP_Manager shop_Manager;
    public CanvasGroup  shopCanvasGroup;

    [Header("Shop Inventory")]
    public List<INV_ItemSO> shopItems;
    public List<INV_ItemSO> shopWeapons;
    public List<INV_ItemSO> shopArmors;

    bool           playerInRange;
    bool           isShopOpen;
    P_InputActions input;

    public static event Action<SHOP_Manager, bool> OnShopStateChanged;

    void Awake()
    {
        anim    ??= GetComponentInChildren<Animator>(true);
        trigger ??= GetComponent<CircleCollider2D>();
        input   = new P_InputActions();

        if (!anim)    Debug.LogWarning($"{name}: Animator is missing!", this);
        if (!trigger) { Debug.LogError($"{name}: CircleCollider2D is missing!", this); return; }
    }

    void Start()
    {
        // Get references from GameManager (singletons not ready in Awake)
        if (SYS_GameManager.Instance != null)
        {
            shop_Manager    ??= SYS_GameManager.Instance.shop_Manager;
            shopCanvasGroup ??= shop_Manager?.GetComponent<CanvasGroup>();
        }

        if (!shop_Manager)    { Debug.LogError($"{name}: SHOP_Manager not found in SYS_GameManager!", this); return; }
        if (!shopCanvasGroup) { Debug.LogError($"{name}: CanvasGroup not found on SHOP_Manager!", this); return; }
    }

    void OnEnable()
    {
        input.UI.Enable();
    }

    void OnDisable()
    {
        input.UI.Disable();
        input.Dispose();
    }

    void Update()
    {
        if (playerInRange && input.UI.ToggleShop.WasPressedThisFrame())
        {
            if (!isShopOpen)
            {
                Time.timeScale              = 0;
                currentShopKeeper           = this;
                isShopOpen                  = true;
                shopCanvasGroup.alpha       = 1;
                shopCanvasGroup.interactable   = true;
                shopCanvasGroup.blocksRaycasts = true;
                OnShopStateChanged?.Invoke(shop_Manager, true);
                OpenItemShop();
            }
            else
            {
                Time.timeScale              = 1;
                currentShopKeeper           = null;
                isShopOpen                  = false;
                shopCanvasGroup.alpha       = 0;
                shopCanvasGroup.interactable   = false;
                shopCanvasGroup.blocksRaycasts = false;
                OnShopStateChanged?.Invoke(shop_Manager, false);
            }
        }
    }

    // Populate shop with items category
    public void OpenItemShop()
    {
        shop_Manager.PopulateShopItems(shopItems);
    }

    // Populate shop with weapons category
    public void OpenWeaponShop()
    {
        shop_Manager.PopulateShopItems(shopWeapons);
    }

    // Populate shop with armour category
    public void OpenArmourShop()
    {
        shop_Manager.PopulateShopItems(shopArmors);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        anim?.SetBool("PlayerInRange", true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        anim?.SetBool("PlayerInRange", false);
    }
}
