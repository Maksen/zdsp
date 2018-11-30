using Kopio.JsonContracts;
using System.Collections.Generic;
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
        if (type != -1)
        {
            int level = PowerUpInventory.powerUpSlots[type];
            return PowerUpRepo.GetPowerUpByPartsLevel((PowerUpPartsType)type, level);
        }
        return null;
    }
    #endregion

    #region MeridianPowerUp
    public List<int> GetAllMeridianLevelAll()
    {
        return PowerUpInventory.meridianLevelSlots;
    }

    public List<int> GetAllMeridianExpAll()
    {
        return PowerUpInventory.meridianExpSlots;
    }

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
        return PowerUpRepo.GetMeridianExpByTypesLevel(type, GetMeridianLevel(type));
    }

    public List<ItemInfo> GetMeridianUnlockMaterial (int type)
    {
        return PowerUpRepo.GetMeridianUnlockMaterial(type, GetMeridianLevel(type));
    }

    public List<ItemInfo> GetMeridianExpMaterial (int type)
    {
        return PowerUpRepo.GetMeridianExpMaterial(type, GetMeridianLevel(type));
    }
    #endregion
    
}
