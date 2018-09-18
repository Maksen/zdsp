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

    public void InitFromStats(PowerUpStats powerupStats)
    {
        PowerUpInventory.InitFromStats(powerupStats);
    }

    public PowerUpJson GetPowerUpJson (PartsType part)
    {
        int type = PowerUpRepo.PartsTypeValue((int)part);
        int level = PowerUpInventory.powerUpSlots[type];
        return PowerUpRepo.GetPowerUpByPartsLevel((PowerUpPartsType)type, level);
    }
}
