using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class CraftingRepo
    {
        public class CraftedItemInfo
        {
            public CraftingType type;
            public int SubId;
        }

        static Dictionary<int, CraftingJson> mAllCrafting;
        static Dictionary<int, CraftingCategoryJson> mAllCraftingCategory;
        static Dictionary<CraftingType, Dictionary<int, List<CraftingJson>>> mAllCraftingType;//the inner dictionary key contain CraftingCategoryJson id
        static Dictionary<CraftingType,string> mAllCraftingTypeName;
        static Dictionary<int, CraftedItemInfo> mAllCraftedItem;//key = itemid
        CraftingRepo()
        {
          
        }

        public static void Init(GameDBRepo gameData)
        {
            mAllCrafting = gameData.Crafting;
            mAllCraftingCategory = gameData.CraftingCategory;
            mAllCraftingType = new Dictionary<CraftingType, Dictionary<int, List<CraftingJson>>>();
            mAllCraftingTypeName = new Dictionary<CraftingType, string>();
            mAllCraftedItem = new Dictionary<int, CraftedItemInfo>();

            foreach (CraftingType type in Enum.GetValues(typeof(CraftingType)))
            {
                mAllCraftingType.Add(type, new Dictionary<int, List<CraftingJson>>());
            }

            foreach(var craft in gameData.Crafting)
            {
                int subid = craft.Value.subcategoryname;
                if(mAllCraftingType[craft.Value.categorytype].ContainsKey(subid) == false)
                {
                    mAllCraftingType[craft.Value.categorytype].Add(craft.Value.subcategoryname, new List<CraftingJson> { craft.Value });
                }
                else
                {
                    mAllCraftingType[craft.Value.categorytype][craft.Value.subcategoryname].Add(craft.Value);
                }

                if (mAllCraftingTypeName.ContainsKey(craft.Value.categorytype) == false)
                {
                    mAllCraftingTypeName.Add(craft.Value.categorytype, craft.Value.localizedname);
                }

                string[] allneededitemid = craft.Value.itemid.Split(',');
                //for(int i=0;i< allneededitemid.Length;i++)
                //{
                //    int itemid = -1;
                //    if(int.TryParse(allneededitemid[i], out itemid) == true)
                //    {
                //        SocketGemItemJson socketgem = GameRepo.ItemFactory.GetSocketGemItemById(itemid);
                //        if(socketgem != null)
                //        {
                //            CraftedItemInfo info = new CraftedItemInfo();
                //            info.type = craft.Value.categorytype;
                //            info.SubId = subid;
                //            mAllCraftedItem.Add(itemid, info);
                //            break;
                //        }
                //    }
                //}
                //if (mAllCraftedItem.ContainsKey(craft.Value.crafteditemid) == true)
                //    throw new Exception("crafting item id duplicate");
                //else
                //{
                //    CraftedItemInfo info = new CraftedItemInfo();
                //    info.type = craft.Value.categorytype;
                //    info.SubId = subid;
                //    mAllCraftedItem.Add(craft.Value.crafteditemid, info);
                //    //GetSocketGemItemById
                //}
            }

            foreach (CraftingType type in Enum.GetValues(typeof(CraftingType)))//sort
            {
                for(int i=0;i< mAllCraftingType[type].Count;i++)
                {
                    int key = mAllCraftingType[type].ElementAt(i).Key;
                    mAllCraftingType[type][key] = mAllCraftingType[type][key].OrderBy(o => o.ordering).ToList();
                }
            }
        }

      
        /// <summary>
        /// the list is sorted according to the ordering
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<CraftingJson> GetAllCraftingByTypeAndSubId(CraftingType type,int subid)
        {
            List<CraftingJson> result = null;
            Dictionary<int, List<CraftingJson>> allcrafting = null;
            mAllCraftingType.TryGetValue(type, out allcrafting);
            if(allcrafting != null)
            {
                allcrafting.TryGetValue(subid, out result);
            }
            return result;
        }

        public static List<CraftingCategoryJson> GetAllSubCatergoryByType(CraftingType type)
        {
            Dictionary<int, List<CraftingJson>> allcrafting = null;
            mAllCraftingType.TryGetValue(type, out allcrafting);
            if(allcrafting != null)
            {
                List<CraftingCategoryJson> result = new List<CraftingCategoryJson>();
                foreach (var craft in allcrafting)
                {
                    result.Add(GetCraftingCategoryJsonById(craft.Key));
                }
                return result;
            }

            return null;
        }

        /// <summary>
        /// return the crafting type localized name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCraftingTypeNameByType(CraftingType type)
        {
            string result = "";
            mAllCraftingTypeName.TryGetValue(type, out result);
            return result;
        }
        public static CraftingJson GetCraftingJsonById(int id)
        {
            CraftingJson result = null;
            mAllCrafting.TryGetValue(id, out result);
            return result;
        }

        public static CraftingCategoryJson GetCraftingCategoryJsonById(int id)
        {
            CraftingCategoryJson result = null;
            mAllCraftingCategory.TryGetValue(id, out result);
            return result;
        }

        public static CraftedItemInfo GetSubIdByItemId(int itemid)
        {
            CraftedItemInfo result = null;
            mAllCraftedItem.TryGetValue(itemid, out result);
            return result;
        }

        public static Dictionary<int, CraftingJson> GetAllCraftingRecipe()
        {
            return mAllCrafting;
        }
    }
}
