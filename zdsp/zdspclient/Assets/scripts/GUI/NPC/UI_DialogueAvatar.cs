using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Client.Entities;
using Zealot.Common;

public class UI_DialogueAvatar : MonoBehaviour
{
    [SerializeField]
    Transform AvatarContent;

    [SerializeField]
    Model_3DAvatar PlayerAvatar;

    [SerializeField]
    Text Name;

    [SerializeField]
    Text Message;

    private GameObject mAvatar;
    private Dictionary<int, GameObject> mNpcList;
    private GameObject mPlayer;

    public void SpawnNpc(List<int> npclist)
    {
        mNpcList = new Dictionary<int, GameObject>();
        foreach (int npcid in npclist)
        {
            StaticNPCJson npcJson = StaticNPCRepo.GetStaticNPCById(npcid);
            if (npcJson != null)
            {
                OnNpcLoaded(npcid, AssetManager.LoadSceneNPC(npcJson.modelprefabpath), npcJson);
            }
        }
    }

    public void SpawnPlayer()
    {
        if(GameInfo.gLocalPlayer == null)
        {
            return;
        }

        PlayerGhost player = GameInfo.gLocalPlayer;
        PlayerAvatar.Change(player.mEquipmentInvData, (JobType)player.PlayerSynStats.jobsect, player.mGender, null);
        mPlayer = PlayerAvatar.GetOutfitModel();
        mPlayer.SetActive(false);
    }

    private void OnPlayerLoaded(GameObject asset)
    {
        if (asset != null)
        {
            mPlayer = Instantiate(asset);
        }
    }

    private void OnNpcLoaded(int npcid, GameObject asset, StaticNPCJson npcJson)
    {
        if (asset == null)
        {
            AssetLoader.Instance.LoadAsync<GameObject>(npcJson.containerprefabpath, (obj) => { OnNpcAsyncLoaded(npcid, obj); });
            return;
        }
        else
        {
            GameObject go = Instantiate(asset);
            mNpcList.Add(npcid, go);
        }
    }

    private void OnNpcAsyncLoaded(int npcid, GameObject asset)
    {
        if (asset !=null)
        {
            GameObject go = Instantiate(asset);
            mNpcList.Add(npcid, go);
        }
    }

    public void ActiveAvatar(int npcid, string message)
    {
        if (mAvatar != null)
        {
            ClientUtils.SetLayerRecursively(mAvatar, 0);
            mAvatar.SetActive(false);
        }

        if (npcid == 0)
        {
            mAvatar = mPlayer;
            Name.text = GameInfo.gLocalPlayer.PlayerSynStats.name;
        }
        else
        {
            if (mNpcList.ContainsKey(npcid))
            {
                mAvatar = mNpcList[npcid];
            }
            StaticNPCJson npcJson = StaticNPCRepo.GetStaticNPCById(npcid);
            if (npcJson != null)
            {
                Name.text = npcJson.localizedname;
            }
        }

        if (mAvatar != null)
        {
            mAvatar.SetActive(true);
            ClientUtils.SetLayerRecursively(mAvatar, AvatarContent.gameObject.layer);
            mAvatar.transform.SetParent(AvatarContent, false);
        }

        Message.text = CheckReplacementText(message);
    }

    private string CheckReplacementText(string message)
    {
        if (GameInfo.gLocalPlayer!=null)
        {
            message = message.Replace("%pc%", GameInfo.gLocalPlayer.PlayerSynStats.name);
            message = message.Replace("%job%", JobSectRepo.GetJobLocalizedName(GameInfo.gLocalPlayer.GetJobSect()));
        }
        return message;
    }

    public void DestroyModel()
    {
        if (mPlayer != null)
        {
            PlayerAvatar.Cleanup();
        }

        if (mNpcList != null)
        {
            foreach (KeyValuePair<int, GameObject> npc in mNpcList)
            {
                Destroy(npc.Value);
            }
        }

        mAvatar = null;
    }
}
