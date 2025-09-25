using UnityEngine;

public class SHOP_CategoryButtons : MonoBehaviour
{
    public void OpenItemShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
        {
            SHOP_Keeper.currentShopKeeper.OpenItemShop();
        }
    }

    public void OpenWeaponShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
        {
            SHOP_Keeper.currentShopKeeper.OpenWeaponShop();
        }
    }

    public void OpenArmourShop()
    {
        if (SHOP_Keeper.currentShopKeeper != null)
        {
            SHOP_Keeper.currentShopKeeper.OpenArmourShop();
        }
    }
    
}
