using UnityEngine;
using Zealot.Repository;
using UnityEngine.UI;
using Kopio.JsonContracts;

public class TeleportNPCUIController : MonoBehaviour
{
    public GameObject Text_NPC_Title;
    public GameObject Text_NPC_Message;
    public GameObject ModelParent_NPC_TranslateMe;

    private string Archetype;
    private TeleportNPCListJson TeleportNPCInfo;
    private GameObject PrefabModel;

    void OnEnable()
    {
        if (Text_NPC_Title == null || Text_NPC_Message == null || ModelParent_NPC_TranslateMe == null)
        {
            Debug.LogError("!!!NPC Tlak UI Not Link!!!");
            return;
        }
        if (TeleportNPCData.Archetype == null)
        {
            Debug.LogError("!!!NPC Talk Archetype Not Set!!!");
            return;
        }
        Archetype = TeleportNPCData.Archetype;
        if (TeleportNPCListRepo.GetInfoByArchetype(Archetype) == null)
        {
            Debug.LogError("!!!NPC Talk Archetype Not Find!!!");
            return;
        }
        SetNPCUI();
    }

    void OnDisable()
    {
        if (PrefabModel != null)
            Destroy(PrefabModel.gameObject);
        PrefabModel = null;
    }

    public void SetNPCUI()
    {
        //Set NPC Message
        TeleportNPCInfo = TeleportNPCListRepo.GetInfoByArchetype(Archetype);
        Text_NPC_Title.GetComponent<Text>().text = TeleportNPCInfo.title;
        Text_NPC_Message.GetComponent<Text>().text = TeleportNPCInfo.message;

        //Set NPC Prefab
        string path = TeleportNPCListRepo.GetNPCPrefabPath(Archetype);
        AssetLoader.Instance.LoadAsync<GameObject>(path, InitNPCPrefab);
    }

    public void InitNPCPrefab(GameObject prefab)
    {
        PrefabModel = GameObject.Instantiate(prefab);
        PrefabModel.transform.SetParent(ModelParent_NPC_TranslateMe.transform, false);
        PrefabModel.transform.localPosition = Vector3.zero;
        PrefabModel.transform.localRotation = Quaternion.identity;
        float[] NPCScale = ParseTransformInTalk(TeleportNPCInfo.npcscale);
        PrefabModel.transform.localScale = new Vector3(NPCScale[0], NPCScale[1], NPCScale[2]);
        ClientUtils.SetLayerRecursively(PrefabModel.gameObject, LayerMask.NameToLayer("UI"));
    }

    public float[] ParseTransformInTalk(string transform)
    {
        float[] Float = new float[3];
        string[] String = transform.Split(';');
        for (int i = 0; i < Float.Length; i++)
        {
            Float[i] = float.Parse(String[i]);
        }
        return Float;
    }

    public void Enter()
    {
        RPCFactory.CombatRPC.LeaveRealm();
    }
}
