using UnityEngine;

namespace ViewPlayer
{
    public class ViewPlayer : MonoBehaviour
    {

        public GameObject mainMenu;
        public GameObject loadModelMeun;
        public GameObject tipPanel;
        public GameObject dialogPanel;
        public GameObject dragArea;

        private GameObject currentMenu;

        void Start()
        {
            currentMenu = mainMenu;

            //EfxSystem.Instance.Init();
            //yield return StartCoroutine( InitGameDB() );
            //StartCoroutine(InitGameDB());
        }

        /*IEnumerator InitGameDB()
        {
            Debug.Log("InitGameDB start");
            //ShowTip("GameDB loading...");
            yield return AssetLoader.Instance.LoadAsyncCoroutine<TextAsset>("GameData_GameRepo", "gamedata.json",
                (TextAsset gameDB) =>
                {
                    Debug.Log("InitGameDB done");
                    if (!string.IsNullOrEmpty(gameDB.text))
                    {
                        Debug.Log("InitGameDB success");
                        CloseTip();
                        //GameRepo.SetUnCommonRepo(new ClientGameRepository());
                        GameRepo.SetItemFactory(new ClientItemFactory());
                        GameRepo.InitClient(gameDB.text);

                        EfxSystem.Instance.InitFromGameDB();
                    }
                    else {
                        Debug.Log("InitGameDB error");
                        ShowTip("load GameDB error"); 
                    }
                });
        }*/

        void Update()
        {

        }

        public void ShowTip(string msg)
        {
            tipPanel.SetActive(true);
            tipPanel.SendMessage("ShowMessage", msg, SendMessageOptions.DontRequireReceiver);
        }

        public void CloseTip()
        {
            tipPanel.SetActive(false);
        }

        public void ShowDialog(string msg)
        {
            dialogPanel.SetActive(true);
            dialogPanel.SendMessage("ShowMessage", msg, SendMessageOptions.DontRequireReceiver);
        }

        public void CloseDialog()
        {
            dialogPanel.SetActive(false);
        }

        void OnLoadGameDBEnd(string error_message = "")
        {
            if (error_message.Length == 0)
            {
                CloseTip();
                EfxSystem.Instance.InitFromGameDB();
            }
            else
            {
                ShowTip(error_message);
                EfxSystem.Instance.InitFromGameDB();
            }
        }

        public void OnBackMainMeun()
        {
            currentMenu.SetActive(false);
            mainMenu.SetActive(true);
            currentMenu = mainMenu;
            Debug.Log("OnBackMainMeun");
        }

        public void OnPlayer()
        {
            mainMenu.SetActive(false);
            loadModelMeun.SetActive(true);
            currentMenu = loadModelMeun;
            loadModelMeun.SendMessage("SetLoadModelType", ViewPlayerLoadModelType.player, SendMessageOptions.DontRequireReceiver);
            //Debug.Log("OnPlayer");
        }

        public void OnMonster()
        {
            mainMenu.SetActive(false);
            loadModelMeun.SetActive(true);
            currentMenu = loadModelMeun;
            loadModelMeun.SendMessage("SetLoadModelType", ViewPlayerLoadModelType.monster, SendMessageOptions.DontRequireReceiver);
            //Debug.Log("OnMonster");
        }

        public void OnBoss()
        {
            mainMenu.SetActive(false);
            loadModelMeun.SetActive(true);
            currentMenu = loadModelMeun;
            loadModelMeun.SendMessage("SetLoadModelType", ViewPlayerLoadModelType.boss, SendMessageOptions.DontRequireReceiver);
            //Debug.Log("OnBoss");
        }

        public void OnNpc()
        {
            mainMenu.SetActive(false);
            loadModelMeun.SetActive(true);
            currentMenu = loadModelMeun;
            loadModelMeun.SendMessage("SetLoadModelType", ViewPlayerLoadModelType.npc, SendMessageOptions.DontRequireReceiver);
            //Debug.Log("OnNpc");
        }

        void DestoryGameObj()
        {

        }
        void OnDestroy()
        {
            DestoryGameObj();

            mainMenu = null;
            loadModelMeun = null;
            tipPanel = null;
            dialogPanel = null;
            dragArea = null;
            currentMenu = null;
        }
    }
}