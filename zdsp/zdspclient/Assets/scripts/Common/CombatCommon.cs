namespace Zealot.Common
{
    public static class ZConst
    {        
        public readonly static short MANAMAX = 500;
        public readonly static byte RAGEMAX = 100;        
    }

    // QUACKKY TODO: Replaced by piliQ version. To be removed when safe
    //public class StoreItem
    //{
    //    public int id;
    //    public string productid;
    //    public float itemprice;
    //    public int quantity;
    //    public int rebategem;
    //    public bool onetimelimit;
    //    public int multiply;
    //    public bool onsale;
    //}

    //public static class StoreProduct
    //{
    //    static private Dictionary<byte, List<StoreItem>> mProducts;

    //    static StoreProduct()
    //    {
    //        mProducts = new Dictionary<byte, List<StoreItem>>();
    //        InitStoreProduct();
    //    }

    //    public static void InitStoreProduct()
    //    {
    //        List<StoreItem> androidProductlist = new List<StoreItem>();
    //        List<TopUpJson> androidlist = TopUpRepo.GetInfoByStoreType(StoreType.Android);
    //        if (androidlist.Count > 0)
    //        {
    //            for (int i = 0; i < androidlist.Count; i++)
    //            {
    //                TopUpJson data = androidlist[i];
    //                StoreItem newitem = new StoreItem();
    //                newitem.id = data.id;
    //                newitem.productid = data.productid;
    //                newitem.itemprice = data.price;
    //                newitem.quantity = data.diamond;
    //                newitem.rebategem = data.rebategem;
    //                newitem.onetimelimit = data.onetimelimit;
    //                newitem.multiply = data.multiple;
    //                newitem.onsale = data.onsale;
    //                androidProductlist.Add(newitem);
    //            }
    //            mProducts.Add((byte)StoreType.Android, androidProductlist);
    //        }

    //        List<StoreItem> iOSProductlist = new List<StoreItem>();
    //        List<TopUpJson> ioslist = TopUpRepo.GetInfoByStoreType(StoreType.IOS);
    //        if (ioslist.Count > 0)
    //        {
    //            for (int i = 0; i < ioslist.Count; i++)
    //            {
    //                TopUpJson data = androidlist[i];
    //                StoreItem newitem = new StoreItem();
    //                newitem.id = data.id;
    //                newitem.productid = data.productid;
    //                newitem.itemprice = data.price;
    //                newitem.quantity = data.diamond;
    //                newitem.rebategem = data.rebategem;
    //                newitem.onetimelimit = data.onetimelimit;
    //                newitem.multiply = data.multiple;
    //                newitem.onsale = data.onsale;
    //                androidProductlist.Add(newitem);
    //            }
    //            mProducts.Add((byte)StoreType.IOS, iOSProductlist);
    //        }
    //    }
        
    //    public static StoreItem GetStoreItemById(byte storetype, int id)
    //    {
    //        List<StoreItem> storeitems = mProducts[storetype];
    //        for (int i = 0; i < storeitems.Count; i++)
    //        {
    //            if (storeitems[i].id == id)
    //                return storeitems[i];
    //        }
    //        return null;
    //    }
    //}


    public enum UTNotificationType
    {
        Lottery = 2,
        TianZiZhan = 3,
    }
     
    public class SyncAttackResult
    {
        public int TargetPID { get; set; }
        public int RealDamage { get; set; }
        public bool IsHeal { get; set; }

        public SyncAttackResult(int targetpid, int dmg, bool isheal)
        {
            TargetPID = targetpid; 
            RealDamage = dmg;
            IsHeal = isheal;
        }
    }
    public class AttackResult
    {
        public int TargetPID { get; set; }
        public int RealDamage { get; set; }
        public int LabelNum { get; set; }

        public int Skillid { get; set; }
        public bool IsCritical { get; set; }
        public bool IsEvasion { get; set; }
        public bool IsBlocked { get; set; }

        public bool IsDot { get; set; }

        public bool IsHeal { get; set; } 

        public int AttackInfo
        {
            get
            {
                byte res = 0;
                if (IsCritical)
                    res |= 1;
                if (IsEvasion)
                    res |= 2;
                if (IsDot)
                    res |= 4;
                if (TargetInvincible)
                    res |= 8;
                if (IsHeal)
                    res |= 16;
                if (IsBlocked)
                    res |= 32;
                return res;
               
            }
            set
            {
                if ((value & 1) > 0)
                    IsCritical = true;
                if ((value & 2) > 0)
                {
                    IsEvasion = true;
                }
                if ((value & 4) > 0)
                {
                    IsDot = true;
                }
                if ((value & 8) > 0)
                {
                    TargetInvincible = true;
                }if((value &16) > 0)
                {
                    IsHeal = true;
                }
                if ((value & 32) > 0)
                    IsBlocked = true;
            }
        }

        public bool TargetInvincible { get; set; }
        public AttackResult(int targetpid, int dmg, bool invincible = false, int attackerpid=0, int count  =1)
        {
            TargetPID = targetpid; 
            RealDamage = dmg;
            TargetInvincible = invincible;
            LabelNum = count;
        }
    }

    public enum Trainingstep
    {
        SpawnedAndTalk,
        Talk1Start,
        GoToArea2, 
        DoPathFind,
        Talk2Start,
        KillMonster,
        GoToArea3, 
        Talk3Start,
        PlayCutScene1,
        EncounterBoss,
        //ShowFlashButton,
        //FlashSuccess,
        ShowJobButton,
        PlayCutScene2,
        ShowRGB,
        RGBSuccess,
        PlayCutScene3,
        Finished
    }

    public enum WardrobeRetCode
    {
        Failed,
        EquipSuccess,
        UnequipSuccess,
    }
}
