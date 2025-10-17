using UnityEngine;

public class SHOP_CategoryButtons : MonoBehaviour
{
    // Opens the item shop category
    public void OpenItemShop()
    {
        SHOP_Keeper.currentShopKeeper.OpenItemShop();
    }

    // Opens the weapon shop category
    public void OpenWeaponShop()
    {
        SHOP_Keeper.currentShopKeeper.OpenWeaponShop();
    }

    // Opens the armour shop category
    public void OpenArmourShop()
    {
        SHOP_Keeper.currentShopKeeper.OpenArmourShop();
    }
}
