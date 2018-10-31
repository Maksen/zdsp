using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
# endif

namespace ViewPlayer
{
    enum ViewPlayerLoadModelType
    {
        begin,
        player,
        monster,
        boss,
        npc,
        end
    }
    public class ViewPlayerLoadModelControl : MonoBehaviour
    {

        public GameObject inputAssetPath;
        public GameObject inputFileName;
        public GameObject animateControl;
        public GameObject playerEquipementControl;
        public GameObject inputControl;
        public GameObject mainObject;
        public GameObject dragArea;
        public GameObject MainPosObject;
        public GameObject TargetPosObject;

        private int id = 0;
        private string mAnimaName = "";
        private ViewPlayerLoadModelType loadType;
        private Vector3 spawnLocation = new Vector3(0.0f, -0.35f, -7.5f);
        private Vector3 startRotate = new Vector3(0f, 180f, 0f);
        private string modelTag = "";

        void Start()
        {

        }

        void Update()
        {

        }

        void EnableSubMenus()
        {
            //dragArea.SetActive(true);
            inputControl.SetActive(true);
            animateControl.SetActive(true);
            if (loadType == ViewPlayerLoadModelType.player)
                playerEquipementControl.SetActive(true);
        }

        void DesableSubMenus()
        {
            //dragArea.SetActive(false);
            inputControl.SetActive(false);
            animateControl.SetActive(false);
            playerEquipementControl.SetActive(false);
        }

        void SetLoadModelType(ViewPlayerLoadModelType type)
        {
            if (type > ViewPlayerLoadModelType.begin && type < ViewPlayerLoadModelType.end)
                loadType = type;

            switch (loadType)
            {
                case ViewPlayerLoadModelType.player:
                    SetAssetPathInputText("Models_Characters_prefabs");
                    SetFileNameInputText("Prefab_scy.prefab");
                    modelTag = "LocalPlayer";
                    break;
                case ViewPlayerLoadModelType.monster:
                    SetAssetPathInputText("Models_npc_prefabs_mon");
                    SetFileNameInputText("Prefab_mon_001.prefab");
                    modelTag = "Monster";
                    break;
                case ViewPlayerLoadModelType.boss:
                    SetAssetPathInputText("Models_npc_prefabs_boss");
                    SetFileNameInputText("Prefab_boss_001.prefab");
                    modelTag = "Monster";
                    break;
                case ViewPlayerLoadModelType.npc:
                    SetAssetPathInputText("Models_npc_prefabs_npc");
                    SetFileNameInputText("Prefab_npc_001.prefab");
                    modelTag = "NPC";
                    break;

            }
        }

        public void OnLoadModelButton()
        {
            string assetPath = GetAssetPathInputText();
            string fileName = GetFileNameInputText();
            if (fileName.Length == 0)
            {
                mainObject.SendMessage("ShowDialog", "Prefabs is empty!", SendMessageOptions.DontRequireReceiver);
                return;
            }

            string loadPath = assetPath + "/" + fileName;
            GameObject charmodel = AssetManager.LoadAsset<GameObject>(loadPath) as GameObject;
            OnLoadedModel(charmodel);
        }

        public void OnLoadFileButton()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("select .prefeb file of model", "Assets/", "prefab");
            if (path.Length > 0)
            {
                int subLen = Application.dataPath.Length - 6;
                string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");
                GameObject charmodel = AssetDatabase.LoadAssetAtPath<GameObject>(assetRelativePath) as GameObject;
                OnLoadedModel(charmodel);
            }
#endif
        }

        void SetAssetPathInputText(string text)
        {
            inputAssetPath.GetComponent<InputField>().text = text;
        }

        string GetAssetPathInputText()
        {
            return inputAssetPath.GetComponent<InputField>().text;
        }

        void SetFileNameInputText(string text)
        {
            inputFileName.GetComponent<InputField>().text = text;
        }

        string GetFileNameInputText()
        {
            return inputFileName.GetComponent<InputField>().text;
        }

        void OnLoadedModel(GameObject charmodel)
        {
            DesableSubMenus();


            if (charmodel != null)
            {
                var isMainModel = ViewPlayerModelControl.Instance.IsSelectMain;

                var pos = isMainModel? ((MainPosObject != null)? MainPosObject.transform.position : spawnLocation) : ((TargetPosObject != null) ? TargetPosObject.transform.position : spawnLocation);
                GameObject loadedCharModel = Instantiate(charmodel, pos, Quaternion.identity) as GameObject;
                loadedCharModel.transform.Rotate(startRotate);

                EffectController effectController = loadedCharModel.AddComponent<EffectController>();
                Animator anim = loadedCharModel.GetComponent<Animator>();
                anim.tag = modelTag;
                effectController.Animator = anim;
                effectController.ShowAnim(true);
                effectController.ShowEfx(true);
                 
                if (isMainModel)
                    loadedCharModel.name = "MainModel";
                else
                    loadedCharModel.name = "TargetModel";
                //AvatarAdaptorScript avataradaptor = loadedCharModel.GetComponent<AvatarAdaptorScript>();
                //if (avataradaptor)
                //{
                //    avataradaptor.InitAttachList();
                //    avataradaptor.gameObject.layer = 0;
                //}
                ViewPlayerModelControl.Instance.SetModel(loadedCharModel);

                EnableSubMenus();

                animateControl.SendMessage("OnModelLoaded", null);
                inputControl.SendMessage("OnModelLoaded", null);
            }
            else
            {
                mainObject.SendMessage("ShowDialog", "Prefabs load failed!", SendMessageOptions.DontRequireReceiver);
            }
        }

        void OnDestroy()
        {
            inputAssetPath = null;
            inputFileName = null;
            animateControl = null;
            playerEquipementControl = null;
            inputControl = null;
            mainObject = null;
            dragArea = null;
        }
    }
}