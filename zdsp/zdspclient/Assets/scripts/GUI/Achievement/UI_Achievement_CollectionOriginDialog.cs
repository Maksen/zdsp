using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_Achievement_CollectionOriginDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;

    public void Init(string originStr)
    {
        if (originStr == "-1")
            return;
        string[] origins = originStr.Split(';');
        for (int i = 0; i < origins.Length; ++i)
        {
            int originId;
            if (int.TryParse(origins[i], out originId))
            {
                ItemOriginJson itemOriginJson = GameRepo.ItemFactory.GetItemOriginById(originId);
                if (itemOriginJson != null)
                    InitOriginData(itemOriginJson);
            }
        }
    }

    private void InitOriginData(ItemOriginJson json)
    {
        string[] paramArr = json.param.Split(';');
        for (int i = 0; i < paramArr.Length; i++)
        {
            if (json.origintype == Zealot.Common.ItemOriginType.Auction)  // todo: jm to confirm when have auction
                continue;
            GameObject go = ClientUtils.CreateChild(dataParent, dataPrefab);
            go.GetComponent<Achievement_ItemOriginData>().Init(json.origintype, paramArr[i]);
        }
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}