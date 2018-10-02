using Kopio.JsonContracts;
using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;

[Serializable]
public class AvatarAttachedPart
{
    public string PartName;
    public Transform AttachedTo;

    [HideInInspector]
    public GameObject AttachedObj;
    [HideInInspector]
    public string PrefabLoadingPath = "";
    [HideInInspector]
    public bool show = true;

#if UNITY_EDITOR
    public GameObject TestPrefab;
#endif

    public void Show(bool show)
    {
        this.show = show;
        if (AttachedObj != null)
            AttachedObj.SetActive(show);
    }

    public void Attach(GameObject obj)
    {
        AttachedObj = obj;
        AttachedObj.SetActive(show);
    }

    public void Detach()
    {
        if (AttachedObj != null)
        {
            UnityEngine.Object.Destroy(AttachedObj);
            AttachedObj = null;
        }
    }
}

[Serializable]
public class AvatarSkinPart
{
    public string PartName;
    public SkinnedMeshRenderer SkinMesh = null;

    [HideInInspector]
    public string MeshLoadingPath = "";
    [HideInInspector]
    public string MaterialLoadingPath = "";
    [HideInInspector]
    public bool show = true;

    public void ResetToDefault()
    {
        MeshLoadingPath = "";
        MaterialLoadingPath = "";
        SkinMesh.sharedMesh = null;
        SkinMesh.sharedMaterial = null;
    }

    public void Show(bool show)
    {
        this.show = show;
        SkinMesh.gameObject.SetActive(show);
    }
}

public class AvatarController : MonoBehaviour
{
    public bool artistTest;

    [SerializeField]
    AvatarAttachedPart[] AttachedParts = null;

    [SerializeField]
    AvatarSkinPart[] SkinParts = null;

    bool isDestroyed = false;

#if UNITY_EDITOR
    void Start()
    {
        if (artistTest == true && GameInfo.gLocalPlayer == null)
            TestAttachment();
    }

    //artist testing part
    public void TestAttachment()
    {
        for (int index = 0; index < AttachedParts.Length; index++)
        {
            AvatarAttachedPart obj = AttachedParts[index];
            if (obj.TestPrefab != null)
            {
                GameObject test = Instantiate(obj.TestPrefab);
                test.transform.SetParent(obj.AttachedTo, false);
                obj.Attach(test);
            }
        }
    }
#endif

    public static string GetEquipmentPartName(PartsType type)
    {
        switch (type)
        {
            case PartsType.Sword:
            case PartsType.Blade:
            case PartsType.Lance:
            case PartsType.Hammer:
            case PartsType.Fan:
            case PartsType.Xbow:
            case PartsType.Dagger:
            case PartsType.Sanxian:
                return "weapon_r";
            case PartsType.Helm:
                return "helm";
            case PartsType.Body:
                return "body";
            case PartsType.Wing:
                return "back";
            default:
                return "";
        }
    }

    public static string GetEquipmentPartName(EquipmentSlot type)
    {
        switch (type)
        {
            case EquipmentSlot.Weapon:
                return "weapon_r";
            case EquipmentSlot.Helm:
                return "helm";
            case EquipmentSlot.Body:
                return "body";
            case EquipmentSlot.Back:
                return "back";
        }
        return "";
    }

    public static string GetFashionPartName(FashionSlot slot)
    {
        switch (slot)
        {
            case FashionSlot.Helm:
                return "helm";
            case FashionSlot.Weapon:
                return "weapon_r";
            case FashionSlot.Body:
                return "body";
            case FashionSlot.Back:
                return "back";
        }
        return "";
    }

    public void InitAvatar(EquipmentInventoryData equipmentInvData, JobType jobtype, Gender gender)
    {
        List<Equipment> fashionSlots = equipmentInvData.FashionSlots;
        List<Equipment> equipmentSlots = equipmentInvData.Slots;

        //weapon
        Equipment fashion_weapon = fashionSlots[(int)FashionSlot.Weapon];
        Equipment equip_weapon = equipmentSlots[(int)EquipmentSlot.Weapon];
        if (equip_weapon == null)
            Unequip("weapon_r");
        else if (fashion_weapon != null && fashion_weapon.EquipmentJson.partstype == equip_weapon.EquipmentJson.partstype)
            OnWeaponChanged(fashion_weapon.EquipmentJson.prefabpath);
        else
            OnWeaponChanged(equip_weapon.EquipmentJson.prefabpath);

        //Back
        Equipment fashion_back = fashionSlots[(int)FashionSlot.Back];
        Equipment equip_back = equipmentSlots[(int)EquipmentSlot.Back];
        if (fashion_back != null)
            OnBackChanged(fashion_back.EquipmentJson.prefabpath);
        else if (equip_back != null)
            OnBackChanged(equip_back.EquipmentJson.prefabpath);
        else
            Unequip("back");
        
        //Helm
        Equipment fashion_helm = fashionSlots[(int)FashionSlot.Helm];
        Equipment equip_helm = equipmentSlots[(int)EquipmentSlot.Helm];
        if (equipmentInvData.HideHelm)
            EquipDefaultHelm(0, gender);
        else if (fashion_helm != null)
            EquipHelm(fashion_helm.EquipmentJson, 0, gender);
        else if (equip_helm != null)
            EquipHelm(equip_helm.EquipmentJson, 0, gender);
        else
            EquipDefaultHelm(0, gender);

        //Body
        Equipment fashion_body = fashionSlots[(int)FashionSlot.Body];
        if (fashion_body != null)
            OnSkinChanged("body", fashion_body.EquipmentJson, gender);
        else
            EquipJobBody(jobtype, gender);
    }

    public void EquipJobBody(JobType jobtype, Gender gender)
    {
        JobsectJson jobsectJson = JobSectRepo.GetJobByType(jobtype);
        if (gender == Gender.Male)
            OnSkinChanged("body", jobsectJson.malemeshpath, jobsectJson.malematerialpath);
        else
            OnSkinChanged("body", jobsectJson.femalemeshpath, jobsectJson.femalematerialpath);
    }

    public void EquipDefaultHelm(int hairStyle, Gender gender)
    {
        Unequip("accessory");
        EquipDefaultHair(hairStyle, gender);
    }

    private void EquipDefaultHair(int hairStyle, Gender gender)
    {
        //todo: base on hairStyle selected in character creation, get path from kopio
        if (gender == Gender.Male)
            OnSkinChanged("helm", "Models_Characters/Pc_job/t3_bladeMaster_male_head.fbx", "Models_Characters/Pc_job/Materials/t3_bladeMaster_male_head.mat");
        else
            OnSkinChanged("helm", "Models_Characters/Pc_job/t3_bladeMaster_female_head.fbx", "Models_Characters/Pc_job/Materials/t3_bladeMaster_female_head.mat");
    }

    public void EquipHelm(EquipmentJson equipmentJson, int hairStyle, Gender gender)
    {
        string accessory_prefabpath = gender == Gender.Male ? equipmentJson.prefabpath : equipmentJson.femaleprefabpath;
        if (string.IsNullOrEmpty(accessory_prefabpath))
            Unequip("accessory");
        else
            OnAccessoryChanged(accessory_prefabpath);

        string meshpath = (gender == Gender.Male) ? equipmentJson.malemeshpath : equipmentJson.femalemeshpath;
        if (string.IsNullOrEmpty(meshpath))
            EquipDefaultHair(hairStyle, gender);
        else
            OnSkinChanged("helm", equipmentJson, gender);
    }

    public void OnWeaponChanged(string prefabpath)
    {
        AvatarAttachedPart attachedPart = GetAttachedPart("weapon_r");
        if (attachedPart == null)
            return;
        if (prefabpath == attachedPart.PrefabLoadingPath) //same prefab as currently loaded/loading
            return;
        attachedPart.PrefabLoadingPath = prefabpath;
        AssetLoader.Instance.LoadAsync<GameObject>(prefabpath, (prefab) => OnWeaponLoaded(prefab, prefabpath, attachedPart));
    }

    public void OnBackChanged(string prefabpath)
    {
        AvatarAttachedPart attachedPart = GetAttachedPart("back");
        if (attachedPart == null)
            return;
        if (prefabpath == attachedPart.PrefabLoadingPath) //same prefab as currently loaded/loading
            return;
        attachedPart.PrefabLoadingPath = prefabpath;
        AssetLoader.Instance.LoadAsync<GameObject>(prefabpath, (prefab) => OnPrefabLoaded(prefab, prefabpath, attachedPart));
    }

    public void OnAccessoryChanged(string prefabpath)
    {
        AvatarAttachedPart attachedPart = GetAttachedPart("accessory");
        if (attachedPart == null)
            return;
        if (prefabpath == attachedPart.PrefabLoadingPath) //same prefab as currently loaded/loading
            return;
        attachedPart.PrefabLoadingPath = prefabpath;
        AssetLoader.Instance.LoadAsync<GameObject>(prefabpath, (prefab) => OnPrefabLoaded(prefab, prefabpath, attachedPart));
    }

    public void OnSkinChanged(string partName, EquipmentJson equipmentJson, Gender gender)
    {
        if (gender == Gender.Male)
            OnSkinChanged(partName, equipmentJson.malemeshpath, equipmentJson.malematerialpath);
        else
            OnSkinChanged(partName, equipmentJson.femalemeshpath, equipmentJson.femalematerialpath);
    }

    public void OnSkinChanged(string partName, string meshpath, string materialpath)
    {
        AvatarSkinPart skinPart = GetSkinPart(partName);
        if (skinPart == null)
            return;
        if (meshpath != skinPart.MeshLoadingPath)
        {
            skinPart.MeshLoadingPath = meshpath;
            AssetLoader.Instance.LoadAsync<Mesh>(meshpath, (mesh) => OnMeshLoaded(mesh, meshpath, skinPart));
        }
        if (materialpath != skinPart.MaterialLoadingPath)
        {
            skinPart.MaterialLoadingPath = materialpath;
            AssetLoader.Instance.LoadAsync<Material>(materialpath, (material) => OnMaterialLoaded(material, materialpath, skinPart));
        }
    }

    public void Unequip(string partName)
    {
        switch(partName)
        {
            case "weapon_r":
                AvatarAttachedPart attachedPart = GetAttachedPart(partName);
                if (attachedPart == null)
                    return;
                attachedPart.PrefabLoadingPath = "";
                DetachFromBodyPart(attachedPart);
                DetachFromBodyPart("weapon_l");
                break;
            case "back":
                attachedPart = GetAttachedPart(partName);
                if (attachedPart == null)
                    return;
                attachedPart.PrefabLoadingPath = "";
                DetachFromBodyPart(attachedPart);
                break;
            case "helm":
                AvatarSkinPart skinPart = GetSkinPart(partName);
                if (skinPart == null)
                    return;
                skinPart.ResetToDefault();
                break;
            case "accessory":
                attachedPart = GetAttachedPart(partName);
                if (attachedPart == null)
                    return;
                attachedPart.PrefabLoadingPath = "";
                DetachFromBodyPart(attachedPart);
                break;
            case "body":
                break;
        }
    }

    private void OnWeaponLoaded(GameObject prefab, string path, AvatarAttachedPart attachedPart)
    {
        if (isDestroyed || attachedPart.PrefabLoadingPath != path)
            return;
        DetachFromBodyPart(attachedPart);
        DetachFromBodyPart("weapon_l");

        if (prefab == null)
        {
            Debug.LogErrorFormat("OnWeaponLoaded prefab path {0} not found!!!", path);
            return;
        }

        GameObject attachedObj = Instantiate(prefab);
        ClientUtils.SetLayerRecursively(attachedObj, gameObject.layer);
        attachedObj.transform.SetParent(attachedPart.AttachedTo, false);
        attachedPart.Attach(attachedObj);

        Transform root2 = attachedObj.transform.Find("root2");
        if (root2 != null) //one weapon each hand
        {
            var part_l = GetAttachedPart("weapon_l");
            if (part_l != null)
            {
                root2.SetParent(part_l.AttachedTo, false);
                part_l.Attach(root2.gameObject);
            }
        }
    }

    private void OnPrefabLoaded(GameObject prefab, string path, AvatarAttachedPart attachedPart)
    {
        if (isDestroyed || attachedPart.PrefabLoadingPath != path)
            return;
        DetachFromBodyPart(attachedPart);
        if (prefab == null)
        {
            Debug.LogErrorFormat("OnPrefabLoaded prefab path {0} not found!!!", path);
            return;
        }

        GameObject attachedObj = Instantiate(prefab);
        ClientUtils.SetLayerRecursively(attachedObj, gameObject.layer);
        attachedObj.transform.SetParent(attachedPart.AttachedTo, false);
        attachedPart.Attach(attachedObj);
    }

    private void OnMeshLoaded(Mesh mesh, string path, AvatarSkinPart skinPart)
    {
        if (isDestroyed || skinPart.MeshLoadingPath != path)
            return;
        skinPart.SkinMesh.sharedMesh = mesh;
        if (mesh == null)
        {
            Debug.LogErrorFormat("OnMeshLoaded mesh path {0} not found!!!", path);
            return;
        }
    }

    private void OnMaterialLoaded(Material material, string path, AvatarSkinPart skinPart)
    {
        if (isDestroyed || skinPart.MaterialLoadingPath != path)
            return;
        skinPart.SkinMesh.sharedMaterial = material;
        if (material == null)
        {
            Debug.LogErrorFormat("OnMaterialLoaded material path {0} not found!!!", path);
            return;
        }
    }

    public void HideWeapon(bool hide)
    {
        AvatarAttachedPart weapon_r = GetAttachedPart("weapon_r");
        if (weapon_r != null)
            weapon_r.Show(!hide);
        AvatarAttachedPart weapon_l = GetAttachedPart("weapon_l");
        if (weapon_l != null)
            weapon_l.Show(!hide);
    }

    public void DetachFromBodyPart(string partname)
    {
        AvatarAttachedPart attachedPart = Array.Find(AttachedParts, o => o.PartName == partname);
        if (attachedPart != null)
            DetachFromBodyPart(attachedPart);
    }

    private void DetachFromBodyPart(AvatarAttachedPart attachedPart)
    {
        attachedPart.Detach();
    }

    public AvatarAttachedPart GetAttachedPart(string partname)
    {
        return Array.Find(AttachedParts, o => o.PartName == partname);
    }

    public AvatarSkinPart GetSkinPart(string partname)
    {
        return Array.Find(SkinParts, o => o.PartName == partname);
    }

    void OnDestroy()
    {
        isDestroyed = true;
    }
}