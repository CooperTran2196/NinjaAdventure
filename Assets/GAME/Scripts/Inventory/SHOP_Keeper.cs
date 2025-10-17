using System;
using System.Collections.Generic;
using UnityEngine;

public class SHOP_Keeper : MonoBehaviour
{
    [Header("References")]
    Animator                  anim;
    Collider2D                talkTrigger;
    SHOP_Manager              shop_Manager;
    CanvasGroup               shopCanvasGroup;
    P_InputActions            input;
    
    public static SHOP_Keeper currentShopKeeper;

    [Header("Shop Inventory")]
    [SerializeField] List<INV_ItemSO> shopItems;
    [SerializeField] List<INV_ItemSO> shopWeapons;
    [SerializeField] List<INV_ItemSO> shopArmors;

    bool playerInRange;
    bool isShopOpen;

    public static event Action<SHOP_Manager, bool> OnShopStateChanged;

    void Awake()
    {
        anim        ??= GetComponentInChildren<Animator>(true);
        talkTrigger ??= GetComponentInChildren<CircleCollider2D>();
        input       = new P_InputActions();
        
        if (!anim)    { Debug.LogError($"{name}: Animator is missing!", this); return; }
        if (!talkTrigger) { Debug.LogError($"{name}: CircleCollider2D is missing!", this); return; }
    }

    // Assign references in Start to ensure SYS_GameManager.Instance is ready
    void Start()
    {
        if (SYS_GameManager.Instance != null)
        {
            shop_Manager = SYS_GameManager.Instance.shop_Manager;
            if (shop_Manager != null)
            {
                shopCanvasGroup = shop_Manager.GetComponent<CanvasGroup>();
            }
        }

        if (!shop_Manager)    { Debug.LogError($"{name}: Could not find SHOP_Manager from SYS_GameManager!", this); return; }
        if (!shopCanvasGroup) { Debug.LogError($"{name}: Could not find CanvasGroup on SHOP_Manager!", this); return; }
    }

    void OnEnable()
    {
        input.UI.Enable();
    }

    void OnDisable()
    {
        input.UI.Disable();
    }

    void OnDestroy()
    {
        input?.Dispose();
    }

    void Update()
    {
        if (playerInRange && input.UI.ToggleShop.WasPressedThisFrame())
        {
            if (!isShopOpen)
            {
                // Play open shop sound
                SYS_GameManager.Instance.sys_SoundManager.PlayOpenShop();
                
                Time.timeScale                  = 0;
                currentShopKeeper               = this;
                isShopOpen                      = true;
                OnShopStateChanged?.Invoke(shop_Manager, true);
                shopCanvasGroup.alpha           = 1;
                shopCanvasGroup.interactable    = true;
                shopCanvasGroup.blocksRaycasts  = true;
                OpenItemShop();
            }
            else
            {
                // Play close panel sound
                SYS_GameManager.Instance.sys_SoundManager.PlayClosePanel();
                
                Time.timeScale                  = 1;
                currentShopKeeper               = null;
                isShopOpen                      = false;
                OnShopStateChanged?.Invoke(shop_Manager, false);
                shopCanvasGroup.alpha           = 0;
                shopCanvasGroup.interactable    = false;
                shopCanvasGroup.blocksRaycasts  = false;
            }
        }
    }

    // Populates shop with consumable items
    public void OpenItemShop()
    {
        shop_Manager.PopulateShopItems(shopItems);
    }

    // Populates shop with weapons
    public void OpenWeaponShop()
    {
        shop_Manager.PopulateShopItems(shopWeapons);
    }

    // Populates shop with armor items
    public void OpenArmourShop()
    {
        shop_Manager.PopulateShopItems(shopArmors);
    }

    // Detect player entering talkTrigger area
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        anim?.SetBool("PlayerInRange", true);
    }

    // Detect player exiting talkTrigger area
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        anim?.SetBool("PlayerInRange", false);
    }
}
