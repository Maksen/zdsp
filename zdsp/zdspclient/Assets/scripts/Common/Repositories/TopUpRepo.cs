using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;

namespace Zealot.Repository
{
    public static class TopUpRepo
    {
        public static Dictionary<string, int> mNameMapAndroid;
        public static Dictionary<int, TopUpItemAndroidJson> mIdMapAndroid;

        public static Dictionary<string, int> mNameMapApple;
        public static Dictionary<int, TopUpItemAppleJson> mIdMapApple;

        public static Dictionary<string, int> mNameMapMyCard;
        public static Dictionary<int, TopUpItemMyCardJson> mIdMapMyCard;

        public static List<TopUpItemAndroidJson> _uiOrderedTopUpItemsAndroid;
        public static List<TopUpItemAppleJson> _uiOrderedTopUpItemsApple;
        public static List<TopUpItemMyCardJson> _uiOrderedTopUpItemsMyCard;

        static TopUpRepo()
        {
            mNameMapAndroid = new Dictionary<string, int>();
            mIdMapAndroid = new Dictionary<int, TopUpItemAndroidJson>();

            mNameMapApple = new Dictionary<string, int>();
            mIdMapApple = new Dictionary<int, TopUpItemAppleJson>();

            mNameMapMyCard = new Dictionary<string, int>();
            mIdMapMyCard = new Dictionary<int, TopUpItemMyCardJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMapAndroid.Clear();
            mIdMapAndroid = gameData.TopUpItemAndroid;

            mNameMapApple.Clear();
            mIdMapApple = gameData.TopUpItemApple;

            mNameMapMyCard.Clear();
            mIdMapMyCard = gameData.TopUpItemMyCard;

            List<TopUpItemAndroidJson> TopUpItemsAndroid = new List<TopUpItemAndroidJson>();

            foreach (KeyValuePair<int, TopUpItemAndroidJson> entry in gameData.TopUpItemAndroid)
            {
                mNameMapAndroid.Add(entry.Value.name, entry.Key);

                TopUpItemsAndroid.Add(entry.Value);
            }

            _uiOrderedTopUpItemsAndroid = TopUpItemsAndroid.OrderBy(item => item.uiorder).ToList();

            List<TopUpItemAppleJson> TopUpItemsApple = new List<TopUpItemAppleJson>();

            foreach (KeyValuePair<int, TopUpItemAppleJson> entry in gameData.TopUpItemApple)
            {
                mNameMapApple.Add(entry.Value.name, entry.Key);

                TopUpItemsApple.Add(entry.Value);
            }

            _uiOrderedTopUpItemsApple = TopUpItemsApple.OrderBy(item => item.uiorder).ToList();

            List<TopUpItemMyCardJson> TopUpItemsMyCard = new List<TopUpItemMyCardJson>();

            foreach (KeyValuePair<int, TopUpItemMyCardJson> entry in gameData.TopUpItemMyCard)
            {
                mNameMapMyCard.Add(entry.Value.name, entry.Key);

                TopUpItemsMyCard.Add(entry.Value);
            }

            _uiOrderedTopUpItemsMyCard = TopUpItemsMyCard.OrderBy(item => item.uiorder).ToList();
        }

        public static List<string> GetAndroidProductIds()
        {
            List<string> productIds = mNameMapAndroid.Keys.ToList();

            return productIds;
        }

        public static List<string> GetAppleProductIds()
        {
            List<string> productIds = mNameMapApple.Keys.ToList();

            return productIds;
        }

        public static List<string> GetMyCardProductIds()
        {
            List<string> productIds = mNameMapMyCard.Keys.ToList();

            return productIds;
        }

        public static List<Dictionary<string, string>> GetUIOrderedTopUpItemsAndroid()
        {
            List<Dictionary<string, string>> uiOrderedTopUpItems = new List<Dictionary<string, string>>();

            foreach (TopUpItemAndroidJson item in _uiOrderedTopUpItemsAndroid)
            {
                Dictionary<string, string> itemDetails = new Dictionary<string, string>();
                itemDetails.Add("name", item.name);
                itemDetails.Add("icon", item.icon);
                itemDetails.Add("gold", item.gold.ToString());
                itemDetails.Add("doublebonus", item.doublebonus.ToString());
                itemDetails.Add("lockgold", item.lockgold.ToString());
                itemDetails.Add("vippoints", item.vippoints.ToString());
                itemDetails.Add("price", item.price.ToString());

                uiOrderedTopUpItems.Add(itemDetails);
            }

            return uiOrderedTopUpItems;
        }

        public static List<Dictionary<string, string>> GetUIOrderedTopUpItemsApple()
        {
            List<Dictionary<string, string>> uiOrderedTopUpItems = new List<Dictionary<string, string>>();

            foreach (TopUpItemAppleJson item in _uiOrderedTopUpItemsApple)
            {
                Dictionary<string, string> itemDetails = new Dictionary<string, string>();
                itemDetails.Add("name", item.name);
                itemDetails.Add("icon", item.icon);
                itemDetails.Add("gold", item.gold.ToString());
                itemDetails.Add("doublebonus", item.doublebonus.ToString());
                itemDetails.Add("lockgold", item.lockgold.ToString());
                itemDetails.Add("vippoints", item.vippoints.ToString());
                itemDetails.Add("price", item.price.ToString());

                uiOrderedTopUpItems.Add(itemDetails);
            }

            return uiOrderedTopUpItems;
        }

        public static List<Dictionary<string, string>> GetUIOrderedTopUpItemsMyCard()
        {
            List<Dictionary<string, string>> uiOrderedTopUpItems = new List<Dictionary<string, string>>();

            foreach (TopUpItemMyCardJson item in _uiOrderedTopUpItemsMyCard)
            {
                Dictionary<string, string> itemDetails = new Dictionary<string, string>();
                itemDetails.Add("name", item.name);
                itemDetails.Add("icon", item.icon);
                itemDetails.Add("gold", item.gold.ToString());
                itemDetails.Add("doublebonus", item.doublebonus.ToString());
                itemDetails.Add("lockgold", item.lockgold.ToString());
                itemDetails.Add("vippoints", item.vippoints.ToString());
                itemDetails.Add("price", item.price.ToString());

                uiOrderedTopUpItems.Add(itemDetails);
            }

            return uiOrderedTopUpItems;
        }

        public static TopUpItemAndroidJson GetTopUpItemAndroidByName(string name)
        {
            return mIdMapAndroid[mNameMapAndroid[name]];
        }

        public static TopUpItemAppleJson GetTopUpItemAppleByName(string name)
        {
            return mIdMapApple[mNameMapApple[name]];
        }

        public static TopUpItemMyCardJson GetTopUpItemMyCardByName(string name)
        {
            return mIdMapMyCard[mNameMapMyCard[name]];
        }
    }
}
