using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class LootDisplayController : MonoBehaviour
{
    private List<GameObject> mMyLoot = new List<GameObject>();
    private List<GameObject> mNotMyLoot = new List<GameObject>();
    //private List<GameObject> mFlyingEffects = new List<GameObject>();
    private float mFlyTimeLeft = 0;
    private PlayerGhost mLocalPlayer = null;

    public void Init(LootItemDisplayInventory lootData)
    {
        enabled = false;
        int lootCount = lootData.records.Count;
        float radius = Math.Min(6, 2 + ((lootCount - 1) / 4) * 0.5f);
        Vector3 localOrigin = new Vector3(0, 0.5f, 0);
        mLocalPlayer = GameInfo.gLocalPlayer;
        int mypid = mLocalPlayer.GetPersistentID();
        for (int index = 0; index < lootCount; index++)
            CreateLoot(lootData.records[index], GameUtils.RandomPosWithRadiusRange(localOrigin, 1, radius), mypid);
        Invoke("FlyToOwner", 1.5f);
    }

    private void CreateLoot(LootItemDisplay loot, Vector3 offset, int mypid)
    {
        int itemid = loot.itemid;
        string prefabName = "";
        if (itemid < 0)
            prefabName = GameConstantRepo.GetConstant("Loot_3DModel_Money");
        else
        {
            var itemJson = GameRepo.ItemFactory.GetItemById(itemid);
            if (itemJson != null)
            {
                if (itemJson.itemtype == ItemType.Equipment)
                    prefabName = GameConstantRepo.GetConstant("Loot_3DModel_Chest");
                else
                    prefabName = GameConstantRepo.GetConstant("Loot_3DModel_Bag");
            }
        }
        if (string.IsNullOrEmpty(prefabName))
            return;

        GameObject prefab = AssetLoader.Instance.Load<GameObject>(AssetLoader.GetLoadString("Scenes_Zdsp_itemobj_FBX_FOR_VIEW_NOTFORBUILD", prefabName));
        if (prefab == null)
            return;
        GameObject lootObject = Instantiate(prefab);
        lootObject.transform.SetParent(transform, false);
        lootObject.transform.localPosition = offset;
        if (loot.pid == mypid)
            mMyLoot.Add(lootObject);
        else
            mNotMyLoot.Add(lootObject);
    }

    private void FlyToOwner()
    {
        int myLootCount = mMyLoot.Count;
        if (myLootCount > 0)
        {
            mFlyTimeLeft = 2f;
            int notMyLootCount = mNotMyLoot.Count;
            for (int index = 0; index < notMyLootCount; index++)
                mNotMyLoot[index].SetActive(false);
            enabled = true;
        }
        else
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mFlyTimeLeft <= 0)
            return;
        float dt = Time.deltaTime;
        Vector3 _targetPos = mLocalPlayer.Position;
        if (mFlyTimeLeft <= dt)
        {
            Destroy(gameObject);
        }
        else
        {
            int myLootCount = mMyLoot.Count;
            for (int index = myLootCount - 1; index >= 0; index--)
            {
                Vector3 mDir = _targetPos - mMyLoot[index].transform.position;
                if (mDir.sqrMagnitude <= 0.01)
                {
                    mMyLoot[index].SetActive(false);
                    mMyLoot.RemoveAt(index);
                }
                else
                    mMyLoot[index].transform.position += mDir * dt / mFlyTimeLeft;             
            }
        }
        mFlyTimeLeft -= dt;
    }
}
