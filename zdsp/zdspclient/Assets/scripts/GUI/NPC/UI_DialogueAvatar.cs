using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class UI_DialogueAvatar : MonoBehaviour
{
    [SerializeField]
    Transform AvatarContent;

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
                OnNpcLoaded(npcid, AssetManager.LoadSceneNPC(npcJson.modelprefabpath));
            }
        }
    }

    public void SpawnPlayer()
    {
        if(GameInfo.gLocalPlayer == null)
        {
            return;
        }
        
        OnPlayerLoaded(ClientUtils.InstantiatePlayer(GameInfo.gLocalPlayer.mGender));
    }

    private void OnPlayerLoaded(GameObject asset)
    {
        if (asset != null)
        {
            mPlayer = Instantiate(asset);
            RemoveComponent(mPlayer);
        }
    }

    private void OnNpcLoaded(int npcid, GameObject asset)
    {
        if (asset != null)
        {
            GameObject go = Instantiate(asset);
            RemoveComponent(go);
            mNpcList.Add(npcid, go);
        }
    }

    private void RemoveComponent(GameObject go)
    {
        foreach(Component component in go.GetComponents<Component>())
        {
            if (!(component is Transform) && !(component is Animator))
            {
                Destroy(component);
            }
        }
        go.transform.position = Vector3.zero;
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
                StaticNPCJson npcJson = StaticNPCRepo.GetStaticNPCById(npcid);
                Name.text = npcJson.localizedname;
            }
        }

        if (mAvatar != null)
        {
            mAvatar.SetActive(true);
            ClientUtils.SetLayerRecursively(mAvatar, AvatarContent.gameObject.layer);
            mAvatar.transform.SetParent(AvatarContent, false);
        }
        Message.text = message;
    }

    public void DestroyModel()
    {
        if (mPlayer != null)
        {
            Destroy(mPlayer);
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
