using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SHOP_Keeper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator Anim;
    [SerializeField] CircleCollider2D trigger;
    public SHOP_Manager shop_Manager;
    public CanvasGroup shopCanvasGroup;
    public static SHOP_Keeper currentShopKeeper;

    bool playerInRange;
    bool isShopOpen = false;
    P_InputActions input;

    [SerializeField] private List<INV_ItemSO> shopItems;
    [SerializeField] private List<INV_ItemSO> shopWeapons;
    [SerializeField] private List<INV_ItemSO> shopArmors;

    public static event Action<SHOP_Manager, bool> OnShopStateChanged;

    void Awake()
    {
        Anim ??= GetComponentInChildren<Animator>(true);
        trigger ??= GetComponent<CircleCollider2D>();

        input = new P_InputActions();
        input.UI.ToggleShop.Enable();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        if (playerInRange && input.UI.ToggleShop.WasPressedThisFrame())
        {
            if (!isShopOpen)
            {
                Time.timeScale = 0; // Pause the game
                currentShopKeeper = this;
                isShopOpen = true;
                OnShopStateChanged?.Invoke(shop_Manager, true);
                shopCanvasGroup.alpha = 1; // Show the shop UI
                shopCanvasGroup.interactable = true;
                shopCanvasGroup.blocksRaycasts = true;
                OpenItemShop();

            }
            else
            {
                Time.timeScale = 1; // Resume the game
                currentShopKeeper = null;
                isShopOpen = false;
                OnShopStateChanged?.Invoke(shop_Manager, false);
                shopCanvasGroup.alpha = 0; // Hide the shop UI
                shopCanvasGroup.interactable = false;
                shopCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void OpenItemShop()
    {
        shop_Manager.PopulateShopItems(shopItems);
    }

    public void OpenWeaponShop()
    {
        shop_Manager.PopulateShopItems(shopWeapons);
    }
    public void OpenArmourShop()
    {
        shop_Manager.PopulateShopItems(shopArmors);
    }

    // Detect player entering trigger area
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        Anim?.SetBool("PlayerInRange", true);
    }

    // Detect player exiting trigger area
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        Anim?.SetBool("PlayerInRange", false);
    }
}
