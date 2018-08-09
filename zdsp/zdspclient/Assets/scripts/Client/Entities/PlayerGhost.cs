using UnityEngine;
using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.Actions;
using Zealot.Common.Datablock;
using Zealot.Client.Actions;
using Zealot.Bot;
using Zealot.Repository;
using System.Collections;

namespace Zealot.Client.Entities
{
    public class PlayerGhost : ActorGhost
    {
        // Stats
        public PlayerSynStats PlayerSynStats { get; set; }
        public SecondaryStats SecondaryStats { get; set; }
        public InventoryStats[] InventoryStats { get; private set; }
        public EquipmentStats EquipmentStats { get; set; }
        public ItemHotbarStats ItemHotbarStats { get; set; }
        public SkillSynStats SkillStats { get; set; }
        public QuestSynStatsClient QuestStats { get; set; }
        public RealmStats RealmStats { get; set; }
        public LocalCombatStats LocalCombatStats { get; set; }
        public LocalSkillPassiveStats LocalSkillPassiveStats { get; set; } //overwrite Actor
        public BuffTimeStats BuffTimeStats { get; set; }
        public SocialStats SocialStats { get; set; }
        public WelfareStats WelfareStats { get; set; }
        public SevenDaysStats SevenDaysStats { get; set; }
        public QuestExtraRewardsStats QuestExtraRewardsStats { get; set; }
        public LotteryInfoStats LotteryInfoStats { get; set; }
        public HeroStatsClient HeroStats { get; private set; }
        public PowerUpStats PowerUpStats { get; set; }
        public DestinyClueStatsClient DestinyClueStats { get; set; }

        // Shared stats 
        public PartyStatsClient PartyStats { get; set; }
        //public GuildStatsClient GuildStats { get; set; }
        public DungeonObjectiveStats DungeonObjectiveStats { get; set; }
        public ExchangeShopSynStats ExchangeShopSynStats { get; private set; }
        public PortraitDataStats PortraitDataStats { get; private set; }

        // Controllers
        public ItemInventoryController clientItemInvCtrl;
        public PowerUpController clientPowerUpCtrl;
        public QuestClientController QuestController { get; private set; }
        public DestinyClueClientController DestinyClueController { get; private set; }
        private BotController mBotController;
        public BotController Bot { get { return mBotController; } }

        public GameTimer mArenaRewardCD = null;
        public DateTime mArenaLastRewardDT;

        /// <summary>
        /// use for non local player only
        /// </summary>
        public SystemSwitchData mSysSwitch;
        public EquipmentInventoryData mEquipmentInvData;
        public Gender mGender;

        public PlayerGhost() : base()
        {
            this.EntityType = EntityType.PlayerGhost;
        }

        public JobType GetJobSect()
        {
            return (JobType)PlayerSynStats.jobsect;
        }

        public int GetHealth()
        {
            return LocalCombatStats.Health;
        }

        public int GetHealthMax()
        {
            return LocalCombatStats.HealthMax;
        }

        public bool IsGuildLeader()
        {
            return SecondaryStats.guildRank == (byte)GuildRankType.Leader;
        }

        public bool IsCurrencyEnough(CurrencyType currencyType, int amount, bool allowbind = true)
        {
            if (currencyType == CurrencyType.None) return true;
            long curval = 0;
            switch (currencyType)
            {
                case CurrencyType.Money:
                    curval = SecondaryStats.money;
                    break;
                case CurrencyType.Gold:
                    if (allowbind)
                        return SecondaryStats.gold >= amount - SecondaryStats.bindgold;
                    else
                        return SecondaryStats.gold >= amount;
                case CurrencyType.LockGold:
                    if (allowbind)
                        return SecondaryStats.bindgold >= amount - SecondaryStats.gold;
                    else
                        return SecondaryStats.bindgold >= amount;
                case CurrencyType.HonorValue:
                    curval = SecondaryStats.honor;
                    break;
                case CurrencyType.GuildContribution:
                    curval = SecondaryStats.contribute;
                    break;
                case CurrencyType.GuildGold:
                    //if (GuildStats != null)
                    //    curval = GuildStats.guildGold;
                    break;

                case CurrencyType.BattleCoin:
                    curval = SecondaryStats.battlecoin;
                    break;
            }
            return curval >= amount;
        }

        public long GetCurrencyAmount(CurrencyType currencyType)
        {
            long currencyvalue = 0;
            switch (currencyType)
            {
                case CurrencyType.Money:
                    currencyvalue = SecondaryStats.money;
                    break;
                case CurrencyType.GuildContribution:
                    currencyvalue = SecondaryStats.contribute;
                    break;
                case CurrencyType.GuildGold:
                    //if (GuildStats != null)
                    //    currencyvalue = GuildStats.guildGold;
                    break;
                case CurrencyType.Gold:
                    currencyvalue = SecondaryStats.gold;
                    break;
                case CurrencyType.LockGold:
                    currencyvalue = SecondaryStats.bindgold;
                    break;
                case CurrencyType.HonorValue:
                    currencyvalue = SecondaryStats.honor;
                    break;
                case CurrencyType.BattleCoin:
                    currencyvalue = SecondaryStats.battlecoin;
                    break;
                default:
                    break;
            }
            return currencyvalue;
        }

        public bool IsFeatureUnlocked(WindowType windowType)
        {
            int minLvl = 1;
            switch (windowType)
            {
                //case WindowType.Equipment:
                //    minLvl = GameConstantRepo.GetConstantInt("Equipment_UnlockLvl", 1);
                //    break;
                //case WindowType.Friends:
                //    minLvl = GameConstantRepo.GetConstantInt("Friends_UnlockLvl");
                //    break;
                //case WindowType.Talent:
                //    minLvl = GameConstantRepo.GetConstantInt("Talent_UnlockLvl", 1);
                //    break;
                //case WindowType.Dungeon:
                //    minLvl = GameConstantRepo.GetConstantInt("Dungeon_UnlockLvl");
                //    break;
                //case WindowType.HeroFight:
                //case WindowType.PersistentGuild:
                //    minLvl = GuildRepo.GetValue("CreateGuildMinLevel");
                //    break;
                //case WindowType.Pet:
                //    minLvl = GameConstantRepo.GetConstantInt("Pet_UnlockLvl");
                //    break;
                //case WindowType.OfflineExp:
                //    minLvl = GameConstantRepo.GetConstantInt("OfflineExp_UnlockLvl", 1);
                //    break;
                //case WindowType.MartialArts:
                //    minLvl = GameConstantRepo.GetConstantInt("MartialArts_UnlockLevel", 1);
                //    break;
                //case WindowType.Arena:
                //    minLvl = GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1);
                //    break;
                //case WindowType.SkillComboRecommend:
                //    minLvl = GameConstantRepo.GetConstantInt("SkillComboRecommend_UnlockLvl");
                //    break;
                //case WindowType.NewSkillCombo:
                //    minLvl = GameConstantRepo.GetConstantInt("SkillCombo_UnlockLvl");
                //    break;
                //case WindowType.NewHeroBook:
                //    minLvl = GameConstantRepo.GetConstantInt("Herobook_UnlockLvl");
                //    break;
                //case WindowType.QuestExtraRewards:
                //    minLvl = GameConstantRepo.GetConstantInt("QER_UnlockLvl");
                //    break;
                //case WindowType.TutorialList:
                //    minLvl = GameConstantRepo.GetConstantInt("TutorialList_UnlockLvl");
                //    break;
                default:
                    break;
            }

            if (PlayerSynStats.Level < minLvl)
            {
                //ClientUtils.ShowFeatureLocked(minLvl);
                return false;
            }
            return true;
        }

        public bool IsSystemOpen(string sysname)
        {
            if (!mSysSwitch.IsOpen(sysname))
            {
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("System_Switch_01", null));
                return false;
            }
            return true;
        }

        public void OnSecondaryStatsChanged(string field, object value, object oldvalue)
        {
            switch (field)
            {
                case "UnlockedSlotCount": // Unlock new slot
                    clientItemInvCtrl.itemInvData.UnlockedSlotCount = (int)value;
                    break;
                case "experience":
                    if (IsLocal)
                    {
                        GameObject obj = UIManager.GetWidget(HUDWidgetType.PlayerPortrait);
                        HUD_PlayerPortrait hpp = obj.GetComponent<HUD_PlayerPortrait>();
                        hpp.UpdateExp();
                    }
                    break;
                case "money":
                    GameObject windowObj = UIManager.GetWindowGameObject(WindowType.Inventory);
                    if (windowObj.activeInHierarchy)
                        windowObj.GetComponent<UI_Inventory>().UpdateCurrencyAmount(CurrencyType.Money);
                    break;
                case "bindgold":
                case "gold":
                    break;
                case "vippoints":
                    break;
                case "guildId":
                    if ((int)value == 0 && (int)oldvalue > 0)
                    {
                        RemoveLocalObject(LOTYPE.GuildStats);
                        UIManager.AlertManager2.SetAlert(AlertType.GuildInfo, false);
                        UIManager.AlertManager2.SetAlert(AlertType.GuildTech, false);
                        UIManager.AlertManager2.SetAlert(AlertType.GuildActivity, false);
                        UIManager.AlertManager2.SetAlert(AlertType.GuildSMBoss, false);
                        UIManager.AlertManager2.SetAlert(AlertType.GuildWishingPool, false);
                        StopTimer(guildquestTimer);
                    }
                    break;
                case "guildRank":
                    break;
                case "GuildSMBossEntry":
                    break;
                case "GuildDreamHouseUsed":
                    break;
                case "GuildDreamHouseCollected":
                    break;
                case "guildDonateDot":
                    UIManager.AlertManager2.SetAlert(AlertType.GuildWishingPool, GameInfo.gLocalPlayer.SecondaryStats.guildDonateDot);
                    break;
                case "BattleTime":
                    GameObject battletimeWidget = UIManager.GetWidget(HUDWidgetType.BattleTime);
                    if(battletimeWidget != null)
                    {
                        HUD_BattleTime battleTimeHUD = battletimeWidget.GetComponent<HUD_BattleTime>();
                        battleTimeHUD.UpdateBattleTime((int)value);
                    }
                    break;
            }
            if (!SecondaryStats.IsNewlyAdded)
                NotifyCurrencyIncrement(field, value, oldvalue);
        }

        public void OnExchangeShopStatsChanged(string field, object value, object oldvalue)
        {
        }

        public void OnPlayerStatsChanged(string field, object value, object oldvalue)
        {
            switch (field)
            {
                case "Party":
                    if (IsLocal)
                    {
                        if ((int)oldvalue == 0 && (int)value > 0)
                            OnJoinParty();
                        else if ((int)value == 0 && (int)oldvalue > 0)
                            OnLeaveParty();
                    }
                    //Set correct headlabel
                    SetHeadLabel();
                    break;
                case "invincible":
                    //Debug.Log("invicible value :" + PlayerStats.invincible.ToString());
                    if (PlayerStats.invincible)
                        PlaySEEffect("invincible");
                    else
                        StopEffect("invincible");
                    break;
                case "silenceAttackBuff":
                case "silenceDefendBuff":
                    if (IsLocal)
                        UpdateSkillLocks();
                    break;
                case "positiveVisualSE":
                case "negativeVisualSE":
                case "VisualEffectTypes":
                case "ElementalVisualSE":
                    HandleSideEffectVisuals(field, value);
                    break;
                case "havebuff":
                case "havedebuff":
                case "havedot":
                case "havecontrol":
                case "havehot":
                    HandleBuffStatus(field, value);
                    break;
                case "Level":
                    if (IsLocal)
                    {
                        GameObject obj = UIManager.GetWidget(HUDWidgetType.PlayerPortrait);
                        HUD_PlayerPortrait hpp = obj.GetComponent<HUD_PlayerPortrait>();
                        hpp.UpdateLevel();

                        if ((int)oldvalue != -1)
                            PlayEffect("", "levelup", null, 4f, Position, this);
                    }
                    break;
                case "guildName":
                    if (HeadLabel != null)
                        HeadLabel.mPlayerLabel.GuildName = (string)value;
                    break;
                case "DisplayHp":
                    float hp = (float)value;
                    if (HeadLabel != null)
                        HeadLabel.mPlayerLabel.HPf = hp;
                    if (this == GameInfo.gSelectedEntity)
                        GameInfo.gCombat.UpdateSelectedEntityHealth(hp);
                    break;
                case "jobsect":
                    if (!PlayerStats.IsNewlyAdded)
                    {
                        AvatarController controller = AnimObj.GetComponent<AvatarController>();
                        controller.InitAvatar(mEquipmentInvData, (JobType)PlayerSynStats.jobsect, mGender);
                    }
                    break;
                case "QuestCompanionId":
                    UpdatePlayerCompanion();
                    break;
                default:
                    break;
            }
        }

        private void OnPlayerStatsLocalObjectChanged()
        {
            GameInfo.OnPlayerStatsChangedSpawned();
        }

        public void OnModifyCooldown(int[] listIndex, float perct)
        {
            if (IsLocal)
            {
                HUD_Skills hudskill = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
                foreach (int index in listIndex)
                    hudskill.ChangeCooldown(index, perct);
            }
        }

        private UI_ComboHit ui_combohit;
        private long comboTimer = 0;
        public void OnComboHitChanged(int count)
        {
            if (IsLocal)
            {
                if (ui_combohit == null)
                {
                    //GameObject widget = UIManager.GetWidget(HUDWidgetType.ComboHit);
                    //if (widget != null)
                    //    ui_combohit = widget.GetComponent<UI_ComboHit>();
                }
                if (ui_combohit != null && count > 0)
                {
                    ui_combohit.PlayNumber(count);
                    comboTimer = 0;
                }
            }
        }

        public void OnStopCooldown(int[] listIndex)
        {
            if (IsLocal)
            {
                HUD_Skills hudskill = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
                foreach (int index in listIndex)
                    hudskill.StopCooldown(index);
            }
        }

        public void OnDead()
        {
            if (Bot.Enabled)
            {
                HUD_Skills hudskill = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
                //hudskill.botToggle.;
                Bot.StopBot();
                hudskill.AutoCombatToggle.isOn = false;
                if (PartyStats != null)
                    PartyStats.StopFollowTarget();
            }
            if (IsLocal)
            {
                ActionInterupted();
            }
        }

        public void OnSkillLocalObjectChanged()
        {
            //Debug.Log("new skilllocal object " + SkillStats.ToString());
            GameObject widget = UIManager.GetWidget(HUDWidgetType.SkillButtons);
            if (widget == null)
                return;
            HUD_Skills hudskill = widget.GetComponent<HUD_Skills>();
            if (hudskill == null)
                return;

            //for(int i = 0; i < 9; ++i)
            //{
            //    SkillData data = SkillRepo.GetSkill((int)SkillStats.SkillInv[i]);
            //    if (data != null)
            //    {
            //        string icon = data.skillgroupJson.icon;
            //        hudskill.SetSkillImage(i, ClientUtils.LoadIcon(icon));
            //    }
                
            //}
            
            //todo: update skill icon 
            //int[] skillslist = new int[4] { SkillStats.JobskillAttackSId, SkillStats.RedHeroCardSkillAttackSId, SkillStats.GreenHeroCardSkillAttackSId, SkillStats.BlueHeroCardSkillAttackSId };
            //for (int i = 0; i < 4; i++)
            //{
            //    SkillData data = SkillRepo.GetSkillByGroupID(skillslist[i]);//take only primary id as secondary id is the support of primary id skill
            //    if (data != null)
            //    {
            //        string iconpath = data.skillgroupJson.icon;
            //        //hudskill.SetSkillImage(i, ClientUtils.LoadIcon(iconpath));
            //    }
            //    else
            //    {
            //        hudskill.SetSkillImage(i, null);
            //    }
            //}
        }

        public void OnSkillStatsChanged(string field, object value, object oldvalue)
        {
            //Debug.LogFormat("OnSkillStatsChanged, field: {0}, value: {1}, oldvalue: {2}", field, value, oldvalue);
            //Debug.Log(@"
            //if (_ui_skill == null && _ui_skill.isActiveAndEnabled)
            //    return;
            //");
            if (field.Contains("SId"))
            {
                int skillId = (int)value; 
                if (field == "basicAttack1SId")
                {
                    //Debug.Log("basicAttack1SId changed: " + value);                   
                    GameInfo.gBasicAttackState.mBasicAttackIDs[0] = skillId;
                }
                else if (field == "basicAttack2SId")
                {                
                    GameInfo.gBasicAttackState.mBasicAttackIDs[1] = skillId;
                }
                else if (field == "basicAttack3SId")
                {                    
                    GameInfo.gBasicAttackState.mBasicAttackIDs[2] = skillId;
                }           
            }
        }

        public SkillData[] comboSkills = new SkillData[3];

        public void OnRealmStatsChanged(string field, object value, object oldvalue)
        {
        }

        public void OnInventoryStatsCollectionChanged(LOTYPE lotype, string field, byte idx, object value)
        {
            IInventoryItem item = GameRepo.ItemFactory.GetItemFromCode(value);
            int slotid = (lotype - LOTYPE.InventoryStats) * (int)InventorySlot.COLLECTION_SIZE + idx;
            clientItemInvCtrl.UpdateItemInv(slotid, item); 

            Equipment equipment = GameRepo.ItemFactory.GetItemFromCode(value) as Equipment;
            if(equipment != null)
            {
                GameObject uiEquipUpgradeObj = UIManager.GetWindowGameObject(WindowType.EquipUpgrade);
                if(uiEquipUpgradeObj != null && uiEquipUpgradeObj.activeSelf == true)
                {
                    UI_EquipmentUpgrade uiEquipUpgrade = uiEquipUpgradeObj.GetComponent<UI_EquipmentUpgrade>();
                    if(uiEquipUpgrade != null)
                    {
                        uiEquipUpgrade.Refresh();
                    }
                }

                GameObject uiEquipReformObj = UIManager.GetWindowGameObject(WindowType.EquipReform);
                if (uiEquipReformObj != null && uiEquipReformObj.activeSelf == true)
                {
                    UI_EquipmentReform uiEquipReform = uiEquipReformObj.GetComponent<UI_EquipmentReform>();
                    if (uiEquipReform != null)
                    {
                        uiEquipReform.Refresh();
                    }
                }
            }
        }

        public void OnInventoryStatsLocalObjectChanged()
        {
            GameObject windowObj = UIManager.GetWindowGameObject(WindowType.Inventory);
            if (windowObj.activeInHierarchy)
                windowObj.GetComponent<UI_Inventory>().RefreshRight(this);
        }

        public void OnEquipmentStatsCollectionChanged(string field, byte idx, object value)
        {
            if (field == "EquipInventory")
            {                
                if (value != null)
                    mEquipmentInvData.SetEquipmentToSlot(idx, GameRepo.ItemFactory.GetItemFromCode(value) as Equipment);
                else
                    mEquipmentInvData.SetEquipmentToSlot(idx, null);
                if (idx == (int)EquipmentSlot.Weapon)
                {
                    PartsType _prevWeaponTypeUsed = WeaponTypeUsed;
                    WeaponTypeUsed = value == null ? PartsType.Blade : mEquipmentInvData.GetEquipmentBySlotId(idx).EquipmentJson.partstype;
                    if (_prevWeaponTypeUsed != WeaponTypeUsed && mAction != null)
                    {
                        switch (mAction.mdbCommand.GetActionType())
                        {
                            case ACTIONTYPE.APPROACH:
                            case ACTIONTYPE.APPROACH_PATHFIND:
                            case ACTIONTYPE.WALK:
                                PlayEffect(GetRunningAnimation());
                                break;
                            case ACTIONTYPE.IDLE:
                                string anim = GetStandbyAnimation();
                                if (anim != "")
                                    PlayEffect(anim);
                                break;
                        }
                    }
                }

                GameObject uiEquipUpgradeObj = UIManager.GetWindowGameObject(WindowType.EquipUpgrade);
                if (uiEquipUpgradeObj != null && uiEquipUpgradeObj.activeSelf == true)
                {
                    UI_EquipmentUpgrade uiEquipUpgrade = uiEquipUpgradeObj.GetComponent<UI_EquipmentUpgrade>();
                    if (uiEquipUpgrade != null)
                    {
                        uiEquipUpgrade.Refresh();
                    }
                }
            }
            else if (field == "FashionInventory")
            {
                if (value != null)
                    mEquipmentInvData.SetFashionToSlot(idx, GameRepo.ItemFactory.GetItemFromCode(value) as Equipment);
                else
                    mEquipmentInvData.SetFashionToSlot(idx, null);
            }
            GameObject _inventoryWindow = UIManager.GetWindowGameObject(WindowType.Inventory);
            if (_inventoryWindow != null && _inventoryWindow.activeSelf)
                _inventoryWindow.GetComponent<UI_Inventory>().RefreshLeft(this);
        }

        public void OnEquipmentStatsLocalObjectChanged()
        {
            mEquipmentInvData.HideHelm = EquipmentStats.HideHelm;
            AvatarController controller = AnimObj.GetComponent<AvatarController>();
            controller.InitAvatar(mEquipmentInvData, (JobType)PlayerSynStats.jobsect, mGender);
        }

        public void OnItemHotbarCollectionChanged(string field, byte idx, object value)
        {
            if (field == "ItemHotbar" && value != null)
            {
                GameObject windowObj = UIManager.GetWindowGameObject(WindowType.Inventory);
                if (windowObj.activeInHierarchy)
                {
                    UI_Inventory_QuickSlot uiInvQuickSlot = windowObj.GetComponent<UI_Inventory>().InvQuickSlot;
                    if (uiInvQuickSlot.gameObject.activeInHierarchy)
                        uiInvQuickSlot.SetItemToSlot(idx, (int)value);
                }

                GameObject widgetObj = UIManager.GetWidget(HUDWidgetType.ItemHotbar);
                if (widgetObj.activeInHierarchy)
                    widgetObj.GetComponent<HUD_ItemHotbar>().SetItemToSlot(idx, (int)value);
            }
        }

        public int GetAccumulatedLevel()
        {
            return PlayerSynStats.Level;
        }

        public override int GetDisplayLevel()
        {
            return PlayerSynStats.Level;
        }

        public void OnLocalSkillPassiveStatsChanged()
        {
#if UNITY_EDITOR
            //TODO: display it in the ui or somewhere. 
            //string str = string.Format("{0} = {1}", field, value);
            //Debug.Log(str);
#endif
        }

        public void OnLocalCombatStatsChanged(string field, object value, object oldvalue)
        {
            //Debug.Log("OnLocalCombatStatsChanged field: " + field + " value: " + value);
            switch (field)
            {
                case "Health":
                case "Mana":
                    if (IsLocal)
                    {
                        GameObject obj = UIManager.GetWidget(HUDWidgetType.PlayerPortrait);
                        HUD_PlayerPortrait hpp = obj.GetComponent<HUD_PlayerPortrait>();
                        hpp.UpdateHPMPBar();
                    }
                    break;
                case "IsInCombat":
                    if(IsLocal)
                    {
                        if(!IsMoving())
                        PlayEffect(GetStandbyAnimation());
                    }
                    break;
            }
        }

        private void OnCombatStatsLocalObjectChanged()
        {
            GameInfo.OnCombatStatsChangedSpawned();
        }

        public bool IsSkillSilenced(SkillData skilldata)
        {
            if (IsSilenced())
                return true;
            if (PlayerStats.Silence) // as long as silenced
                return true;
            //if (PlayerStats.silenceAttackBuff)
            //{
            //    foreach (SideEffectJson sej in skilldata.mainskills)
            //    {
            //        if (sej.effecttype == EffectType.StatsAttack_Accuracy ||
            //            //sej.effecttype == EffectType.StatsAttack_Attack ||
            //            sej.effecttype == EffectType.StatsAttack_Critical ||
            //            sej.effecttype == EffectType.StatsAttack_CriticalDamage)
            //        {
            //            return true;
            //        }
            //    }
            //}
            //if (PlayerStats.silenceDefendBuff)
            //{
            //    foreach (SideEffectJson sej in skilldata.mainskills)
            //    {
            //        if (sej.effecttype == EffectType.StatsDefence_Armor ||
            //            sej.effecttype == EffectType.StatsDefence_CoCritical ||
            //            sej.effecttype == EffectType.StatsDefence_Evasion )
            //            //sej.effecttype == EffectType.StatsDefence_CoCriticalDamage)
            //        {
            //            return true;
            //        }
            //    }
            //}
            return false;
        }

        private void UpdateSkillLocks()
        {
            if (LocalCombatStats == null || PlayerStats == null)
                return;
            int[] list = new int[] { };
            //bool lockBasic = false;
            //if (LocalCombatStats.Stun || LocalCombatStats.Silence || LocalCombatStats.Disarmed)
            //{
            //    lockBasic = LocalCombatStats.Stun || LocalCombatStats.Disarmed;
            //    list = new int[] { 0, 1, 2, 3, 4 };//get index from ui. 0-job, 1,2,3- rgb, 4-dodge
            //}
            //else
            {
                List<int> lockedlist = new List<int>();
                int[] skilllist = new int[] { 0, 1, 2, 3 };//job, r, g, b;
                foreach (int i in skilllist)
                {
                    int skillid = 0;
                    if (i == 0)
                    {
                        skillid = SkillStats.JobskillAttackSId;
                    }
                    //else if (i == 1)
                    //{
                    //    skillid = SkillStats.RedHeroCardSkillAttackSId;
                    //}
                    //else if (i == 2)
                    //{
                    //    skillid = SkillStats.GreenHeroCardSkillAttackSId;
                    //}
                    //else if (i == 3)
                    //{
                    //    skillid = SkillStats.BlueHeroCardSkillAttackSId;
                    //}
                    SkillData sdata = SkillRepo.GetSkillByGroupID(skillid);
                    if (sdata != null && IsSkillSilenced(sdata))
                    {
                        lockedlist.Add(i);
                    }
                }
                list = new int[lockedlist.Count];
                for (int j = 0; j < lockedlist.Count; j++)
                {
                    list[j] = lockedlist[j];
                }
            }
            GameObject widget = UIManager.GetWidget(HUDWidgetType.SkillButtons);
            if (widget != null)
            {
               // HUD_Skills hudskill = widget.GetComponent<HUD_Skills>();
                //hudskill.UpdateLockStatus(list, lockBasic);
            }         
        }

        public void OnBuffTimeStatsChanged(string field, byte idx, object value)//value is the side effect id
        {
            //if (field == "Positives")
            //{
            //    SideEffectJson dupSideEffect = SideEffectRepo.GetSideEffect((int)value);
            //    if (dupSideEffect != null)
            //    {
            //        //if (string.IsNullOrEmpty(dupSideEffect.icon) == false)//this check for all the side effect that do not contain icon
            //        {
            //            HUD_Buff.mybuffInstance.SetBuffAmount(BuffTimeStats);
            //        }
            //    }
            //    else//the side effect duration time out
            //    {
            //        HUD_Buff.mybuffInstance.SetBuffAmount(BuffTimeStats);
            //    }
            //}
#if UNITY_EDITOR
            string bufflist = "";
            foreach (int id in BuffTimeStats.Positives)
            {
                bufflist += id.ToString() + " ";
            }
            bufflist += "\n";
            foreach (int id in BuffTimeStats.Negatives)
            {
                bufflist += id.ToString() + " ";
            }
            bufflist += "\n";
            foreach (int id in BuffTimeStats.Persistents)
            {
                bufflist += id.ToString() + " ";
            }
            bufflist += "\n";
            foreach (int dur in BuffTimeStats.PersistentsDur)
            {
                bufflist += dur.ToString() + " ";
            }
            bufflist += "\n";

            Debug.Log(bufflist);
#endif
        }

        public void OnSevenDaysStatsChanged()
        {
        }

        public void OnWelfareStatsChanged()
        {
        }

        public void OnWelfareStatsValueChanged(string field, object value, object oldvalue)
        {
        }

        public void OnPowerUpStatsChanged()
        {
            clientPowerUpCtrl.InitFromStats(PowerUpStats);

            // Refresh
            GameObject uiPowerUpObj = UIManager.GetWindowGameObject(WindowType.Inventory);
            if (uiPowerUpObj != null)
            {
                UI_CharacterPowerup_Manager uiPowerUp = uiPowerUpObj.GetComponentInChildren<UI_CharacterPowerup_Manager>();
                if (uiPowerUp != null)
                {
                    uiPowerUp.CS_CharacterToggle.CS_MG_ToggleSelected(UI_CharacterPowerup_Manager.CP_State);
                }
            }
        }

        public void OnPowerUpStatsValueChanged(string field, object value, object oldvalue)
        {
        }

        public void OnPowerUpStatsCollectionChanged(string field, byte idx, object value)
        {
            //clientPowerUpCtrl.InitFromStats(PowerUpStats);

            clientPowerUpCtrl.PowerUpInventory.powerUpSlots[idx] = (int)value;

            // Refresh
            GameObject uiPowerUpObj = UIManager.GetWindowGameObject(WindowType.Inventory);
            if(uiPowerUpObj != null)
            {
                UI_CharacterPowerup_Manager uiPowerUp = uiPowerUpObj.GetComponentInChildren<UI_CharacterPowerup_Manager>();
                if(uiPowerUp != null)
                {
                    uiPowerUp.CS_CharacterToggle.CS_MG_ToggleSelected(UI_CharacterPowerup_Manager.CP_State);
                }
            }
            //UI_CharacterPowerup_Manager.CharacterToggle.CS_MG_ToggleSelected(UI_CharacterPowerup_Manager.CP_State);
        }

        public void OnLotteryInfoStatsChanged(int stat_index)
        {
        }

        #region Hero
        private void OnHeroStatsCollectionChanged(string field, byte idx, object value)
        {
            if (HeroStats.IsNewlyAdded)
                return;

            switch (field)
            {
                case "heroes":
                    HeroStats.UpdateHeroesList(idx, (string)value);
                    break;
            }
        }
        private void OnHeroStatsChanged(string field, object value, object oldvalue)
        {
            if (HeroStats.IsNewlyAdded)
                return;

            switch (field)
            {
                case "SummonedHeroId":
                    HeroStats.OnSummonedHeroChanged();
                    break;
                case "Explorations":
                    HeroStats.UpdateExplorations();
                    break;
                case "Explored":
                    HeroStats.UpdateExploredMaps();
                    break;
            }
        }
        #endregion

        #region Party
        private void OnPartyStatsCollectionChanged(string field, byte idx, object value)
        {
            if (PartyStats.IsNewlyAdded)
                return;

            switch (field)
            {
                case "members":
                    PartyStats.UpdateMemberList(idx, (string)value);
                    break;
                case "requests":
                    PartyStats.UpdateRequestList(idx, (string)value);
                    break;
            }
        }
        private void OnPartyStatsChanged(string field, object value, object oldvalue)
        {
            if (PartyStats.IsNewlyAdded)
                return;

            switch (field)
            {
                case "leader":
                    if ((string)oldvalue != (string)value)
                        PartyStats.UpdatePartyLeader((string)value);
                    break;
                case "partySetting":
                    PartyStats.UpdatePartySetting((string)value);
                    break;
                default:
                    break;
            }
        }

        public bool IsInParty()
        {
            return PlayerSynStats.Party > 0 && PartyStats != null;
        }

        public bool IsPartyLeader()
        {
            if (PartyStats != null)
                return PartyStats.IsLeader(Name);
            return false;
        }

        public override int GetParty()
        {
            if (PlayerSynStats != null)
                return PlayerSynStats.Party;
            return -1;
        }

        private void OnJoinParty()
        {
            GameObject windowObj = UIManager.GetWindowGameObject(WindowType.Party);
            if (windowObj.activeInHierarchy)
                windowObj.GetComponent<UI_Party>().SetUp(true);
        }

        private void OnLeaveParty()
        {
            PartyStats.OnLeaveParty();
            RemoveLocalObject(LOTYPE.PartyStats);
        }

        #endregion

        public void OnLotteryInfoStatsCollectionChange(string field, byte idx, object value)
        {
            if (LotteryInfoStats.IsNewlyAdded)
                return;
        }

        public void OnGuildStatsCollectionChanged(string field, byte idx, object value)
        {
            //if (GuildStats.IsNewlyAdded)
            //    return;
            //switch (field)
            //{
            //    case "members":
            //        GuildStats.UpdateMemberList(idx, (string)value);
            //        break;
            //    case "memberRequests":
            //        GuildStats.UpdateRequestList(idx, (string)value);
            //        break;
            //}
        }

        public void OnGuildStatsChanged(string field, object value, object oldvalue)
        {
            //GameObject window_guildInfo = UIManager.GetWindowGameObject(WindowType.GuildInfo);
            //switch (field)
            //{

            //    case "SMBossDmgDone":
            //        if (!GuildStats.IsNewlyAdded)
            //            GuildStats.UpdateGuildSMBossRedDot();
            //        break;
            //}
        }

        public void OnDungeonObjectiveCollectionChanged(string field, byte idx, object value)
        {
        }

        public void OnTongbaoCostBuffInfoChanged(string field, object value, object oldvalue)
        {
            switch (field)
            {
                case "id":
                    break;
            }
        }

        private void SetPlayerVisible(RealmType realmType)  // only used for non-local players
        {
            switch (realmType)
            {
                // realms with no pvp
                case RealmType.World:
                case RealmType.Dungeon:
                //case RealmType.RealmTutorial:
                //case RealmType.ActivityGuildSMBoss:
                //case RealmType.ActivityWorldBoss:
                    Show(!GameSettings.HideOtherPlayers);
                    break;
                // realms with pvp
                //case RealmType.ActivityGuardWar:
                //case RealmType.Arena:
                //case RealmType.InvitePVP:
                //case RealmType.EliteMap:
                //    Show(true);
                //    break;
                default:
                    Show(!GameSettings.HideOtherPlayers);
                    break;
            }
        }

        void OnLoadPlayer(GameObject go)
        {
            AnimObj = go;
            GameInfo.gCombat.SetPlayerParent(AnimObj);
            CharColliderDetect ccd = AnimObj.AddComponent<CharColliderDetect>();
            if (ccd != null)
                ccd.SetGhost(this);
            go.transform.position = Position;
            go.transform.forward = Forward;
            base.Init();

            string layerName = IsLocal ? "Player" : "Entities";
            ClientUtils.SetLayerRecursively(go, LayerMask.NameToLayer(layerName));
            go.tag = IsLocal ? "LocalPlayer" : "GhostPlayer";

            //mountController.OnSpawn();

            if (!IsLocal)
            {
                //Add playerghost to entity system
                ((ClientEntitySystem)EntitySystem).AddPlayer(this, true);
                if (GameInfo.mRealmInfo != null)
                {
                    HeadLabel.SetPlayerLabelByRealm(this, true);
                    SetPlayerVisible(GameInfo.mRealmInfo.type);
                }
                else
                {
                    Show(!GameSettings.HideOtherPlayers);
                }
            }
            else
            {
                ZDSPCamera mainCam = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
                mainCam.Init(AnimObj);

                Show(true);
                if (GameInfo.mInspectMode)
                {
                    foreach (Transform child in AnimObj.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
            IsModelLoaded = true;
        }

        public bool IsModelLoaded { get; set; }

        public override bool MaxEvasionChance
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool MaxCriticalChance
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override ICombatStats CombatStats
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private SkillPassiveCombatStats _skillPassiveCombatstats;
        public override SkillPassiveCombatStats SkillPassiveStats
        {
            get
            {
                if (_skillPassiveCombatstats == null)
                    _skillPassiveCombatstats = new SkillPassiveCombatStats(EntitySystem.Timers, this);
                return _skillPassiveCombatstats;
            }

            set
            {
                _skillPassiveCombatstats = value;
            }
        }

        public void Init(string playername, JobType jobType, byte gender, int mount, Vector3 pos, Vector3 dir, int health, int healthmax)
        {
            Position = pos;
            Forward = dir;
            this.Name = playername;
            mGender = (Gender)gender;

            if (IsLocal)
            {
                InitItemInvCtrl();
                InitPowerUpCtrl();
                QuestController = new QuestClientController(this);
                DestinyClueController = new DestinyClueClientController();
            }
            mEquipmentInvData = new EquipmentInventoryData();
            mEquipmentInvData.InitDefault();

            OnLoadPlayer(ClientUtils.InstantiatePlayer(mGender));
        }

        public override void AddLocalObject(LOTYPE objtype, LocalObject obj)
        {
            LocalObject mylocalobj = null;
            switch (objtype)
            {
                case LOTYPE.PlayerSynStats:
                    PlayerStats = new PlayerSynStats();
                    PlayerSynStats = (PlayerSynStats)PlayerStats;
                    PlayerStats.OnValueChanged = this.OnPlayerStatsChanged;
                    PlayerStats.OnLocalObjectChanged = OnPlayerStatsLocalObjectChanged;
                    mylocalobj = PlayerStats;
                    break;
                case LOTYPE.EquipmentStats:
                    EquipmentStats = new EquipmentStats();
                    EquipmentStats.EquipInventory.SetNotifyParent(false);
                    EquipmentStats.FashionInventory.SetNotifyParent(false);
                    EquipmentStats.OnCollectionChanged = OnEquipmentStatsCollectionChanged;
                    EquipmentStats.OnLocalObjectChanged = OnEquipmentStatsLocalObjectChanged;
                    mylocalobj = EquipmentStats;
                    break;
                case LOTYPE.ItemHotbarStats:
                    ItemHotbarStats = new ItemHotbarStats();
                    ItemHotbarStats.ItemHotbar.SetNotifyParent(false);
                    ItemHotbarStats.OnCollectionChanged = OnItemHotbarCollectionChanged;
                    mylocalobj = ItemHotbarStats;
                    break;
                case LOTYPE.SecondaryStats:
                    SecondaryStats = new SecondaryStats();
                    SecondaryStats.OnValueChanged = OnSecondaryStatsChanged;
                    mylocalobj = SecondaryStats;
                    break;
                case LOTYPE.SkillStats:
                    SkillStats = new SkillSynStats();
                    SkillStats.OnLocalObjectChanged = OnSkillLocalObjectChanged;
                    SkillStats.OnValueChanged = OnSkillStatsChanged;
                    mylocalobj = SkillStats;
                    break;
                case LOTYPE.RealmStats:
                    RealmStats = new RealmStats();
                    RealmStats.DungeonStory.SetNotifyParent(false);
                    RealmStats.DungeonDaily.SetNotifyParent(false);
                    RealmStats.DungeonSpecial.SetNotifyParent(false);
                    RealmStats.OnValueChanged = OnRealmStatsChanged;
                    mylocalobj = RealmStats;
                    break;
                case LOTYPE.QuestSynStats:
                    QuestStats = new QuestSynStatsClient();
                    QuestStats.AdventureQuest.SetNotifyParent(false);
                    QuestStats.SublineQuest.SetNotifyParent(false);
                    QuestStats.GuildQuest.SetNotifyParent(false);
                    QuestStats.EventQuest.SetNotifyParent(false);
                    QuestStats.OnValueChanged = OnQuestStatsValueChanged;
                    QuestStats.OnCollectionChanged = OnQuestStatsCollectionChanged;
                    QuestStats.OnNewlyAdded = OnQuestStatsAdded;
                    mylocalobj = QuestStats;
                    break;
                case LOTYPE.LocalCombatStats:
                    LocalCombatStats = new LocalCombatStats(); //note that this contains mimum amount of info to compute the rest of combat stats                    
                    LocalCombatStats.OnValueChanged = OnLocalCombatStatsChanged;
                    LocalCombatStats.OnLocalObjectChanged = OnCombatStatsLocalObjectChanged;
                    mylocalobj = LocalCombatStats;
                    break;
                case LOTYPE.LocalSkillPassiveStats:
                    LocalSkillPassiveStats = new LocalSkillPassiveStats();
                    LocalSkillPassiveStats.OnLocalObjectChanged = OnLocalSkillPassiveStatsChanged;
                    mylocalobj = LocalSkillPassiveStats;
                    break;
                case LOTYPE.HeroStats:
                    HeroStats = new HeroStatsClient();
                    HeroStats.heroes.SetNotifyParent(false);
                    HeroStats.OnCollectionChanged = OnHeroStatsCollectionChanged;
                    HeroStats.OnValueChanged = OnHeroStatsChanged;
                    HeroStats.OnNewlyAdded = HeroStats.Init;
                    mylocalobj = HeroStats;
                    break;
                case LOTYPE.PowerUpStats:
                    PowerUpStats = new PowerUpStats();
                    PowerUpStats.powerUpSlots.SetNotifyParent(false);
                    PowerUpStats.OnCollectionChanged = OnPowerUpStatsCollectionChanged;
                    PowerUpStats.OnValueChanged = OnPowerUpStatsValueChanged;
                    PowerUpStats.OnLocalObjectChanged = OnPowerUpStatsChanged;
                    mylocalobj = PowerUpStats;
                    break;
                // Shared Objects
                case LOTYPE.PartyStats:
                    PartyStats = new PartyStatsClient();
                    PartyStats.members.SetNotifyParent(false);
                    PartyStats.requests.SetNotifyParent(false);
                    PartyStats.OnCollectionChanged = OnPartyStatsCollectionChanged;
                    PartyStats.OnValueChanged = OnPartyStatsChanged;
                    PartyStats.OnNewlyAdded = PartyStats.Init;
                    mylocalobj = PartyStats;
                    break;
                case LOTYPE.DestinyClueSynStats:
                    DestinyClueStats = new DestinyClueStatsClient();
                    DestinyClueStats.OnValueChanged = OnDestinyClueStatsValueChanged;
                    DestinyClueStats.OnNewlyAdded = OnDestinyClueStatsAdded;
                    mylocalobj = DestinyClueStats;
                    break;
            }

            // InventoryStats Array
            if (objtype >= LOTYPE.InventoryStats && objtype <= LOTYPE.InventoryStatsEnd)
            {
                int index = objtype - LOTYPE.InventoryStats;
                InventoryStats[index] = new InventoryStats(objtype);
                InventoryStats[index].ItemInventory.SetNotifyParent(false);
                InventoryStats[index].OnCollectionChangedwithLO = OnInventoryStatsCollectionChanged;
                InventoryStats[index].OnLocalObjectChanged = OnInventoryStatsLocalObjectChanged;
                mylocalobj = InventoryStats[index];
            }

            if (mylocalobj == null)
            {
                Debug.LogWarning("Warning!! AddLocalObject mylocalobj is null objtype = " + objtype);
                return;
            }
            base.AddLocalObject(objtype, mylocalobj);
        }

        #region Quest Stats
        private void OnQuestStatsValueChanged(string field, object value, object oldvalue)
        {
            QuestController.DeserializeData(field, (string)value, (string)oldvalue);
        }

        private void OnQuestStatsCollectionChanged(string field, byte idx, object value)
        {
            QuestController.DeserializeCollectionData(field, (string)value, idx);
        }

        private void OnQuestStatsAdded()
        {
            QuestController.Init();
        }
        #endregion

        #region Destiny Clue Stats
        private void OnDestinyClueStatsValueChanged(string field, object value, object oldvalue)
        {
            DestinyClueController.DeserializeData(field, (string)value, (string)oldvalue);
        }

        private void OnDestinyClueStatsAdded()
        {
            DestinyClueController.Init();
        }
        #endregion

        public override void RemoveLocalObject(LOTYPE lotype)
        {
            base.RemoveLocalObject(lotype);
            switch (lotype)
            {
                case LOTYPE.PartyStats:
                    PartyStats = null;
                    break;
                //case LOTYPE.GuildStats:
                //    GuildStats = null;
                //    break;
            }
        }

        private void InitItemInvCtrl()
        {
            clientItemInvCtrl = new ItemInventoryController();
            if (clientItemInvCtrl == null)
                Debug.LogError("itemInvCtrl is null");

            InventoryStats = new InventoryStats[(int)InventorySlot.MAXSLOTS / (int)InventorySlot.COLLECTION_SIZE];
        }

        private void InitPowerUpCtrl()
        {
            clientPowerUpCtrl = new PowerUpController();
            if (clientPowerUpCtrl == null)
                Debug.LogError("powerUpCtrl is null");

            InventoryStats = new InventoryStats[(int)InventorySlot.MAXSLOTS / (int)InventorySlot.COLLECTION_SIZE];
        }
        protected override void PlayStunEffect(bool bplay)
        {
        }

        public void OnPlayerDead()
        {
            //Bot.ResetOnBotKilled();
            if (IsLocal)
            {
                DeadActionCommand cmd = new DeadActionCommand();
                ClientAuthoACDead action = new ClientAuthoACDead(this, cmd);
                PerformAction(action);
            }
        }

        public void DashAttack(float range = 0f, float dur = 0.4f)
        {
            if (IsLocal)
            {
                DashAttackCommand cmd = new DashAttackCommand();
                cmd.targetpos = GameInfo.gLocalPlayer.Position + GameInfo.gLocalPlayer.Forward * 8.0f;
                cmd.range = range;
                cmd.dashduration = dur;
                ClientAuthoDashAttack action = new ClientAuthoDashAttack(this, cmd);
                action.SetCompleteCallback(() =>
                {
                    Idle();
                });
                PerformAction(action);
            }
        }

        public bool Flash(float dur)
        {
            return true;
        }

        public void Respawn(Vector3 position, Vector3 dir)
        {
            Position = position;
            Forward = dir;
            Idle(true);
            mHeadLabel.mPlayerLabel.gameObject.SetActive(true);
        }

        public void InitMap()
        {
            GameObject obj = UIManager.GetWidget(HUDWidgetType.MiniMap);
            HUD_MiniMap hudMM = obj.GetComponent<HUD_MiniMap>();

            if (IsLocal)
            {
                hudMM.InitMap();
            }

            hudMM.AddIcon(HUD_MiniMap.IconType.PLAYER, this.Position);
        }

        public void PathFindToTarget(Vector3 position, int targetid, float range, bool targetsafe, bool movedirectonpathfound, Common.Actions.Action.CompleteCallBackDelegate callback)
        {
            if (!CanMove())
                return;
            ApproachWithPathFindCommand cmd = new ApproachWithPathFindCommand();
            cmd.movedirectonpathfound = movedirectonpathfound;
            cmd.range = range;
            cmd.targetpid = targetid;//target entity pid,pathfind action .
            cmd.targetpos = position;
            cmd.targetposSafe = targetsafe;
            ACApproachWithPathFind action = new ACApproachWithPathFind(this, cmd);
            action.SetCompleteCallback(callback);
            PerformAction(action, false, !(mAction != null && mAction.mdbCommand.GetActionType() == ACTIONTYPE.APPROACH_PATHFIND));
            ActionInterupted();
        }

        #region Quest
        public void ProceedToTarget(Vector3 pos, int id, CallBackAction actiontype, float prange =1.0f, int skillid=0)
        {
            Idle();
            Common.Actions.Action.CompleteCallBackDelegate action = null;
            float range = prange;
            int targetid = id;
            if (actiontype == CallBackAction.Interact)
            {
                StaticClientNPCAlwaysShow staticnpc = ((ClientEntitySystem)EntitySystem).GetStaticClientNPC(id);
                if (staticnpc != null)
                {
                    action = delegate {
                        staticnpc.Interact();
                    };
                    range = 1.5f;
                    targetid = -1;
                }
            }
            else if (actiontype == CallBackAction.BasicAttack)
            {
                targetid = id;
                action = delegate
                {
                    GameInfo.gCombat.CommonCastBasicAttack(targetid);
                };
            }else if (actiontype == CallBackAction.ActiveSkill)
            {
                targetid = id; 
                action = delegate
                {
                    GameInfo.gCombat.ApproachAndCastSkill(skillid, targetid, pos);
                };
            }

            PathFindToTarget(pos, targetid, range, true, false, action);
        }
        #endregion

        public override bool IsInvalidTarget()
        {
            return !IsAlive() || (LocalCombatStats != null && LocalCombatStats.IsInSafeZone);
            //nonlocal playerghost's localcombatstats is null
        }

        public override bool IsInSafeZone()
        {
            if (LocalCombatStats != null)
                return LocalCombatStats.IsInSafeZone;
            else
                return false;
        }

        public bool CanCastSkill(bool notify)
        {
            if (!IsAlive())
                return false;
            if (IsStun() || IsDisarmed())
                return false;
            if (mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Weapon) == null)
            {
                if (notify)
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CastSkillFail_NoWeapon"));
                return false;
            }
            return true;
        }

        public bool IsStun()
        {
            //if (LocalCombatStats != null)
            //    return LocalCombatStats.Stun;
            //else
                return false;
        }

        public bool IsRooted()
        {
            //if (LocalCombatStats != null)
            //    return LocalCombatStats.Root;
            //else
                return false;
        }

        public bool IsSilenced()
        {
            //if (LocalCombatStats != null)
            //    return LocalCombatStats.Silence;
            //else
                return false;
        }

        public bool IsDisarmed()
        {
            //if (LocalCombatStats != null)
            //    return LocalCombatStats.Disarmed;
            //else
                return false;
        }

        public bool CanMove()
        {
            return !IsStun() && !IsRooted();
        }

        public bool HasPositiveSE(int otherSEID)
        {
            CollectionHandler<object> positives = BuffTimeStats.Positives;
            for (int i = 0; i < positives.Count; i++)
            {
                int seid = (int)positives[i];

                if (otherSEID == seid)
                    return true;
            }
            return false;
        }

        public bool HasSideEffect(int seid)
        {
            CollectionHandler<object> positives = BuffTimeStats.Positives;
            for (int i = 0; i < positives.Count; i++)
            {
                if (seid == (int)positives[i])
                    return true;
            }

            CollectionHandler<object> negetives = BuffTimeStats.Negatives;
            for (int i = 0; i < negetives.Count; i++)
            {
                if (seid == (int)negetives[i])
                    return true;
            }
            return false;
        }

        public void InitBot(PlayerInput input)
        {
            mBotController = new BotController(this, input);
            //AutoPotion = new AutoPotion(this);
        }

        public bool CanStartNewBot()
        {
            if (!IsAlive())
                return false;

            if (!CanMove())
                return false;

            return true;
        }

        public void Idle(bool force = false)
        {
            IdleActionCommand cmd = new IdleActionCommand();
            PerformAction(new ClientAuthoACIdle(this, cmd), force);
        }

        public void ForceIdle()
        {
            Zealot.Common.Actions.Action action = GetAction();
            IdleActionCommand cmd = new IdleActionCommand();
            PerformAction(new ClientAuthoACIdle(this, cmd), true);
        }

        private void NotifyCurrencyIncrement(string field, object value, object oldvalue)
        {
            //RealmJson realmInfo = GameInfo.mRealmInfo;
            //if (realmInfo != null && realmInfo.type != RealmType.RealmWorld)
            //    return;
            switch (field)
            {
                //case "experience":
                case "money":
                case "contribute":
                case "gold":
                case "lotterypoints":
                case "honor":
                case "battlecoin":
                case "vippoints":
                case "bindgold":
                    ConfirmNotifyIncrement(field, value, oldvalue);
                    break;
            }
        }

        public void ConfirmNotifyIncrement(string field, object value, object oldvalue)
        {
            Type t = value.GetType();
            if (t.Name == "Int64")
            {
                long increment = (long)value - (long)oldvalue;
                if (increment > 0)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("currency", GUILocalizationRepo.GetLocalizedString("currency_" + field));
                    parameters.Add("increment", increment.ToString());
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CurrencyIncrement", parameters), true);
                }
            }
            else if (t.Name == "Int32")
            {
                int increment = (int)value - (int)oldvalue;
                if (increment > 0)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("currency", GUILocalizationRepo.GetLocalizedString("currency_" + field));
                    parameters.Add("increment", increment.ToString());
                    UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CurrencyIncrement", parameters), true);
                }
            }
        }

        public void SetAuctionAlert()
        {
            bool auctionOpen = GameUtils.IsBitSet(GameUtils.mAuctionStatus, (int)AuctionStatusBit.AuctionOpen);
            bool newRecord = GameUtils.IsBitSet(GameUtils.mAuctionStatus, (int)AuctionStatusBit.NewRecord);
            bool hasCollection = GameUtils.IsBitSet(GameUtils.mAuctionStatus, (int)AuctionStatusBit.CollectionAvailable);

            UIManager.AlertManager2.SetAlert(AlertType.AuctionBid, auctionOpen);
            UIManager.AlertManager2.SetAlert(AlertType.AuctionRecord, newRecord);
            UIManager.AlertManager2.SetAlert(AlertType.AuctionCollect, hasCollection);
        }

        public void CheckAuctionStatus(int status)
        {
            if (GameUtils.IsBitSet(status, (int)AuctionStatusBit.AuctionOpen))
                GameUtils.mAuctionStatus = GameUtils.SetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.AuctionOpen);
            else
                GameUtils.mAuctionStatus = GameUtils.UnsetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.AuctionOpen);

            if (GameUtils.IsBitSet(status, (int)AuctionStatusBit.CollectionAvailable))
                GameUtils.mAuctionStatus = GameUtils.SetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.CollectionAvailable);
            else
                GameUtils.mAuctionStatus = GameUtils.UnsetBit(GameUtils.mAuctionStatus, (int)AuctionStatusBit.CollectionAvailable);

            SetAuctionAlert();
        }

        public void OnNewDay()
        {
            //if (GuildStats != null)
            //    SetGuildQuestRedDot(true);
            if (PlayerSynStats.Level >= GameConstantRepo.GetConstantInt("Arena_UnlockLvl", 1))
                UIManager.AlertManager2.SetAlert(AlertType.ArenaFreeCount, true);
        }


        private GameTimer guildquestTimer = null;
        public void SetGuildQuestAlert()
        {
            StopTimer(guildquestTimer);

        }

        public override void Update(long dt)
        {
            base.Update(dt);
            if (IsLocal)
            {
                if (PlayerSynStats == null || LocalCombatStats == null || GameInfo.mInspectMode)
                    return;         //Player just spawned and not all stats are available yet
                if (!IsAlive())
                    return;

                if (PartyStats != null)
                    PartyStats.Update(dt);
                Bot.Update(dt);
                //AutoPotion.Update(dt);
                long temp = comboTimer;
                comboTimer += dt;
                if (temp < CombatUtils.COMBOTHIT_TIMEOUT && comboTimer >= CombatUtils.COMBOTHIT_TIMEOUT && ui_combohit != null)
                {
                    ui_combohit.gameObject.SetActive(false);
                    ui_combohit.ResetNumberOnTimeout();
                }
            }
        }

        private void StopTimer(GameTimer timer)
        {
            if (timer != null)
                EntitySystem.Timers.StopTimer(timer);
            timer = null;
        }

        public override void OnRemove()
        {
            ((ClientEntitySystem)EntitySystem).RemoveClientEntityByType(this);
            base.OnRemove();

            if (IsLocal == true)
            {
                StopTimer(mArenaRewardCD);
                StopTimer(guildquestTimer);
            }
        }

        public override bool CanPlayEffect()
        {
            if (base.CanPlayEffect())
            {
                if (IsLocal)
                    return true;
                else
                    return !GameSettings.HideOtherPlayers;
            }
            else
                return false;
        }

        public override void Show(bool val)
        {
            base.Show(val);
            if (val && CanPlayEffect())
                ShowEffect(true);
            else
                ShowEffect(false);

            if (HeadLabel != null)
            {
                if (IsLocal && GameInfo.mInspectMode)
                    HeadLabel.Show(false);
                else
                    HeadLabel.Show(val);
            }
        }

        public void ShowModelOnly(bool val)
        {
            base.Show(val);
            if (mAnimObj != null)
                mAnimObj.SetActive(val);//fix for cutscene;
        }

        public void SetArenaRewardDT(DateTime lastrewarddt)
        {
            mArenaLastRewardDT = lastrewarddt;
            TimeSpan timespan = GameInfo.GetSynchronizedServerDT() - lastrewarddt;
            //ArenaJson arena_info = RealmRepo.mArenaJson;
            //if (arena_info == null)
            //    return;
            //double timeLeft = arena_info.rewardcd * 3600 - timespan.TotalSeconds;
            //if (timeLeft > 0)
            //{
            //    mArenaRewardCD = EntitySystem.Timers.SetTimer((long)(timeLeft * 1000), (arg) =>
            //    {
            //        mArenaRewardCD = null;
            //        UIManager.AlertManager2.SetAlert(AlertType.ArenaReward, true);
            //    }, null);
            //    UIManager.AlertManager2.SetAlert(AlertType.ArenaReward, false);
            //}
            //else
            //    UIManager.AlertManager2.SetAlert(AlertType.ArenaReward, true);
        }

        public int GetArenaRewardCD()
        {
            if (mArenaRewardCD == null)
                return 0;
            return Mathf.CeilToInt((float)(mArenaRewardCD.Duration - mArenaRewardCD.ElapsedTime));
        }

        public void TestComboSkill(int sid, SideEffectJson mainsej, SideEffectJson sej, int lvl, float dur)
        {
            //TEST OF ONE SKill Combo ONLY
        }

        public PartsType WeaponTypeUsed = PartsType.Blade;
        public override string GetRunningAnimation()
        {           
            switch (WeaponTypeUsed)
            {
                case PartsType.Sword:
                    return "sword_run";
                case PartsType.Blade:
                    return "blade_run";
                case PartsType.Lance:
                    return "lance_run";
                case PartsType.Hammer:
                    return "hammer_run";
                case PartsType.Fan:
                    return "fan_run";
                case PartsType.Xbow:
                    return "xbow_run";
                case PartsType.Dagger:
                    return "dagger_run";
                case PartsType.Sanxian:
                    return "sanxian_run";                        
                default:
                    return "blade_run";
            }
        }

        public override string GetHitAnimation()
        {
            switch (WeaponTypeUsed)
            {
                case PartsType.Sword:
                    return "sword_gethit";
                case PartsType.Blade:
                    return "blade_gethit";
                case PartsType.Lance:
                    return "lance_gethit";
                case PartsType.Hammer:
                    return "hammer_gethit";
                case PartsType.Fan:
                    return "fan_gethit";
                case PartsType.Xbow:
                    return "xbow_gethit";
                case PartsType.Dagger:
                    return "dagger_gethit";
                case PartsType.Sanxian:
                    return "sanxian_gethit";   
                default:
                    return "blade_gethit";
            }
        }

        public override string GetStandbyAnimation()
        {
            bool isincombat = false;
            if (LocalCombatStats != null)
                isincombat = LocalCombatStats.IsInCombat;

            switch (WeaponTypeUsed)
            {
                case PartsType.Sword:
                    return isincombat ? "sword_standby" : "sword_nmstandby";
                case PartsType.Blade:
                    return isincombat ? "blade_standby" : "blade_nmstandby";
                case PartsType.Lance:
                    return isincombat ? "lance_standby" : "lance_nmstandby";
                case PartsType.Hammer:
                    return isincombat ? "hammer_standby" : "hammer_nmstandby";
                case PartsType.Fan:
                    return isincombat ? "fan_standby" : "fan_nmstandby";
                case PartsType.Xbow:
                    return isincombat ? "xbow_standby" : "xbow_nmstandby";
                case PartsType.Dagger:
                    return isincombat ? "dagger_standby" : "dagger_nmstandby";
                case PartsType.Sanxian:
                    return isincombat ? "sanxian_standby" : "sanxian_nmstandby";               
                default:
                    return isincombat ? "blade_standby" : "blade_nmstandby";
            }
        }

        public override string GetDyingEffect()
        {
            switch (WeaponTypeUsed)
            {
                case PartsType.Sword:
                    return "sword_dying";
                case PartsType.Blade:
                    return "blade_dying";
                case PartsType.Lance:
                    return "lance_dying";
                case PartsType.Hammer:
                    return "hammer_dying";
                case PartsType.Fan:
                    return "fan_dying";
                case PartsType.Xbow:
                    return "xbow_dying";
                case PartsType.Dagger:
                    return "dagger_dying";
                case PartsType.Sanxian:
                    return "sanxian_dying";     
                default:
                    return "blade_dying";
            }
        }

        public override string GetWeaponExtension()
        {
            switch (WeaponTypeUsed)
            {
                case PartsType.Sword:
                    return "sword_";
                case PartsType.Blade:
                    return "blade_";
                case PartsType.Lance:
                    return "lance_";
                case PartsType.Hammer:
                    return "hammer_";
                case PartsType.Fan:
                    return "fan_";
                case PartsType.Xbow:
                    return "xbow_";
                case PartsType.Dagger:
                    return "dagger_";
                case PartsType.Sanxian:
                    return "sanxian_";  
                default:
                    return "blade_";
            }
        }

        public override void SetHeadLabel(bool init=false)
        {
            HeadLabel.SetPlayerLabelByRealm(this, init);
        }
         
        public override float RtReduction()
        {
            if (PlayerStats != null)
            {
                return PlayerStats.rtReduction;
            }
            return 1f;
        }

        public override int GetMinDmg()
        {
            throw new NotImplementedException();
        }

        public override int GetAccuracy()
        {
            throw new NotImplementedException();
        }

        public override int GetAttack()
        {
            throw new NotImplementedException();
        }

        public override int GetCritical()
        {
            throw new NotImplementedException();
        }

        public override int GetCriticalDamage()
        {
            throw new NotImplementedException();
        }

        public override int GetArmor()
        {
            throw new NotImplementedException();
        }

        public override int GetEvasion()
        {
            throw new NotImplementedException();
        }

        public override int GetCocritical()
        {
            throw new NotImplementedException();
        }

        public override int GetCocriticalDamage()
        {
            throw new NotImplementedException();
        }

        public override float GetExDamage()
        {
            throw new NotImplementedException();
        }

        public void ActionInterupted()
        {
            if (QuestController != null)
            {
                QuestController.ActionInterupted();
            }
        }

        public IEnumerator PlayCutscene(string name, int delay, int questid)
        {
            yield return new WaitForSecondsRealtime(delay);

            GameInfo.gCombat.CutsceneManager.PlayCutscene(name, () => StartNextQuestEvent(questid));
        }

        public void StartNextQuestEvent(int questid)
        {
            //RPCFactory.NonCombatRPC.UpdateQuestStatus(questid);
            //QuestController.StartNextQuestEvent();
        }

        public void CheckQuestTeleportAction()
        {
            QuestController.CheckQuestTeleportAction();
        }

        private CompanionGhost mCompanionGhost;

        public void UpdatePlayerCompanion()
        {
            StaticNPCJson staticNPC = StaticNPCRepo.GetStaticNPCById(PlayerSynStats.QuestCompanionId);
            if (staticNPC != null)
            {
                if (mCompanionGhost != null && mCompanionGhost.GetNpcId() != staticNPC.id)
                {
                    EntitySystem.RemoveEntityByID(mCompanionGhost.ID);
                    mCompanionGhost = null;
                }

                Vector3 pos = mPos + ((mForward * -1) * 1.5f);
                if (mCompanionGhost == null)
                {
                    mCompanionGhost = EntitySystem.SpawnEntity<CompanionGhost>();
                    mCompanionGhost.Init(staticNPC, pos, mForward, this);
                }
                else
                {
                    mCompanionGhost.UpdatePosition(pos, mForward);
                }
            }
            else
            {
                if (mCompanionGhost != null)
                {
                    EntitySystem.RemoveEntityByID(mCompanionGhost.ID);
                }
                mCompanionGhost = null;
            }
        }
    }
}
