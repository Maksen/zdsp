using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class PowerUpController
{
    public PowerUpInventoryData PowerUpInventory;

    public PowerUpController()
    {
        PowerUpInventory = new PowerUpInventoryData();
        PowerUpInventory.InitDefault();
    }

    public void InitFromPowerUpStats(PowerUpStats powerupStats)
    {
        PowerUpInventory.InitFromPowerUpStats(powerupStats);
    }

    public void InitFromMeridianStats(MeridianStats meridianStats)
    {
        PowerUpInventory.InitFromMeridianStats(meridianStats);
    }

    #region PowerUp
    public PowerUpJson GetPowerUpJson (PartsType part)
    {
        int type = PowerUpRepo.PartsTypeValue((int)part);
        int level = PowerUpInventory.powerUpSlots[type];
        return PowerUpRepo.GetPowerUpByPartsLevel((PowerUpPartsType)type, level);
    }
    #endregion

    #region MeridianPowerUp
    public int GetMeridianLevel (int type)
    {
        return PowerUpInventory.meridianLevelSlots[type];
    }

    public int GetMeridianExp (int type)
    {
        return PowerUpInventory.meridianExpSlots[type];
    }

    public MeridianUnlockListJson GetMeridianUnlockJson (int type)
    {
        return PowerUpRepo.GetMeridianUnlockByTypesLevel(type, GetMeridianLevel(type));
    }

    public MeridianExpListJson GetMeridianExpJson(int type)
    {
        return PowerUpRepo.GetMeridianExpByTypesLevel(type, GetMeridianExp(type));
    }

    public List<MeridianUnlockListJson> GetMeridianUnlockList ()
    {
        List<MeridianUnlockListJson> lis = new List<MeridianUnlockListJson>();
        for (int i = 0; i < PowerUpInventoryData.MAX_MERIDIANLEVELSLOTS; ++i)
        {
            lis.Add(GetMeridianUnlockJson(i));
        }
        return lis;
    }

    public List<MeridianExpListJson> GetMeridianExpList ()
    {
        List<MeridianExpListJson> lis = new List<MeridianExpListJson>();
        for (int i = 0; i < PowerUpInventoryData.MAX_MERIDIANLEVELSLOTS; ++i)
        {
            if(PowerUpInventory.meridianLevelSlots[i] == 0)
                lis.Add(null);
            else
                lis.Add(GetMeridianExpJson(i));
        }
        return lis;
    }

    public List<ItemInfo> GetMeridianUnlockMaterial (int type)
    {
        return PowerUpRepo.GetMeridianUnlockMaterial(type, GetMeridianLevel(type));
    }

    public List<ItemInfo> GetMeridianExpMaterial (int type)
    {
        return PowerUpRepo.GetMeridianExpMaterial(type, GetMeridianExp(type));
    }
    #endregion
    
}
