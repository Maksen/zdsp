#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Zealot.Common;

/// <summary>
/// Code in here is more like a hack to quicken the testing on Editor and it is not suppose to be in the game
/// </summary>
namespace TestContent
{
    public class TestContent : MonoSingleton<TestContent>
    {
        [MenuItem("Debug/CreateTestContent %~")]
        public static void CreateTestContent()
        {
            TestContent.Instance.Init();
        }

        void Init()
        {
            gameObject.AddComponent<TestLogin>();
            gameObject.AddComponent<TestOpenWindow>();
        }
    }

    public class TestLogin : MonoBehaviour
    {
        [SerializeField]
        string AccountID = "";
        public void ChangeDeviceID()
        {
            LoginData.Instance.LoginType = (short)LoginAuthType.Device;
            LoginData.Instance.DeviceId = AccountID;
        }

        public void ExitToLogin()
        {
            PhotonNetwork.networkingPeer.Disconnect();
        }
    }

    [CustomEditor(typeof(TestLogin))]
    public class TestLoginEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TestLogin myScript = (TestLogin)target;
            if (GUILayout.Button("Change Log in"))
            {
                myScript.ChangeDeviceID();
            }
            if (GUILayout.Button("Exit to Login"))
            {
                myScript.ExitToLogin();
            }

        }
    }

    public class TestOpenWindow : MonoBehaviour
    {
        [SerializeField]
        WindowType windowtype;

        public void Openwindow()
        {
            if (windowtype < WindowType.WindowEnd)
                UIManager.OpenWindow(windowtype);
            else if (windowtype < WindowType.DialogEnd)
                UIManager.OpenDialog(windowtype);
        }
    }

    [CustomEditor(typeof(TestOpenWindow))]
    public class TestOpenWindowEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TestOpenWindow myScript = (TestOpenWindow)target;
            if (GUILayout.Button("Test Open Window"))
            {
                myScript.Openwindow();
            }
        }
    }
}
#endif
