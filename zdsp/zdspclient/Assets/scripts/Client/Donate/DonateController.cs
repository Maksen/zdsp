using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Common.Entities;

public class DonateSynStatsClient : DonateSynStats
{

}

public class DonateClientController
{
    private List<DonateOrderData> mDonateOrder;
    private bool bInit = false;

    public void Init()
    {
        bInit = true;
    }

    public void DeserializeData(string field, string data, string olddata)
    {
        switch (field)
        {
            case "donateData":
                mDonateOrder = DeserializeData(data);
                UpdateUI();
                break;
        }
    }

    private List<DonateOrderData> DeserializeData(string data)
    {
        return string.IsNullOrEmpty(data) ? new List<DonateOrderData>() : JsonConvertDefaultSetting.DeserializeObject<List<DonateOrderData>>(data);
    }

    public List<DonateOrderData> GetDonateOrders()
    {
        return mDonateOrder;
    }

    public DonateOrderData GetDonateData(string guid)
    {
        List<DonateOrderData> result = mDonateOrder.Where(o => o.Guid == guid).ToList();
        if (result.Count > 0)
        {
            return result[0];
        }
        return null;
    }

    private void UpdateUI()
    {
        if (UIManager.IsWindowOpen(WindowType.DailyQuest))
        {
            UIManager.GetWindowGameObject(WindowType.DailyQuest).GetComponent<UI_DailyActivity>().UpdateDonate();
        }
    }
}