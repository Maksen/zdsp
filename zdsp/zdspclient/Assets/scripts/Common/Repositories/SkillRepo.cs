using System.Collections.Generic;
using Kopio.JsonContracts;
using Newtonsoft.Json;
using Zealot.Common;

namespace Zealot.Repository
{
    [System.Flags]
    public enum SkillReturnCode : byte
    {
        EMPTY = 0,
        SUCCESS = 1 << 0,
        SKILLPOINTFAILED = 1 << 1,
        MONEYFAILED = 1 << 2,
    }

    public class SkillData
    {
        public SkillGroupJson skillgroupJson;
        public SkillJson skillJson;
        public SkillSideEffect skills;

        public SkillData(SkillGroupJson skgrp, SkillJson sk)
        {
            skillgroupJson = skgrp;
            skillJson = sk;
        }
    }

    public class SkillSideEffect {
        public List<SideEffectJson> mSelf;
        public List<SideEffectJson> mTarget;

        public SkillSideEffect() {
            mSelf = new List<SideEffectJson>();
            mTarget = new List<SideEffectJson>();
        }

        public void AddSelf(SideEffectJson item) {
            mSelf.Add(item);
        }

        public void AddTarget(SideEffectJson item) {
            mTarget.Add(item);
        }
    }

    public static class SkillRepo
    {
        //public static readonly int MONSTER_BASIC_ATTACK_SKILLID = 7; //the skillgroupid
        //public static int Rage_BasicAtk1 = 1;//the skillgroupid
        //public static int Rage_BasicAtk2 = 2;//the skillgroupid
        //public static int Rage_BasicAtk3 = 3; //the skillgroupid

        public static Dictionary<int, SkillGroupJson> mSkillGroupsRaw;
        public static Dictionary<int, SkillData> mSkills;
        //public static Dictionary<int, List<SideEffectJson>> mSkillSideEffects; //active
        public static Dictionary<int, SkillSideEffect> mSkillSideEffects;
        //public static Dictionary<int, Dictionary<int, SideEffectJson>> mSideEffectByLevel;
        //public static Dictionary<int, List<SideEffectJson>> mPassiveSideEffects; //passive
        //public static SkillData mMonsterBasicAttack; //Cache for quick access in monster AI

        public static Dictionary<int, List<int>> m_SkillGroupToSkill;


        public static Dictionary<PartsType, Dictionary<int, int>> mWeaponTypeGetSkillID;

        public static Dictionary<int, Dictionary<WeaponType, int>> mWeapontypeToSId;
        static SkillRepo()
        {
            mSkills = new Dictionary<int, SkillData>();
            mSkillSideEffects = new Dictionary<int, SkillSideEffect>();
            //mPassiveSideEffects = new Dictionary<int, List<SideEffectJson>>();
            mWeapontypeToSId = new Dictionary<int, Dictionary<WeaponType, int>>();
            //mSideEffectByLevel = new Dictionary<int, Dictionary<int, SideEffectJson>>();
            mWeaponTypeGetSkillID = new Dictionary<PartsType, Dictionary<int, int>>();
            m_SkillGroupToSkill = new Dictionary<int, List<int>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mSkills.Clear();
            mSkillSideEffects.Clear();
            mWeapontypeToSId.Clear();
            mWeaponTypeGetSkillID.Clear();
            m_SkillGroupToSkill.Clear();
            mSkillGroupsRaw = gameData.SkillGroup; 
            foreach (KeyValuePair<int, SkillJson> entry in gameData.Skill)
            {
                SkillJson skillJson = entry.Value;
                SkillGroupJson skillgroupJson;
                if (!mSkillGroupsRaw.TryGetValue(skillJson.skillgroupid, out skillgroupJson))
                {
                    throw new System.Exception("Invalid skillgroupid [" + skillJson.skillgroupid + "] linked to skill[" + skillJson.id + "]");
                }

                SkillData skillData = new SkillData(skillgroupJson, skillJson);
                mSkills.Add(skillJson.id, skillData);

                if (!mWeapontypeToSId.ContainsKey(skillJson.skillgroupid))
                {
                    mWeapontypeToSId.Add(skillJson.skillgroupid, new Dictionary<WeaponType, int>()); 
                }

                if (!m_SkillGroupToSkill.ContainsKey(skillgroupJson.id))
                {
                    m_SkillGroupToSkill.Add(skillgroupJson.id, new List<int>());
                }
                m_SkillGroupToSkill[skillgroupJson.id].Add(skillJson.id);

                List<WeaponType> temp = SplitWeaponType(skillgroupJson.weaponsrequired);
                foreach (WeaponType type in temp) {

                    if (!mWeapontypeToSId[skillJson.skillgroupid].ContainsKey(type))
                        mWeapontypeToSId[skillJson.skillgroupid].Add(type, skillJson.id);
                }

                List<PartsType> tempp = SplitWeaponPartType(skillgroupJson.weaponsrequired);
                foreach (PartsType type in tempp) {
                    if (!mWeaponTypeGetSkillID.ContainsKey(type)) {
                        mWeaponTypeGetSkillID.Add(type, new Dictionary<int, int>());
                    }

                    if (!mWeaponTypeGetSkillID[type].ContainsKey(skillJson.id)) {
                        mWeaponTypeGetSkillID[type].Add(skillJson.id, skillJson.skillgroupid);
                    }
                }
            }

            foreach (Skill__selfsideeffectJson val in gameData.Skill__selfsideeffect)
            {
                SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(val.selfsideeffectid);
                if (sideeffect == null)
                {
                    throw new System.Exception("Unknown sideeffect[" + val.selfsideeffectid + "]" + " linked to skill [" + val.skillid + "]");
                }

                if (mSkillSideEffects.ContainsKey(val.skillid))
                    mSkillSideEffects[val.skillid].AddSelf(sideeffect);
                else {
                    mSkillSideEffects.Add(val.skillid, new SkillSideEffect());
                    mSkillSideEffects[val.skillid].AddSelf(sideeffect);
                }
            }

            foreach (Skill__sideeffectsJson val in gameData.Skill__sideeffects) {
                SideEffectJson sideeffect = SideEffectRepo.GetSideEffect(val.sideeffectsid);
                if (sideeffect == null) {
                    throw new System.Exception("Unknown sideeffect[" + val.sideeffectsid + "]" + " linked to skill [" + val.skillid + "]");
                }

                if (mSkillSideEffects.ContainsKey(val.skillid))
                    mSkillSideEffects[val.skillid].AddTarget(sideeffect);
                else {
                    mSkillSideEffects.Add(val.skillid, new SkillSideEffect());
                    mSkillSideEffects[val.skillid].AddTarget(sideeffect);
                }
            }

            //foreach (KeyValuePair<int, SideEffectJson> entry in SideEffectRepo.mIdMap)
            //{
            //    SideEffectJson sideeffect = entry.Value;
            //    Dictionary<int, SideEffectJson> tmplist = new Dictionary<int, SideEffectJson>();
            //    for (int i = 0; i < 100; i++)
            //    {
            //        tmplist.Add(i, UpdateSideEffectByLevel(sideeffect, i+1));
            //    }

            //    mSideEffectByLevel.Add(sideeffect.id, tmplist);
               
            //}

            foreach (KeyValuePair<int, List<int>> ids in m_SkillGroupToSkill)
            {
                ids.Value.Sort((lhs, rhs) => GetSkill(lhs).skillJson.level.CompareTo(GetSkill(rhs).skillJson.level));
            }

            //mMonsterBasicAttack = GetSkill(MONSTER_BASIC_ATTACK_SKILLID);
        }

        public static List<WeaponType> SplitWeaponType(string weaponlist) {
            List<WeaponType> result = new List<WeaponType>();
            string[] split = weaponlist.Split(';');
            foreach(var key in split) {
                WeaponType type = WeaponType.Any;
                switch (key) {
                    case "SW":
                        type = WeaponType.Sword;
                        break;
                    case "BD":
                        type = WeaponType.Blade;
                        break;
                    case "SP":
                        type = WeaponType.Lance;
                        break;
                    case "HA":
                        type = WeaponType.Hammer;
                        break;
                    case "D":
                        type = WeaponType.Dagger;
                        break;
                    case "XB":
                        type = WeaponType.Xbow;
                        break;
                    case "FA":
                        type = WeaponType.Fan;
                        break;
                    case "LT":
                        type = WeaponType.Sanxian;
                        break;
                    case "NONE":
                        type = WeaponType.Any;
                        break;
                }
                result.Add(type);
            }
            return result;
        }

        public static List<PartsType> SplitWeaponPartType(string weaponlist) {
            List<PartsType> result = new List<PartsType>();
            string[] split = weaponlist.Split(';');
            foreach (var key in split) {
                PartsType type = PartsType.Sword;
                switch (key) {
                    case "SW":
                        type = PartsType.Sword;
                        break;
                    case "BD":
                        type = PartsType.Blade;
                        break;
                    case "SP":
                        type = PartsType.Lance;
                        break;
                    case "HA":
                        type = PartsType.Hammer;
                        break;
                    case "D":
                        type = PartsType.Dagger;
                        break;
                    case "XB":
                        type = PartsType.Xbow;
                        break;
                    case "FA":
                        type = PartsType.Fan;
                        break;
                    case "LT":
                        type = PartsType.Sanxian;
                        break;
                    //case "NONE":
                    //    type = PartsType.Any;
                    //    break;
                }
                result.Add(type);
            }
            return result;
        }


        public static SkillGroupJson GetSkillGroupById(int skillgroupid)
        {
            SkillGroupJson json;
            mSkillGroupsRaw.TryGetValue(skillgroupid, out json);
            return json;
        }

        /// <summary>
        /// call this method to get level 1 skill. it will not create new data 
        /// </summary>
        /// <param name="skillgroupid"></param>
        /// <returns></returns>
        public static SkillData GetSkillByGroupID(int skillgroupid)
        {
            int skillid = GetSkillIdFromGroupId(skillgroupid);
            if (skillid > 0)
                return GetSkill(skillid);
            return null; 
        }

        /// <summary>
        /// get skill of level or next closest level to given level 
        /// </summary>
        /// <param name="skillgroupid"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static SkillData GetSkillByGroupIDOfNextLevel(int skillgroupId, int level)
        {
            if (m_SkillGroupToSkill.ContainsKey(skillgroupId))
            {
                //skill group found
                //search for skill of level + 1
                int result = BinarySearchList(m_SkillGroupToSkill[skillgroupId], 0, m_SkillGroupToSkill[skillgroupId].Count - 1, level + 1);
                if (result == -1) return null;

                return GetSkill(m_SkillGroupToSkill[skillgroupId][result]);
            }
            return null;
        }

        /// <summary>
        /// get skill of given level 
        /// </summary>
        /// <param name="skillgroupid"></param>
        /// <param name="level"></param>
        /// <returns>SkillData if found, null if not found, note: does not search for next closest skill</returns>
        public static SkillData GetSkillByGroupIDOfLevel(int skillgroupId, int level)
        {
            if (m_SkillGroupToSkill.ContainsKey(skillgroupId))
            {
                //skill group found
                //search for skill of level + 1
                int result = BinarySearchList(m_SkillGroupToSkill[skillgroupId], 0, m_SkillGroupToSkill[skillgroupId].Count - 1, level);
                if (result == -1 || result == m_SkillGroupToSkill[skillgroupId].Count) return null;

                return GetSkill(m_SkillGroupToSkill[skillgroupId][result]);
            }
            return null;
        }

        public static int BinarySearchList(List<int> list, int start, int end, int x)
        {
            int near = 0;
            while(start <= end)
            {
                int middle = start + ((end - start) >> 1);
                int level = GetSkill(list[middle]).skillJson.level;
                if (level == x) return middle;
                if (level < x) start = middle + 1;
                else end = middle - 1;
                near = middle + 1;
            }
            if (near != end + 1) return near;
            else
                return -1;
        }

        public static bool IsSkillMaxLevel(int skillgroupid, int level)
        {
            if (m_SkillGroupToSkill.ContainsKey(skillgroupid))
            {
                List<int> skillList = m_SkillGroupToSkill[skillgroupid];
                if (skillList.Count > 0)
                    return GetSkill(skillList[skillList.Count - 1]).skillJson.level == level;
                else
                    return false;
            }
            return false;
        }

        public static int GetSkillGroupMaxLevel(int skillgroupid)
        {
            if (m_SkillGroupToSkill.ContainsKey(skillgroupid))
            {
                List<int> skillList = m_SkillGroupToSkill[skillgroupid];
                if (skillList.Count > 0)
                    return GetSkill(skillList[skillList.Count - 1]).skillJson.level;
                else
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// now we refer to skillid in action command as skillgroupid,  
        /// to get skillid from skillgroupid , call 
        /// </summary>
        /// <param name="skillid"></param>
        /// <returns></returns>
        public static SkillData GetSkill(int skillid)
        {
            SkillData skillData;
            mSkills.TryGetValue(skillid, out skillData);
            if (skillData != null)
            {
                if(skillData.skills == null) {
                    skillData.skills = GetSkillSideEffects(skillid);
                }
            }
            return skillData;
        }

        /// <summary>
        /// the same skillgroup will have different action and effect for different weapon type
        /// so use this with weapon type to get the skilldata needed to play 
        /// </summary>
        /// <param name="skillgroupId"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static SkillData GetSkillByWeapon(int skillgroupId, WeaponType weapon)
        {
            int sid = -1;
            Dictionary<WeaponType, int> lvtoid = null;
            mWeapontypeToSId.TryGetValue(skillgroupId, out lvtoid);
            if (lvtoid != null)
            {
                if (!lvtoid.TryGetValue(weapon, out sid))
                {
                    lvtoid.TryGetValue(WeaponType.Any, out sid);
                };
            }
             
            return GetSkill(sid);
        }

        /// <summary>
        /// This Won't work since assuming skill group id is not uq
        /// </summary>
        /// <param name="skillgroupId"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static SkillData GetSkillGivenWeapon(int skillgroupId, PartsType weapon) {
            Dictionary<int, int> result = null;
            mWeaponTypeGetSkillID.TryGetValue(weapon, out result);
            if(result != null) {
                return GetSkill(result[skillgroupId]);
            }
            return null;
        }

        //public static SkillData GetWeaponsBasicAttackData(WeaponType weapon, int index) {
        //    int id = -1;
        //    Dictionary<int, int> result = null;

        //    mWeaponTypeGetSkillID.TryGetValue(weapon, out result);
        //    foreach(var iter in result) {
        //        SkillData data = GetSkill(iter.Value);
        //        if (data.skillJson.name.Contains("atk" + index.ToString()))
        //            return data;
        //    }
        //    return null;
        //}

        //public static SkillData GetGenderWeaponBasicAttackData(WeaponType weapon, int index, string gender) {
        //    int id = -1;
        //    Dictionary<int, int> result = null;

        //    mWeaponTypeGetSkillID.TryGetValue(weapon, out result);
        //    foreach (var iter in result) {
        //        SkillData data = GetSkill(iter.Value);
        //        if (data.skillJson.name.Contains(gender + "_atk" + index.ToString()))
        //            return data;
        //    }
        //    return null;
        //}

        public static SkillData GetWeaponsBasicAttackData(PartsType weapon, int index) {
            Dictionary<int, int> result = null;

            mWeaponTypeGetSkillID.TryGetValue(weapon, out result);
            foreach (var iter in result) {
                SkillData data = GetSkill(iter.Key);
                if (data.skillJson.name.Contains("atk" + index.ToString() + "_" + weapon.ToString().ToLower()))
                    return data;
            }
            return null;
        }

        public static SkillData GetGenderWeaponBasicAttackData(PartsType weapon, int index, string gender) {
            Dictionary<int, int> result = null;

            mWeaponTypeGetSkillID.TryGetValue(weapon, out result);
            if (result == null) return null;
            foreach (var iter in result) {
                SkillData data = GetSkill(iter.Key);
                if (data.skillgroupJson.name.Contains(gender.ToLower() + "_" + weapon.ToString().ToLower() + "_atk" + index.ToString()))
                    return data;
            }
            return null;
        }

        public static SideEffectJson CloneSideEffect(SideEffectJson sej)
        {
            string str = JsonConvert.SerializeObject(sej);
            SideEffectJson ressej = (SideEffectJson)JsonConvert.DeserializeObject(str, typeof(SideEffectJson));
            return ressej;
        }

        public static List<JobType> GetSkillRequiredClass(int skillid)
        {
            List<JobType> result = new List<JobType>();
            SkillData skill = GetSkill(skillid);
            string list = skill.skillJson.requiredclass;
            string[] res = list.Split(';');
            foreach(string iter in res)
            {
                int job;
                bool code = System.Int32.TryParse(iter, out job);
                if (code)
                    result.Add((JobType)job);
            }
            return result;
        }
         
        /// <summary>
        /// call this method to get skill data whose data may be modified 
        /// </summary>
        /// <param name="skillgroupid"></param>
        /// <param name="lvl"></param>
        /// <returns></returns>
      
        public static int GetSkillIdFromGroupId(int skillgroupId)
        {
            if (!m_SkillGroupToSkill.ContainsKey(skillgroupId))
            {
                return -1;
            }
            return m_SkillGroupToSkill[skillgroupId][0];
        }

        public static int GetSkillGroupID(int skillid)
        {
            if (skillid == 0)
                return 0;//for migrating from old character data. 0 means the skill combo is empty
            SkillData skillData;
            mSkills.TryGetValue(skillid, out skillData);
            if (skillData != null)
                return skillData.skillJson.skillgroupid;
            return 0;
        }        

        //public static SkillData GetSkill(int skillgroupId, int level)
        //{
        //    int sid = -1;
        //    mSkillGroupSkillMap.TryGetValue(skillgroupId, out sid);
        //    if (sid != -1)
        //    {
        //        SkillData skillAtLvl1 = null;
        //        mSkills.TryGetValue(sid, out skillAtLvl1);
        //        if (skillAtLvl1 != null)
        //        {
        //            SkillData skillData = new SkillData(skillAtLvl1.skillgroupJson, skillAtLvl1.skillJson);
        //            //clone the sideeffects. 
        //            skillData.mainskills = new List<SideEffectJson>();
        //            skillData.subskills = null;
        //            if (skillAtLvl1.mainskills != null)
        //            {
        //                foreach (SideEffectJson sej in skillAtLvl1.mainskills)
        //                {
        //                    SideEffectJson newsej = CloneSideEffect(sej);

        //                    skillData.mainskills.Add(newsej);
        //                }
        //            }
        //            return skillData;
        //        }
        //        return null;
        //    }
        //    else return null;
        //}


        //public static SkillData GetNextLevelUpSkill(int skillgroupid, int lvl)
        //{
        //    int skillid = GetSkillIdFromGroupId(skillgroupid);
        //    return GetSkill(skillid, lvl);
        //}

        //public static SideEffectJson GetSideEffectAtLevel(int seid, int lvl)
        //{
        //    if (lvl < 1 || lvl > 100)
        //        return null;
        //    if (mSideEffectByLevel.ContainsKey(seid))
        //    {
        //        if(mSideEffectByLevel[seid].ContainsKey(lvl-1))
        //        {
        //            return mSideEffectByLevel[seid][lvl-1];
        //        }
        //    }
        //    return null;
        //}

        public static SkillSideEffect GetSkillSideEffects(int skillid)
        {
            SkillSideEffect sideeffects = null;
            mSkillSideEffects.TryGetValue(skillid, out sideeffects);
            return sideeffects;
        }

        public static List<SideEffectJson> GetPassiveSideEffects(int skillid)
        {
            List<SideEffectJson> sideeffects = null;
            //mPassiveSideEffects.TryGetValue(skillid, out sideeffects);
            return sideeffects;
        }
    }

    public static class SideEffectRepo
    {
        public static Dictionary<int, SideEffectJson> mIdMap;
        public static Dictionary<int, SideEffectGroupJson> mSideEffectGroups;
        public static Dictionary<int, int> mSideEffectGroupID;
        public static Dictionary<int, List<int>> mSideEffectGroupIndex;
        static SideEffectRepo()
        {
            mIdMap = new Dictionary<int, SideEffectJson>();
            //mSeIDToGroupID = new Dictionary<int, SEGroupInfo>();           
            mSideEffectGroupID = new Dictionary<int, int>();
            mSideEffectGroupIndex = new Dictionary<int, List<int>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.SideEffect;
            mSideEffectGroups = gameData.SideEffectGroup;
            mSideEffectGroupID.Clear();
            foreach (SideEffectGroup__sideeffectsJson entry in gameData.SideEffectGroup__sideeffects)
            {
                if (mSideEffectGroupIndex.ContainsKey(entry.sideeffectgroupid))
                    mSideEffectGroupIndex[entry.sideeffectgroupid].Add(entry.sideeffectsid);
                else
                    mSideEffectGroupIndex.Add(entry.sideeffectgroupid, new List<int>(){ entry.sideeffectsid });

                SideEffectGroupJson sideEffectGroup = GetSEGroup(entry.sideeffectgroupid);
                SideEffectJson sideEffect = GetSideEffect(entry.sideeffectsid);
                if (sideEffectGroup != null && sideEffect != null)
                    sideEffectGroup.sideeffects.Add(entry.sideeffectsid, sideEffect);
            }

            foreach (KeyValuePair<int, SideEffectJson> entry in mIdMap)
            {
                int groupid = 0;
                foreach (KeyValuePair<int, List<int>> innerentry in mSideEffectGroupIndex)
                {
                    if (innerentry.Value.Contains(entry.Key))
                    {
                        groupid = innerentry.Key;
                        break;
                    }
                }
                mSideEffectGroupID.Add(entry.Key, groupid);
            }

            //test
            //bool can = CanOverride(14, 10);
            //can = CanOverride(10, 14);
            //can = CanOverride(1, 2);
            //can = CanOverride(1, 1);
        }


        //TODO: currently the same sideeffect can be applied multiply time. 
        //this is to easily resue of the same sideeffect in different skills.
        //overrite happends only if the two in the same sideeffect group.
        public static bool CanOverride(int newse, int oldse)
        {
            if (mSideEffectGroupID[newse] == mSideEffectGroupID[oldse] && mSideEffectGroupID[newse] > 0)
            {
                int groupid = mSideEffectGroupID[newse];
                return mSideEffectGroupIndex[groupid].IndexOf(newse) > mSideEffectGroupIndex[groupid].IndexOf(oldse);
            }
            return false;
        }

        public static bool InSameGroup(int newse, int oldse)
        {
            if (newse == oldse)
                return false;
            if (mSideEffectGroupID.ContainsKey(newse) && mSideEffectGroups.ContainsKey(oldse))
                return mSideEffectGroupID[newse] > 0 && mSideEffectGroupID[newse] == mSideEffectGroupID[oldse];
            else
                return false;
        }

        public static SideEffectJson GetSideEffect(int id)
        {
            SideEffectJson sideeffect;
            mIdMap.TryGetValue(id, out sideeffect);
            return sideeffect;
        }

        public static SideEffectGroupJson GetSEGroup(int groupID)
        {
            SideEffectGroupJson group;
            mSideEffectGroups.TryGetValue(groupID, out group);
            return group;
        }

    }

    public static class SDGRepo
    {
        public static Dictionary<int, SkillDescriptionGroupJson> mIdMap;
        private static Dictionary<int, string> mDescriptionByID;
        private static Dictionary<string, string> mDescriptionByName;
        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.SkillDescriptionGroup;
            mDescriptionByID = new Dictionary<int, string>();
            mDescriptionByName = new Dictionary<string, string>();
            foreach (KeyValuePair<int, SkillDescriptionGroupJson> entry in mIdMap)
            {
                mDescriptionByID.Add(entry.Value.id, entry.Value.description);
                if (mDescriptionByName.ContainsKey(entry.Value.name))
                {
                    throw new System.Exception("SDG have duplicated Group name !");

                }
                else
                {
                    mDescriptionByName.Add(entry.Value.name, entry.Value.description);
                }
            }
        }

        public static string GetDescriptionByName(string sdgName)
        {
            if (mDescriptionByName.ContainsKey(sdgName))
                return mDescriptionByName[sdgName];
            else
                return "";
        }

        public static string GetDescriptionByID(int id)
        {
            if (mDescriptionByID.ContainsKey(id))
                return mDescriptionByID[id];
            else
                return "";
        }
    }

    public class NPCSkillCondition //Execution condition
    {
        public SkillData skilldata;
        public int priority;
        public float chance;
        public NPCSkillCondition(SkillData sd, int p, float c)
        {
            skilldata = sd;
            priority = p;
            chance = c;
        }
    }

    public static class NPCSkillsRepo
    {
        public static Dictionary<int, List<NPCSkillCondition>> mNPCSkills;

        static NPCSkillsRepo()
        {
            mNPCSkills = new Dictionary<int, List<NPCSkillCondition>>();
        }

        public static void Init(GameDBRepo gameData)
        {
             
            //foreach (KeyValuePair<int, NPCToSkillsLinkJson> entry in gameData.NPCToSkillsLink)
            //{
            //    NPCToSkillsLinkJson val = entry.Value;
            //    int npcid = val.npcid;

            //    List<NPCSkillCondition> currConds;
            //    bool found = mNPCSkills.TryGetValue(npcid, out currConds);
            //    if (!found)
            //    {
            //        currConds = new List<NPCSkillCondition>();
            //        mNPCSkills.Add(npcid, currConds);
            //    }

            //    bool added = false;
            //    SkillData skilldata = SkillRepo.GetSkill(val.skillid);
            //    NPCSkillCondition newCond = new NPCSkillCondition(skilldata, val.priority, val.chance);

            //    for (int i = 0; i < currConds.Count; i++)
            //    {
            //        NPCSkillCondition cond = currConds[i];
            //        if (newCond.priority < cond.priority)
            //        {
            //            currConds.Insert(i, newCond);
            //            added = true;
            //            break;
            //        }
            //    }
            //    if (!added)
            //        currConds.Add(newCond);
            //}
        }

        public static List<NPCSkillCondition> GetSkillConditions(int npcid)
        {
            List<NPCSkillCondition> skillConds;
            mNPCSkills.TryGetValue(npcid, out skillConds);
            return skillConds;
        }
    }
}
