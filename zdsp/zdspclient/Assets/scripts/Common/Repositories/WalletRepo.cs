using UnityEngine;
using System.Collections;
using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public class WalletRepo
    {
        static Dictionary<int, CurrencyWalletJson> mCurrencyWallet;
        static Dictionary<CurrencyType, LinkUIType> mCurrencyWindow;
        public static void Init(GameDBRepo gameData)
        {
            mCurrencyWallet = new Dictionary<int, CurrencyWalletJson>();
            mCurrencyWindow = new Dictionary<CurrencyType, LinkUIType>();
            foreach (var currency in gameData.CurrencyWallet)
            {
                int key = currency.Value.rowindex;
                if (mCurrencyWallet.ContainsKey(key) == false)
                    mCurrencyWallet.Add(key, currency.Value);

                mCurrencyWindow.Add(currency.Value.currencytype, currency.Value.uitype);
            }
        }

        public static Dictionary<int, CurrencyWalletJson> GetCurrency()
        {
            return mCurrencyWallet;
        }

        public static LinkUIType GetLinkUITypeByCurrencyType(CurrencyType type)
        {
            LinkUIType result = LinkUIType.None;
            mCurrencyWindow.TryGetValue(type, out result);
            return result;
        }

    }
}
