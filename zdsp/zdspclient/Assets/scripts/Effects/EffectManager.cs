//#define USE_CACHING

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Repository;
using System.Collections;

public class EfxSystem : MonoSingleton<EfxSystem>
{
    private static readonly int[] TEST_SKILLS = new int[] { 524, 540 };
    private static readonly List<string> suffex = new List<string> { "_kn", "_sw", "_sp", "_ha" };

    void Start()
    {
    }

    public void Init()
    {
    }

    protected override void OnDestroy()
    {
        OnDestroyEfxSystem();
        base.OnDestroy();
    }

    private Dictionary<string, string> mEffectPaths;
    private List<string> mPreloadedEffects;

    private Dictionary<string, GameObject> mEfxPrefabList;
    public void InitFromGameDB()
    {
        GameObject _parent = this.gameObject;
        mEfxPrefabList = new Dictionary<string, GameObject>();
        if (playerRGBSkill == null)
            playerRGBSkill = new Dictionary<string, string>();
        mEffectPaths = new Dictionary<string, string>();
        mPreloadedEffects = new List<string>();

        foreach (KeyValuePair<int, SkillData> entry in SkillRepo.mSkills)
        {
            SkillData sdata = entry.Value; 
            if (sdata.skillJson.name.StartsWith("card"))
                continue;
            //normal skill effects
            AddEffect(sdata.skillJson.name, sdata.skillJson.actioneffect, true);
            AddEffect(sdata.skillJson.name+"_gethit", sdata.skillJson.effectgethit, true);
        }

        foreach(KeyValuePair<int, SideEffectJson> entry in SideEffectRepo.mIdMap)
        {
            SideEffectJson sdata = entry.Value;
            AddEffect(sdata.effectpath, sdata.name, true);
        }

        AddEffect("levelup", "Effects_ZDSP_characters/buff/prefab/level_up.prefab", true);
        /*AddEffect(EffectVisualTypes.Stun.ToString(), "Effects_piliq/characters/sw/prefab/sw_stun.prefab");//TODO: replace this se with the correct effect
        AddEffect(EffectVisualTypes.Slow.ToString(), "Effects_generic/prefab/bleed.prefab");
        AddEffect(EffectVisualTypes.Slience.ToString(), "Effects_generic/prefab/bleed.prefab");
        AddEffect(EffectVisualTypes.Root.ToString(), "Effects_piliq/monster/mon_001/prefab/mon_001_run.prefab");*/
        //AddEffect("invincible", "Effects_piliq/characters/sw/prefab/sw_stun.prefab");
        //AddEffect("flashupkn", "Effects_piliq_characters/kn/prefab/kn_dodge1.prefab", true);
        //AddEffect("flashdownkn", "Effects_piliq_characters/kn/prefab/kn_dodge2.prefab", true);
        //AddEffect("flashupha", "Effects_piliq_characters/ha/prefab/ha_dodge1.prefab", true);
        //AddEffect("flashdownha", "Effects_piliq_characters/ha/prefab/ha_dodge2.prefab", true);
        //AddEffect("flashupsp", "Effects_piliq_characters/sp/prefab/sp_dodge1.prefab", true);
        //AddEffect("flashdownsp", "Effects_piliq_characters/sp/prefab/sp_dodge2.prefab", true);
        //AddEffect("flashupsw", "Effects_piliq_characters/sw/prefab/sw_dodge1.prefab", true);
        //AddEffect("flashdownsw", "Effects_piliq_characters/sw/prefab/sw_dodge2.prefab", true);
        //AddEffect("stunkn", "Effects_piliq_characters/kn/prefab/kn_stun.prefab", true);
        //AddEffect("stunha", "Effects_piliq_characters/ha/prefab/ha_stun.prefab", true);
        //AddEffect("stunsp", "Effects_piliq_characters/sp/prefab/sp_stun.prefab", true);
        //AddEffect("stunsw", "Effects_piliq_characters/sw/prefab/sw_stun.prefab", true);
        //AddEffect("runkn", "Effects_piliq_characters/kn/prefab/kn_run.prefab", true);
        //AddEffect("runha", "Effects_piliq_characters/ha/prefab/ha_run.prefab", true);
        //AddEffect("runsp", "Effects_piliq_characters/sp/prefab/sp_run.prefab", true);
        //AddEffect("runsw", "Effects_piliq_characters/sw/prefab/sw_run.prefab", true);
        //AddEffect("showkn", "Effects_piliq_characters/kn/prefab/kn_show.prefab", true);
        //AddEffect("showha", "Effects_piliq_characters/ha/prefab/ha_show.prefab", true);
        //AddEffect("showsp", "Effects_piliq_characters/sp/prefab/sp_show.prefab", true);
        //AddEffect("showsw", "Effects_piliq_characters/sw/prefab/sw_show.prefab", true);

        //AddEffect("mount", "Effects_piliq_mount/prefab/change.prefab", true);
        //AddEffect("unmount", "Effects_piliq_mount/prefab/remove.prefab", true);

        /*foreach (KeyValuePair<int, NPCJson> entry in NPCRepo.mIdMap)
        {
            if (entry.Value.knockupeffect != string.Empty)
                AddEffect(entry.Value.archetype + "knockup2", entry.Value.knockupeffect);
            if (entry.Value.dyingeffect != string.Empty)
                AddEffect(entry.Value.archetype + "dying", entry.Value.dyingeffect);
            if (entry.Value.runeffect != string.Empty)
                AddEffect(entry.Value.archetype + "running", entry.Value.runeffect);
        }*/
        foreach (KeyValuePair<int, SideEffectJson> entry in SideEffectRepo.mIdMap)
        {
            SideEffectJson sideeffect = entry.Value;
            AddEffect(sideeffect.name, sideeffect.effectpath);
        }
        //foreach (KeyValuePair<int, StaticNPCJson> entry in StaticNPCRepo.mIdMap)
        //{
            //entry.Value.standbyeffect = "Effects_piliq_snpc/prefab/snpc_008_idle.prefab";
            //entry.Value.idleeffect = "Effects_piliq_snpc/prefab/snpc_003_idle.prefab";
            //if (!string.IsNullOrEmpty(entry.Value.idleeffect))
            //{
            //    AddEffect(entry.Value.archetype + "_idle", entry.Value.idleeffect);
            //}
            //if (!string.IsNullOrEmpty(entry.Value.standbyeffect))
            //{
            //    AddEffect(entry.Value.archetype + "_standby", entry.Value.standbyeffect);
            //}
        //}

        foreach(var entry in HeroRepo.heroes.Values)
        {
            AddEffect(entry.name, entry.summoneffect, true);
        }
    }

    /// <summary>
    /// add in the effect path with a unique key name, for zdsp, it will be use skilljson.name
    /// passing preload as true will cache the prefab in memeory which is not wise, 
    /// only consider preload effects which is common.  
    /// for the not preload effect, Async effect is loaded at use time, so the firstime for a effect play may not show,
    /// but consective play of the same effect will show.  
    ///  
    /// </summary>
    /// <param name="effectKey"></param>
    /// <param name="effectPath"></param>
    /// <param name="preloaded"></param>
    private void AddEffect(string effectKey, string effectPath, bool preloaded = false)
    {
        if (string.IsNullOrEmpty(effectPath))
            return;
        if (effectPath.Contains("{0}"))
            return;
        if (mEffectPaths.ContainsKey(effectKey))
            return;
        mEffectPaths.Add(effectKey, effectPath);
        if (preloaded)
        {
            if (!mPreloadedEffects.Contains(effectPath))
            {
                mPreloadedEffects.Add(effectPath);
            }
        }
    }

    public void ClearAssets()
    {
        List<string> listtoclear = new List<string>();
        foreach (KeyValuePair<string, GameObject> entry in mEfxPrefabList)
        {
            if (playerRGBSkill.ContainsValue(entry.Key))
                continue;
            if (mPreloadedEffects.Contains(entry.Key))
                continue;
            listtoclear.Add(entry.Key);
        }

        foreach (string effkey in listtoclear)
        {
            mEfxPrefabList.Remove(effkey);
        }
    }

    public void CachePlayerHeroSkills(byte job, params int[] skillgroupids)
    {
        StartCoroutine(LoadingPlayerCardsEffect(job, skillgroupids));
    }

    IEnumerator LoadingPlayerCardsEffect(byte job, params int[] skillgroupids)
    {
        yield return null;
        string ext = suffex[job - 1];
        //todo :for simplicity, now each card we load 4 set of effects for different jobsect, which is not neccessary
        //foreach (string ext in suffex)
        {
            foreach (int skillgroupid in skillgroupids)
            {
                SkillData sData = SkillRepo.GetSkillByGroupID(skillgroupid);
                if (sData == null)
                    continue;
                string eff1 = sData.skillgroupJson.name + ext;
                string eff2 = sData.skillgroupJson.name + "_gethit" + ext;
                GetEffectByName(eff1);
                GetEffectByName(eff2); //this will preload skill effect.

            }
        }

    }

    public void CacheLocalPlayerCardSkill(byte job, params int[] listid)
    {
        playerRGBSkill.Clear();
        string ext = suffex[job - 1];
        //todo :for simplicity, now each card we load 4 set of effects for different jobsect, which is not neccessary
        //foreach(string ext in suffex)
        {
            foreach (int skillid in listid)
            {
                if (skillid <= 0)
                    continue;
                SkillData sData = SkillRepo.GetSkillByGroupID(skillid);
                string eff1 = sData.skillgroupJson.name + ext;
                string eff2 = sData.skillgroupJson.name + "_gethit" + ext;
                if (mEffectPaths.ContainsKey(eff1))
                {
                    playerRGBSkill.Add(eff1, mEffectPaths[eff1]);
                    GetEffectByPath(mEffectPaths[eff1]);
                }
                if (mEffectPaths.ContainsKey(eff2))
                {
                    playerRGBSkill.Add(eff2, mEffectPaths[eff2]);
                    GetEffectByPath(mEffectPaths[eff2]);
                }

            }

        }

    }
    private Dictionary<string, string> playerRGBSkill;

    protected int totalcount = 0;
    protected int loadedcount = 0;
    protected int loadingNum = 0;
    public bool preloadDone = false;
    public IEnumerator PreloadedEffects(Action<float> loadingProgress)
    {
        if (preloadDone || mPreloadedEffects.Count == 0)
            yield break;
        loadedcount = 0;
        totalcount = mPreloadedEffects.Count;
        int maxLoading = 20;
        int loadingIndex = 0;
        while (!preloadDone)
        {
            if (loadingNum < maxLoading && loadingIndex < totalcount)
            {
                AssetLoader.Instance.LoadAsync<GameObject>(mPreloadedEffects[loadingIndex], (inst, info) => { CacheEffect(inst, info, loadingProgress); }, false);
                loadingIndex++;
                loadingNum++;
            }
            else
                yield return null;
        }
        yield break;
    }


    /// <summary>
    /// suffex is for key suffex, effSuffex is for effect path suffex;
    /// </summary>
    /// <param name="skillgroup"></param>
    /// <param name="suffex"></param>
    /// <param name="effSuffex"></param>
    private void AddSkillgroupEffect(SkillGroupJson skillgroup, string suffex = "", string effSuffex = "", bool preload = false)
    {
        //effect format for herocard . 
         

         
    }

    public void CacheEffect(GameObject inst, LoadedAssetInfo info, Action<float> loadingProgress = null)
    {
        if (!preloadDone)
        {
            loadedcount++;
            loadingNum--;
            loadingProgress((float)loadedcount / totalcount);
            //Debug.Log("loading progress: " + loadedcount + "/" + totalcount);
            if (loadedcount == totalcount)
            {
                Debug.Log("loading progress: " + loadedcount + "/" + totalcount);
                preloadDone = true;
            }
        }
        string fullpath = info.container + "/" + info.assetPath;
        if (!mEfxPrefabList.ContainsKey(fullpath))
        {
            mEfxPrefabList.Add(fullpath, inst);
            if (inst == null)
            {
#if UNITY_EDITOR
                //load may fail as prefab may not exist;
                Debug.LogError("Failed to load effect " + fullpath);
#endif
            }
        }
    }

    public GameObject GetEffectByPath(string effectpath)
    {
        if (mEfxPrefabList.ContainsKey(effectpath))
        {
            return mEfxPrefabList[effectpath];
        }
        AssetLoader.Instance.LoadAsync<GameObject>(effectpath, (inst, info) => { CacheEffect(inst, info); }, false);
        return null;
    }
    public GameObject GetEffectByName(string efxName)
    {
        GameObject _parent = this.gameObject;

        if (mEffectPaths.ContainsKey(efxName))
        {
            string effectpath = mEffectPaths[efxName];
            return GetEffectByPath(effectpath);
        }
        return null;
    }

    public void OnDestroyEfxSystem()
    {
        //this is called when game quit. 
        mEffectPaths = null;
        if (mPreloadedEffects != null)
            mPreloadedEffects.Clear();
        mPreloadedEffects = null;
        if (mEfxPrefabList != null)
            mEfxPrefabList.Clear();
        mEfxPrefabList = null;
    }
}
