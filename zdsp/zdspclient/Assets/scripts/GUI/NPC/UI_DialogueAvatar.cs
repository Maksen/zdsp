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
    Transform AvatarContent = null;
    [SerializeField]
    Model_3DAvatar PlayerAvatar = null;
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

        PlayerAvatar.Change(player.mEquipmentInvData, player.GetJobSect(), player.mGender, null);
        mPlayer = PlayerAvatar.GetOutfitModel();
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
                if (npcJson != null)
                {
                    NameObj.SetActive(!string.IsNullOrEmpty(npcJson.localizedname));
                    Name.text = npcJson.localizedname;
                }
                SetAvatar(mActivatedNpc);
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

            StaticNPCJson npcJson = StaticNPCRepo.GetNPCById(npcId);
            if (npcJson != null)
            {
                NameObj.SetActive(!string.IsNullOrEmpty(npcJson.localizedname));
                Name.text = npcJson.localizedname;
            }
        }

        if (mAvatar != null)
        {
            SetAvatar(npcId);
        }

        Message.text = CheckReplacementText(message);
    }

    private void SetAvatar(int npcId)
    {
        mAvatar.SetActive(true);
        ClientUtils.SetLayerRecursively(mAvatar, AvatarContent.gameObject.layer);
        mAvatar.transform.SetParent(AvatarContent, false);
        Animator animator = mAvatar.GetComponent<Animator>();
        if (animator != null)
        {
            if (npcId != 0)
                animator.PlayFromStart("standby");
            else
            {
                Equipment weapon = GameInfo.gLocalPlayer.mEquipmentInvData.GetEquipmentBySlotId((int)EquipmentSlot.Weapon);
                PartsType weaponType = (weapon != null) ? weapon.EquipmentJson.partstype : PartsType.Blade;
                animator.PlayFromStart(ClientUtils.GetStandbyAnimationByWeaponType(weaponType));
            }
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
            PlayerAvatar.Cleanup();

        if (mNpcList != null)
        {
            foreach (KeyValuePair<int, GameObject> npc in mNpcList)
                Destroy(npc.Value);
        }

        mAvatar = null;
    }
}
