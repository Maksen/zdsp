using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_DialogueAvatar : MonoBehaviour
{
    [SerializeField]
    Model_3DAvatar playerAvatar = null;
    [SerializeField]
    Transform npcAvatarParent = null;
    [SerializeField]
    Text Name = null;
    [SerializeField]
    GameObject NameObj = null;
    [SerializeField]
    Text Message = null;

    private GameObject mAvatar;
    private Dictionary<int, GameObject> mNpcList;
    private GameObject mPlayer = null;
    private int mActivatedNpc;

    public void SpawnNPC(List<int> npcList)
    {
        mNpcList = new Dictionary<int, GameObject>();
        foreach (int npcId in npcList)
        {
            StaticNPCJson npcJson = StaticNPCRepo.GetNPCById(npcId);
            if (npcJson != null)
                OnLoadNPC(npcId, npcJson, AssetManager.LoadSceneNPC(npcJson.modelprefabpath));
        }
    }

    public void SpawnPlayer()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
            return;

        playerAvatar.Change(player.mEquipmentInvData, player.GetJobSect(), player.mGender, null);
        mPlayer = playerAvatar.OutfitModel;
        mPlayer.SetActive(false);
    }

    private void OnLoadNPC(int npcId, StaticNPCJson npcJson, GameObject asset)
    {
        if (asset == null)
        {
            AssetLoader.Instance.LoadAsync<GameObject>(npcJson.containerprefabpath, 
                (obj) => OnNpcAsyncLoaded(npcId, npcJson, obj));
        }
        else
        {
            GameObject go = Instantiate(asset);
            CharacterController charCtrl = go.GetComponent<CharacterController>();
            if (charCtrl != null)
                Destroy(charCtrl);
            mNpcList.Add(npcId, go);
        }
    }

    private void OnNpcAsyncLoaded(int npcId, StaticNPCJson npcJson, GameObject asset)
    {
        if (asset != null)
        {
            GameObject go = Instantiate(asset);
            CharacterController charCtrl = go.GetComponent<CharacterController>();
            if (charCtrl != null)
                Destroy(charCtrl);
            mNpcList.Add(npcId, go);

            if (mActivatedNpc == npcId)
            {
                mAvatar = mNpcList[mActivatedNpc];
                NameObj.SetActive(!string.IsNullOrEmpty(npcJson.localizedname));
                Name.text = npcJson.localizedname;
                SetAvatar(npcJson);
            }
        }
    }

    public void ActiveAvatar(int npcId, string message)
    {
        mActivatedNpc = npcId;
        if (mAvatar != null)
        {
            ClientUtils.SetLayerRecursively(mAvatar, 0);
            mAvatar.SetActive(false);
        }

        StaticNPCJson npcJson = null;
        if (npcId == 0)
        {
            mAvatar = mPlayer;
            NameObj.SetActive(true);
            Name.text = GameInfo.gLocalPlayer.Name;
        }
        else
        {
            if (mNpcList.ContainsKey(npcId))
                mAvatar = mNpcList[npcId];

            npcJson = StaticNPCRepo.GetNPCById(npcId);
            NameObj.SetActive(!string.IsNullOrEmpty(npcJson.localizedname));
            Name.text = npcJson.localizedname;
        }

        if (mAvatar != null)
            SetAvatar(npcJson);

        Message.text = CheckReplacementText(message);
    }

    private void SetAvatar(StaticNPCJson npcJson)
    {
        mAvatar.SetActive(true);
        ClientUtils.SetLayerRecursively(mAvatar, playerAvatar.gameObject.layer);

        Animator animator = mAvatar.GetComponent<Animator>();
        if (npcJson != null)
        {
            Transform avatarTransform = mAvatar.transform;
            avatarTransform.SetParent(npcAvatarParent, false);
            float[] camera = StaticNPCRepo.ParseCameraPosInTalk(npcJson.cameraposintalk);
            Vector3 pos = avatarTransform.parent.localPosition;
            avatarTransform.parent.localPosition = new Vector3(camera[0], camera[1], pos.z);
            avatarTransform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
            avatarTransform.localScale = new Vector3(camera[3], camera[3], camera[3]);
            if (animator != null)
                animator.PlayFromStart("standby");
        }
        else if (animator != null) // Player avatar standby
        {
            Equipment weapon = GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Weapon);
            PartsType weaponType = (weapon != null) ? weapon.EquipmentJson.partstype : PartsType.Blade;
            animator.PlayFromStart(ClientUtils.GetStandbyAnimationByWeaponType(weaponType));
        }
    }

    private string CheckReplacementText(string message)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null)
        {
            message = message.Replace("%pc%", player.Name);
            message = message.Replace("%job%", JobSectRepo.GetJobLocalizedName(player.GetJobSect()));
        }
        return message;
    }

    public void DestroyModel()
    {
        if (mPlayer != null)
            playerAvatar.Cleanup();

        if (mNpcList != null)
        {
            foreach (KeyValuePair<int, GameObject> npc in mNpcList)
                Destroy(npc.Value);
        }

        mAvatar = null;
    }
}
