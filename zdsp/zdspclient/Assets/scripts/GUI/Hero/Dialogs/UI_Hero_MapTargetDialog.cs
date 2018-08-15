using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Hero_MapTargetDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    private Action<int> OnTargetSelectedCallback;

    public void Init(ExplorationMapJson mapData, Action<int> selectCallback)
    {
        OnTargetSelectedCallback = selectCallback;

        // create data for all target
        GameObject obj = ClientUtils.CreateChild(dataParent, dataPrefab);
        obj.GetComponent<Hero_MapTargetData>().Init(null, 0, OnTargetSelected);

        List<ExplorationTargetJson> list = HeroRepo.GetExplorationTargetsByGroup(mapData.exploregroupid);
        for (int i = 0; i < list.Count; i++)
        {
            obj = ClientUtils.CreateChild(dataParent, dataPrefab);
            obj.GetComponent<Hero_MapTargetData>().Init(list[i], mapData.reqmonsterlevel, OnTargetSelected);
        }
    }

    private void OnTargetSelected(int targetId)
    {
        if (OnTargetSelectedCallback != null)
            OnTargetSelectedCallback(targetId);
        GetComponent<UIDialog>().ClickClose();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}