using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Repository;

public class EfxSystem : MonoSingleton<EfxSystem>
{
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
        mEfxPrefabList = new Dictionary<string, GameObject>();
        mEffectPaths = new Dictionary<string, string>();
        mPreloadedEffects = new List<string>();

        foreach (KeyValuePair<int, SkillData> entry in SkillRepo.mSkills)
        {
            SkillData sdata = entry.Value;
            AddEffect(sdata.skillJson.name, sdata.skillJson.actioneffect, true);
            AddEffect(sdata.skillJson.name + "_gethit", sdata.skillJson.effectgethit, true);
        }

        foreach (KeyValuePair<int, SideEffectJson> entry in SideEffectRepo.mIdMap)
        {
            SideEffectJson sideeffect = entry.Value;
            AddEffect(sideeffect.name, sideeffect.effectpath, true);
        }

        foreach (var entry in HeroRepo.heroes.Values)
        {
            AddEffect(entry.name, entry.summoneffect, true);
        }

        AddEffect("levelup", "Effects_ZDSP_characters/buff/prefab/level_up.prefab", true);
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
        if (mEffectPaths.ContainsKey(effectKey))
            return;
        mEffectPaths.Add(effectKey, effectPath);
        if (preloaded && !mPreloadedEffects.Contains(effectPath))
        {
            mPreloadedEffects.Add(effectPath);
        }
    }

    public void ClearAssets()
    {
        List<string> listtoclear = new List<string>();
        foreach (KeyValuePair<string, GameObject> entry in mEfxPrefabList)
        {
            if (mPreloadedEffects.Contains(entry.Key))
                continue;
            listtoclear.Add(entry.Key);
        }

        foreach (string effkey in listtoclear)
        {
            mEfxPrefabList.Remove(effkey);
        }
    }

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
                AssetLoader.Instance.LoadAsync<GameObject>(mPreloadedEffects[loadingIndex], (inst, info) => CacheEffect(inst, info, loadingProgress), false);
                loadingIndex++;
                loadingNum++;
            }
            else
                yield return null;
        }
        yield break;
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
                Debug.LogWarning("Failed to load effect " + fullpath);
#endif
            }
        }
    }

    public GameObject GetEffectByPath(string effectpath)
    {
        GameObject effectPrefab;
        if (mEfxPrefabList.TryGetValue(effectpath, out effectPrefab))
            return effectPrefab;

        AssetLoader.Instance.LoadAsync<GameObject>(effectpath, (inst, info) => CacheEffect(inst, info), false);
        return null;
    }

    public GameObject GetEffectByName(string efxName)
    {
        string effectpath;
        if (mEffectPaths.TryGetValue(efxName, out effectpath))
            return GetEffectByPath(effectpath);
        return null;
    }

    public void OnDestroyEfxSystem()
    {
        mEffectPaths.Clear();
        mEffectPaths = null;
        mPreloadedEffects.Clear();
        mPreloadedEffects = null;
        mEfxPrefabList.Clear();
        mEfxPrefabList = null;
    }
}