using UnityEngine;

public class SHOP_CategoryButtons : MonoBehaviour
{
    // Open item category in shop
    public void OpenItemShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
            SHOP_Keeper.currentShopKeeper.OpenItemShop();
    }

    // Open weapon category in shop
    public void OpenWeaponShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
            SHOP_Keeper.currentShopKeeper.OpenWeaponShop();
    }

    // Open armour category in shop
    public void OpenArmourShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
            SHOP_Keeper.currentShopKeeper.OpenArmourShop();
    }
}
