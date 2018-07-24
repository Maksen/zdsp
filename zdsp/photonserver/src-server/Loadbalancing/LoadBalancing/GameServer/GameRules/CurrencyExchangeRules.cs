using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    public static class CurrencyExchangeRules
    {
        //to do changable with GMTools
        public static int basic = 58;
        public static int multiplier2 = 30;
        public static int multiplier3 = 10;
        public static int multiplier5 = 2;

        static CurrencyExchangeRules()
        {

        }

        public static void Init()
        {
            // GetCurrencyExchangeRate
            var exchangeRate = GameApplication.dbGM.CurrencyExchangeGM.GetCurrencyExchangeRate().Result;//task.wait
            if (exchangeRate.Count > 0)
            {
                multiplier2 = (int)exchangeRate["Rate2"];
                multiplier3 = (int)exchangeRate["Rate3"];
                multiplier5 = (int)exchangeRate["Rate5"];
                int value = 100 - multiplier5 - multiplier3 - multiplier2;
                basic = value > 0 ? value : 0;
            }
            else
            {
                //default
                basic = 58;
                multiplier2 = 30;
                multiplier3 = 10;
                multiplier5 = 2;
            }
        }

        public static async Task UpdateRate()
        {
            // GetCurrencyExchangeRate
            var exchangeRate = await GameApplication.dbGM.CurrencyExchangeGM.GetCurrencyExchangeRate();
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                multiplier2 = (int)exchangeRate["Rate2"];
                multiplier3 = (int)exchangeRate["Rate3"];
                multiplier5 = (int)exchangeRate["Rate5"];
                int value = 100 - multiplier5 - multiplier3 - multiplier2;
                basic = value > 0 ? value : 0;
            });
        }

        public static int GetMultiplier(int roll)
        {
            //is event on
            var config = GMActivityConfig.GetConfigInt(GMActivityType.CurrencyExchange, DateTime.Now);
            if (config != null)
            {
                var event_multiplier2 = config.mDataList[0];
                var event_multiplier3 = config.mDataList[1];
                var event_multiplier5 = config.mDataList[2];
                int value = 100 - event_multiplier5 - event_multiplier3 - event_multiplier2;
                var event_basic = value > 0 ? value : 0;
                return GetMultiplier(roll, event_basic, event_multiplier2, event_multiplier3, event_multiplier5);
            }
            else
            {
                return GetMultiplier(roll, basic, multiplier2, multiplier3, multiplier5);
            }            
        }

        static int GetMultiplier(int mRoll, int mBasic, int mMultiplier2, int mMultiplier3, int mMultiplier5)
        {
            if (mRoll < mBasic)
                return 1;
            else if (mRoll <= mBasic + mMultiplier2)
                return 2;
            else if (mRoll <= mBasic + mMultiplier2 + mMultiplier3)
                return 3;
            else if (mRoll <= mBasic + mMultiplier2 + mMultiplier3 + mMultiplier5)
                return 5;
            else
                return 1;
        }
    }
}
