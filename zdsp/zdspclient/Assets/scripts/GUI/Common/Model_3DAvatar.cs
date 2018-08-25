using Kopio.JsonContracts;
using System;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class Model_3DAvatar : MonoBehaviour
{
    [SerializeField]
    Transform modelParent;

    GameObject model;
    string modelpath = "";

    public void Change(EquipmentInventoryData equipmentInvData, JobType jobtype, Gender gender, Action<GameObject> afterLoad = null)
    {
        string prefabPath = JobSectRepo.GetGenderInfo(gender).modelpath;
        if (model != null && modelpath != prefabPath)
        {
            Destroy(model);
            model = null;
            modelpath = "";
        }

        if (model == null)
        {
            GameObject prefab = AssetLoader.Instance.Load<GameObject>(prefabPath);
            model = GameObject.Instantiate(prefab);
            CharacterController charCtrl = model.GetComponent<CharacterController>();
            if (charCtrl != null)
                Destroy(charCtrl);
            modelpath = prefabPath;
            model.transform.SetParent(modelParent, false);
            ClientUtils.SetLayerRecursively(model, LayerMask.NameToLayer("UI"));
        }

        model.GetComponent<AvatarController>().InitAvatar(equipmentInvData, jobtype, gender);
        var _weapon = equipmentInvData.Slots[(int)EquipmentSlot.Weapon];
        PartsType _weaponType = (_weapon != null) ? _weapon.EquipmentJson.partstype : PartsType.Blade;
        model.GetComponent<Animator>().PlayFromStart(ClientUtils.GetStandbyAnimationByWeaponType(_weaponType));
        if (afterLoad != null)
            afterLoad(model);
    }

    private HeroJson heroJson;

    public bool ChangeHero(int heroId, int tier)
    {
        heroJson = HeroRepo.GetHeroById(heroId);
        if (heroJson == null)
            return false;

        string prefabPath = "";
        switch (tier)
        {
            case 1: prefabPath = heroJson.t1modelpath; break;
            case 2: prefabPath = heroJson.t2modelpath; break;
            case 3: prefabPath = heroJson.t3modelpath; break;
            default:
                HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
                if (skinItem != null)
                    prefabPath = skinItem.HeroItemJson.heroskinpath;
                break;
        }

        if (string.IsNullOrEmpty(prefabPath))
            return false;

        if (model != null && modelpath != prefabPath)
        {
            Destroy(model);
            model = null;
            modelpath = "";
        }

        if (model == null)
        {
            AssetLoader.Instance.LoadAsync<GameObject>(prefabPath, OnModelLoaded, true);
            modelpath = prefabPath;
        }
        return true;
    }

    public void Change(string prefabPath, Action<GameObject> afterLoad = null)
    {
        if (model != null && modelpath != prefabPath)
        {
            Destroy(model);
            model = null;
            modelpath = "";
        }

        if (model == null)
        {
            AssetLoader.Instance.LoadAsync<GameObject>(prefabPath, (obj) =>
            {
                OnModelLoaded(obj);
                if (model != null && afterLoad != null)
                    afterLoad(model);
            }, true);
            modelpath = prefabPath;
        }
    }

    private void OnModelLoaded(GameObject obj)
    {
        if (obj != null)
        {
            model = Instantiate(obj);
            CharacterController charCtrl = model.GetComponent<CharacterController>();
            if (charCtrl != null)
                Destroy(charCtrl);
            if (heroJson != null)
                model.transform.localScale = new Vector3(heroJson.modelscalex, heroJson.modelscaley, heroJson.modelscalez);
            model.transform.SetParent(modelParent, false);
            ClientUtils.SetLayerRecursively(model, LayerMask.NameToLayer("UI"));
        }
    }

    public void Cleanup()
    {
        if (model != null)
            Destroy(model);
        model = null;
        modelpath = "";
    }

    public GameObject GetOutfitModel()
    {
        return model;
    }

    public void PlayAnimation(string animation)
    {
        if (model != null)
            model.GetComponent<Animator>().PlayFromStart(animation);
    }
}