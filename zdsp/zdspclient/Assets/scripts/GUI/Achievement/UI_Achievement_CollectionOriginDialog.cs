using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Repository;

public class UI_Achievement_CollectionOriginDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataParent;
    [SerializeField] GameObject dataPrefab;

    public void Init(string originStr)
    {
        int originId;
        ItemOriginJson itemOriginJson;
        string[] origins = originStr.Split(';');
        for (int i = 0; i < origins.Length; ++i)
        {
            if (int.TryParse(origins[i], out originId) && GameRepo.ItemFactory.ItemOriginTable.TryGetValue(originId, out itemOriginJson))
                InitOriginData(itemOriginJson);
        }
    }

    private void InitOriginData(ItemOriginJson json)
    {
        string[] paramArr = json.param.Split(';');
        for (int i = 0; i < paramArr.Length; i++)
        {
            GameObject go = ClientUtils.CreateChild(dataParent, dataPrefab);
            go.GetComponent<Achievement_ItemOriginData>().Init(json.origintype, paramArr[i]);
        }
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClientUtils.DestroyChildren(dataParent);
    }
}
