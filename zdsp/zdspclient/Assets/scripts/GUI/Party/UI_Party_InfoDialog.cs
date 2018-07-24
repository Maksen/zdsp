using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class UI_Party_InfoDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataContentTransform;
    [SerializeField] GameObject dataPrefab;

    private int partyId;

    public void Init(int id, List<PartyMemberInfo> members)
    {
        partyId = id;
        for (int i = 0; i < members.Count; i++)
        {
            PartyMemberInfo info = members[i];
            GameObject obj = ClientUtils.CreateChild(dataContentTransform, dataPrefab);
            obj.GetComponent<UI_Party_MemberData>().Init(info.name, info.level, info.portraitId, info.isLeader);
        }
    }

    public override void OnCloseWindow()
    {
        ClientUtils.DestroyChildren(dataContentTransform);
    }

    public void OnClickApply()
    {
        RPCFactory.CombatRPC.JoinParty(partyId);
        GetComponent<UIDialog>().ClickClose();
    }

}
