using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SHOP_Keeper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator Anim;
    [SerializeField] Collider2D trigger;
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
    }

    void Start()
    {
        // Assign references in Start to ensure SYS_GameManager.Instance is ready.
        if (SYS_GameManager.Instance != null)
        {
            shop_Manager = SYS_GameManager.Instance.shop_Manager;
            if (shop_Manager != null)
            {
                shopCanvasGroup = shop_Manager.GetComponent<CanvasGroup>();
            }
        }

        if (!shop_Manager) Debug.LogError($"{name}: Could not find SHOP_Manager from SYS_GameManager.");
        if (!shopCanvasGroup) Debug.LogError($"{name}: Could not find CanvasGroup on SHOP_Manager.");
    }

    void OnEnable()
    {
        input.UI.Enable();
    }

    void OnDisable()
    {
        input.UI.Disable();
        // Dispose of the input object to prevent memory leaks
        input.Dispose();
    }

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
