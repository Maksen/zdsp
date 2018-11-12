using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
        List<int> appearanceSlots = equipmentInvData.AppearanceSlots;

        List<Equipment> bathrobes = fashionSlots.Where(e => e != null && e.EquipmentJson.partstype == PartsType.Bathrobe).ToList();
        if (bathrobes.Count > 0)
        {
            Unequip("weapon_r");
            Unequip("helm");
            Unequip("body");
            Unequip("back");

            Equipment bathrobe = bathrobes.First();
            int skincolor = appearanceSlots[(int)AppearanceSlot.SkinColor];
            int hairstyle = appearanceSlots[(int)AppearanceSlot.HairStyle];
            int haircolor = appearanceSlots[(int)AppearanceSlot.HairColor];

            OnSkinChanged("body", GetMeshPathByGender(bathrobe.EquipmentJson, gender), GetMaterialPathByGender(bathrobe.EquipmentJson, gender), GetColorCodeByPart("body", skincolor));
            EquipDefaultHair(hairstyle, haircolor);
        }
        else
        {
            InitAvatarWeapon(fashionSlots[(int)FashionSlot.Weapon], equipmentSlots[(int)EquipmentSlot.Weapon], gender);
            InitAvatarBack(fashionSlots[(int)FashionSlot.Back], equipmentSlots[(int)EquipmentSlot.Back], gender);
            InitAvatarHelm(fashionSlots[(int)FashionSlot.Helm], equipmentSlots[(int)EquipmentSlot.Helm], appearanceSlots[(int)AppearanceSlot.HairStyle], appearanceSlots[(int)AppearanceSlot.HairColor], equipmentInvData.HideHelm, gender);
            InitAvatarBody(fashionSlots[(int)FashionSlot.Body], equipmentSlots[(int)EquipmentSlot.Body], appearanceSlots[(int)AppearanceSlot.SkinColor], gender, jobtype);
            InitAvatarFace(appearanceSlots[(int)AppearanceSlot.MakeUp], appearanceSlots[(int)AppearanceSlot.SkinColor], gender);
        }
    }

    public void InitCreationAvatar(EquipmentInventoryData equipmentInvData, JobType jobtype, Gender gender, int outfit)
    {
        List<Equipment> fashionSlots = equipmentInvData.FashionSlots;
        List<Equipment> equipmentSlots = equipmentInvData.Slots;
        List<int> appearanceSlots = equipmentInvData.AppearanceSlots;

        InitSpecialOutfit(outfit, appearanceSlots[(int)AppearanceSlot.SkinColor], gender);
        InitAvatarHelm(fashionSlots[(int)FashionSlot.Helm], equipmentSlots[(int)EquipmentSlot.Helm], appearanceSlots[(int)AppearanceSlot.HairStyle], appearanceSlots[(int)AppearanceSlot.HairColor], equipmentInvData.HideHelm, gender);
        InitAvatarFace(appearanceSlots[(int)AppearanceSlot.MakeUp], appearanceSlots[(int)AppearanceSlot.SkinColor], gender);
    }

    Dictionary<int, bool> mWaitingList = new Dictionary<int, bool>();
    UnityAction mCallback = null;

    public void InitSelectionAvatar(EquipmentInventoryData equipmentInvData, JobType jobtype, Gender gender, int outfit, UnityAction callback)
    {
        List<Equipment> fashionSlots = equipmentInvData.FashionSlots;
        List<Equipment> equipmentSlots = equipmentInvData.Slots;
        List<int> appearanceSlots = equipmentInvData.AppearanceSlots;
        mWaitingList = new Dictionary<int, bool>();
        mWaitingList.Add(0, false);
        mWaitingList.Add(1, false);
        mWaitingList.Add(2, false);
        mCallback = callback;

        InitAvatarBody(fashionSlots[(int)FashionSlot.Body], equipmentSlots[(int)EquipmentSlot.Body], appearanceSlots[(int)AppearanceSlot.SkinColor], gender, jobtype);
        InitAvatarHelm(fashionSlots[(int)FashionSlot.Helm], equipmentSlots[(int)EquipmentSlot.Helm], appearanceSlots[(int)AppearanceSlot.HairStyle], appearanceSlots[(int)AppearanceSlot.HairColor], equipmentInvData.HideHelm, gender);
        InitAvatarFace(appearanceSlots[(int)AppearanceSlot.MakeUp], appearanceSlots[(int)AppearanceSlot.SkinColor], gender);
    }

    public void InitSpecialOutfit(int outfit, int skincolor, Gender gender)
    {
        string meshpath = GetOutfitMeshPath(outfit, gender);
        string materialpath = GetOutfitMaterialPath(outfit, gender);
        OnSkinChanged("body", meshpath, materialpath, GetColorCodeByPart("body", skincolor));
    }

    private string GetOutfitMeshPath(int outfit, Gender gender)
    {
        switch(outfit)
        {
            case 1:
                return gender == Gender.Male ? "Models_Characters/Pc_job/t3_general_male_body.fbx" : "Models_Characters/Pc_job/t3_general_female_body.fbx";
            case 2:
                return gender == Gender.Male ? "Models_Characters/Pc_job/t3_bladeMaster_male_body.fbx" : "Models_Characters/Pc_job/t3_executioner_female_body.fbx";
            case 3:
                return gender == Gender.Male ? "Models_Characters/Pc_fashion/fa_001_agent_male_body.fbx" : "Models_Characters/Pc_fashion/fa_001_agent_female_body.fbx";
            default:
                return gender == Gender.Male ? "Models_Characters/Pc_job/t3_bladeMaster_male_body.fbx" : "Models_Characters/Pc_job/t3_bladeMaster_female_body.fbx";
        }
    }

    private string GetOutfitMaterialPath(int outfit, Gender gender)
    {
        switch (outfit)
        {
            case 1:
                return gender == Gender.Male ? "Models_Characters/Pc_job/Materials/t3_general_male_body.mat" : "Models_Characters/Pc_job/Materials/t3_general_female_body.mat";
            case 2:
                return gender == Gender.Male ? "Models_Characters/Pc_job/Materials/t3_bladeMaster_male_body.mat" : "Models_Characters/Pc_job/Materials/t3_executioner_female_body.mat";
            case 3:
                return gender == Gender.Male ? "Models_Characters/Pc_fashion/Materials/fa_001_agent_male_body.mat" : "Models_Characters/Pc_fashion/Materials/fa_001_agent_female_body.mat";
            default:
                return gender == Gender.Male ? "Models_Characters/Pc_job/Materials/t3_bladeMaster_male_body.mat" : "Models_Characters/Pc_job/Materials/t3_bladeMaster_female_body.mat";
        }
    }

    private void InitAvatarWeapon(Equipment fashionweapon, Equipment weapon, Gender gender)
    {
        if (weapon == null)
            Unequip("weapon_r");
        else if (fashionweapon != null && fashionweapon.EquipmentJson.partstype == weapon.EquipmentJson.partstype)
            OnWeaponChanged(GetPrefabPathByGender(fashionweapon.EquipmentJson, gender));
        else
            OnWeaponChanged(GetPrefabPathByGender(weapon.EquipmentJson, gender));
    }

    private void InitAvatarBack(Equipment fashionback, Equipment back, Gender gender)
    {
        if (fashionback != null)
            OnBackChanged(GetPrefabPathByGender(fashionback.EquipmentJson, gender));
        else if (back != null)
            OnBackChanged(GetPrefabPathByGender(back.EquipmentJson, gender));
        else
            Unequip("back");
    }

    private void InitAvatarHelm(Equipment fashionhelm, Equipment helm, int hairstyle, int haircolor, bool hidehelm, Gender gender)
    {
        if (hidehelm)
            EquipDefaultHair(hairstyle, haircolor);
        else if (fashionhelm != null)
            EquipHelm(fashionhelm.EquipmentJson, hairstyle, haircolor, gender);
        else if (helm != null)
            EquipHelm(helm.EquipmentJson, hairstyle, haircolor, gender);
        else
            EquipDefaultHair(hairstyle, haircolor);
    }

    private void InitAvatarBody(Equipment fashionbody, Equipment body, int skincolor, Gender gender, JobType jobtype)
    {
        if (fashionbody != null)
            OnSkinChanged("body", GetMeshPathByGender(fashionbody.EquipmentJson, gender), GetMaterialPathByGender(fashionbody.EquipmentJson, gender), GetColorCodeByPart("body", skincolor));
        else
            EquipJobBody(jobtype, skincolor, gender);
    }

    private void InitAvatarFace(int makeup, int skincolor, Gender gender)
    {
        EquipMakeUp(makeup, skincolor, gender);
    }

    private void EquipJobBody(JobType jobtype, int skincolor, Gender gender)
    {
        JobsectJson jobsectJson = JobSectRepo.GetJobByType(jobtype);
        string colorcode = GetColorCodeByPart("body", skincolor);
        if (gender == Gender.Male)
            OnSkinChanged("body", jobsectJson.malemeshpath, jobsectJson.malematerialpath, colorcode);
        else
            OnSkinChanged("body", jobsectJson.femalemeshpath, jobsectJson.femalematerialpath, colorcode);
    }

    private void EquipDefaultHair(int hairStyle, int hairColor)
    {
        string hair_meshpath = CharacterCreationRepo.GetMeshPathByPartId(hairStyle, ApperanceType.HairStyle);
        string hair_materialpath = CharacterCreationRepo.GetMaterialPathByPartId(hairStyle, ApperanceType.HairStyle);
        Unequip("accessory");
        OnSkinChanged("helm", hair_meshpath, hair_materialpath, GetColorCodeByPart("helm", hairColor));
    }

    private void EquipHelm(EquipmentJson equipmentJson, int hairStyle, int hairColor, Gender gender)
    {
        string helm_prefab = (gender == Gender.Male) ? equipmentJson.prefabpath : equipmentJson.femaleprefabpath;
        if (string.IsNullOrEmpty(helm_prefab))
        {
            Unequip("accessory");
            string meshpath = (gender == Gender.Male) ? equipmentJson.malemeshpath : equipmentJson.femalemeshpath;
            if (string.IsNullOrEmpty(meshpath))
                EquipDefaultHair(hairStyle, hairColor);
            else
                OnSkinChanged("helm", meshpath, GetMaterialPathByGender(equipmentJson, gender), "");
        }
        else
        {
            string hair_meshpath = CharacterCreationRepo.GetMeshPathByPartId(hairStyle, ApperanceType.HairStyle);
            string hair_materialpath = CharacterCreationRepo.GetMaterialPathByPartId(hairStyle, ApperanceType.HairStyle);
            OnSkinChanged("helm", hair_meshpath, hair_materialpath, GetColorCodeByPart("helm", hairColor));
            OnAccessoryChanged(helm_prefab);
        }
    }

    private void EquipMakeUp(int makeup, int skincolor, Gender gender)
    {
        string makeup_materialpath = CharacterCreationRepo.GetMaterialPathByPartId(makeup, ApperanceType.MakeUp);
        if (string.IsNullOrEmpty(makeup_materialpath))
        {
            OnSkinChanged("face", gender == Gender.Male ? "Models_Characters/Pc_face/Materials/face_000_base_male.mat" : "Models_Characters/Pc_face/Materials/face_000_base_female.mat", GetColorCodeByPart("face", skincolor));
        }
        else
        {
            OnSkinChanged("face", makeup_materialpath, GetColorCodeByPart("face", skincolor));
        }
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

    public void OnSkinChanged(string partName, EquipmentJson equipmentJson, Gender gender, List<int> appearance)
    {
        if (partName == "helm")
        {
            int hairstyle = appearance[(int)AppearanceSlot.HairStyle];
            int haircolor = appearance[(int)AppearanceSlot.HairColor];
            EquipHelm(equipmentJson, hairstyle, haircolor, gender);
        }
        else if (partName == "body")
        {
            int skincolor = appearance[(int)AppearanceSlot.SkinColor];
            OnSkinChanged("body", GetMeshPathByGender(equipmentJson, gender), GetMaterialPathByGender(equipmentJson, gender), GetColorCodeByPart("body", skincolor));
        }
    }

    private void OnSkinChanged(string partName, string materialpath, string colorcode)
    {
        AvatarSkinPart skinPart = GetSkinPart(partName);
        if (skinPart == null)
            return;
        if (materialpath != skinPart.MaterialLoadingPath)
        {
            skinPart.MaterialLoadingPath = materialpath;
            AssetLoader.Instance.LoadAsync<Material>(materialpath, (material) => OnMaterialLoaded(material, materialpath, skinPart, colorcode));
        }
        else
        {
            Color color = new Color();
            if (ColorUtility.TryParseHtmlString(colorcode, out color))
            {
                if (skinPart.SkinMesh.sharedMaterial.HasProperty("_Discoloration"))
                {
                    skinPart.SkinMesh.sharedMaterial.SetColor("_Discoloration", color);
                }
            }
            UpdateWaitingList(partName);
        }
    }

    private void OnSkinChanged(string partName, string meshpath, string materialpath, string colorcode)
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
            AssetLoader.Instance.LoadAsync<Material>(materialpath, (material) => OnMaterialLoaded(material, materialpath, skinPart, colorcode));
        }
        else
        {
            Color color = new Color();
            if (ColorUtility.TryParseHtmlString(colorcode, out color))
            {
                if (skinPart.SkinMesh.sharedMaterial.HasProperty("_Discoloration"))
                {
                    skinPart.SkinMesh.sharedMaterial.SetColor("_Discoloration", color);
                }
            }
            UpdateWaitingList(partName);
        }
    }

    private string GetPrefabPathByGender(EquipmentJson equipmentJson, Gender gender)
    {
        return (gender == Gender.Male) ? equipmentJson.prefabpath : equipmentJson.femaleprefabpath;
    }

    private string GetMeshPathByGender(EquipmentJson equipmentJson, Gender gender)
    {
        return (gender == Gender.Male) ? equipmentJson.malemeshpath : equipmentJson.femalemeshpath;
    }

    private string GetMaterialPathByGender(EquipmentJson equipmentJson, Gender gender)
    {
        return (gender == Gender.Male) ? equipmentJson.malematerialpath : equipmentJson.femalematerialpath;
    }

    private string GetColorCodeByPart(string partName, int color)
    {
        switch(partName)
        {
            case "body":
                return CharacterCreationRepo.GetColorCodeByPartId(color, ApperanceType.SkinColor);
            case "helm":
                return CharacterCreationRepo.GetColorCodeByPartId(color, ApperanceType.HairColor);
            case "face":
                return CharacterCreationRepo.GetColorCodeByPartId(color, ApperanceType.SkinColor);
            default:
                return "";
        }
    }

    private void Unequip(string partName)
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

    private void OnMaterialLoaded(Material material, string path, AvatarSkinPart skinPart, string colorcode)
    {
        if (isDestroyed || skinPart.MaterialLoadingPath != path)
            return;
        skinPart.SkinMesh.sharedMaterial = material;
        if (material == null)
        {
            Debug.LogErrorFormat("OnMaterialLoaded material path {0} not found!!!", path);
            return;
        }
        else if (!string.IsNullOrEmpty(colorcode))
        {
            Color color = new Color();
            if (ColorUtility.TryParseHtmlString(colorcode, out color))
            {
                if (skinPart.SkinMesh.sharedMaterial.HasProperty("_Discoloration"))
                    skinPart.SkinMesh.sharedMaterial.SetColor("_Discoloration", color);
            }
        }
        UpdateWaitingList(skinPart.PartName);
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

    private void DetachFromBodyPart(string partname)
    {
        AvatarAttachedPart attachedPart = Array.Find(AttachedParts, o => o.PartName == partname);
        if (attachedPart != null)
            DetachFromBodyPart(attachedPart);
    }

    private void DetachFromBodyPart(AvatarAttachedPart attachedPart)
    {
        attachedPart.Detach();
    }

    private AvatarAttachedPart GetAttachedPart(string partname)
    {
        return Array.Find(AttachedParts, o => o.PartName == partname);
    }

    private AvatarSkinPart GetSkinPart(string partname)
    {
        return Array.Find(SkinParts, o => o.PartName == partname);
    }

    private void UpdateWaitingList(string partname)
    {
        if (mWaitingList.Count > 0)
        {
            switch (partname)
            {
                case "body":
                    if (mWaitingList.ContainsKey(0))
                    {
                        mWaitingList[0] = true;
                    }
                    break;
                case "helm":
                    if (mWaitingList.ContainsKey(1))
                    {
                        mWaitingList[1] = true;
                    }
                    break;
                case "face":
                    if (mWaitingList.ContainsKey(2))
                    {
                        mWaitingList[2] = true;
                    }
                    break;
                default:
                    break;
            }
            CheckWaitingList();
        }
    }

    private void CheckWaitingList()
    {
        foreach(KeyValuePair<int, bool> entry in mWaitingList)
        {
            if (entry.Value == false)
                return;
        }

        if (mCallback != null)
        {
            mCallback();
            mCallback = null;
            mWaitingList = new Dictionary<int, bool>();
        }
    }

    void OnDestroy()
    {
        isDestroyed = true;
    }

    public void TransformWeapon(bool value)
    {
        AvatarAttachedPart attachedPart = GetAttachedPart("weapon_r");
        if (attachedPart != null)
        {
            Animator animator = attachedPart.AttachedObj.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool("On/Off", value);
            }
        }
    }
}