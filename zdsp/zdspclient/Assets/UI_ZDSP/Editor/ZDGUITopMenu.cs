using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
//using System.Collections;

public class ZDGUITopMenu : EditorWindow
{
    // %(Ctrl), #(Shift), &(Alt), _(None)

    [MenuItem("ZDGUI/SaveAsset (Really Save modified changes) &S", false, -99)]
    static void SaveAsset()
    {
        AssetDatabase.SaveAssets();
        Debug.LogWarningFormat("<b>Asset Saved</b>");
    }

    [MenuItem("ZDGUI/Toggle Activate &A", false, -98)]
    static void ToggleActivate()
    {
        if (Selection.activeGameObject.activeSelf)
        {
            Selection.activeGameObject.SetActive(false);
        }
        else
        {
            Selection.activeGameObject.SetActive(true);
        }
    }

    [MenuItem("File/Mark Scene Dirty", false, 9949)]
    static void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    [MenuItem("File/Revert Scene", false, 9999)]
    static void RevertScene()
    {
        string myScene = EditorSceneManager.GetActiveScene().path;
        OpenNewScene(myScene);
    }

    //####################################################################

    [MenuItem("ZDGUI/UITemplate/HUD (Dept 10)", false, 11)]
    static void HUD()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/HUDTemplate.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UITemplate/UIWindows (Dept 20)", false, 12)]
    static void UI()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/UI_WindowTemplate.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UITemplate/UIWindows (TabA) (Dept 20)", false, 13)]
    static void UITabA()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/UI_WindowTemplate_TabA.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UITemplate/Dialog (Dept 50)", false, 14)]
    static void Dialog()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/DialogTemplate.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/UITemplate/Tutorial (Dept 80)", false, 14)]
    static void UITutorial()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/TutorialTemplate.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UITemplate/SystemMessage (Dept 90)", false, 16)]
    static void SystemMessage()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/SystemMessageTemplate.unity";
        OpenNewScene(myScene);
    }

    //####################################################################

    [MenuItem("ZDGUI/### New Scene with just a Canvas and EventSystem", false, 51)]
    static void Canvas_EventSystem()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UITemplates/Canvas_EventSystem.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/SplashScreen", false, 52)]
    static void SplashScreen()
    {
        string myScene = "Assets/Scenes/SplashScreen.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UI_Sample", false, 53)]
    static void UISample()
    {
        string myScene = "Assets/UI_ZDSP/Widgets/UI_Sample/UI_Sample.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/UI_CombatHierarchy", false, 54)]
    static void UI_CombatHierarchy()
    {
        string myScene = "Assets/UI_ZDSP/SCENES/UI_CombatHierarchy/UI_CombatHierarchy.unity";
        OpenNewScene(myScene);
    }

	[MenuItem("ZDGUI/UI_LoginHierarchy", false, 55)]
    static void LoginHierarchy()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/UI_LoginHierarchy/UI_LoginHierarchy.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/Login (UI file)", false, 58)]
    static void Login()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/Login/UI_Login.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/MenuOthers", false, 59)]
    static void MenuOthers()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/MenuOthers/UI_MenuOthers.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/CreateChar", false, 60)]
    static void CreateChar()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/CreateChar/UI_CreateChar.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/Tutorial", false, 61)]
    static void Tutorial()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/Dialog/Tutorial/Tutorial/Dialog_Tutorial.unity";
        OpenNewScene(myScene);
    }

    [MenuItem("ZDGUI/ViewPlayer ----for Artists", false, 62)]
    static void ViewPlayer()
    {
        string myScene = "Assets/UI_ZDSP/Tools/ViewPlayer/ViewPlayerPiliQ.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/----CharacterInfo", false, 60)]
    static void CharacterInfo()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/CharacterInfo/UI_CharacterInfo.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/----Inventory", false, 60)]
    static void Inventory()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/Inventory/UI_Inventory.unity";
        OpenNewScene(myScene);
    }
	
	[MenuItem("ZDGUI/----ItemDetail", false, 60)]
    static void ItemDetail()
    {
        string myScene = "Assets/UI_ZDSP/Scenes/aaa_Dialog/ItemDetail/Dialog_ItemDetail.unity";
        OpenNewScene(myScene);
    }

    //####################################################################
    // Open Scene with confirmation
    //####################################################################
    //------------------------------------------
    static void OpenNewScene(string myScene)
    {
        var q = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        if (q)
        {
            EditorSceneManager.OpenScene(myScene);
        }
    }
}
