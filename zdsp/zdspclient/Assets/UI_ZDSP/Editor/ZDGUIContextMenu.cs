using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
//using System.Collections;

public class ZDGUIContextMenu : EditorWindow
{
	// %(Ctrl), &(Alt), _(None)
		
    [MenuItem("GameObject/--> Select Current Scene", false, -1050)]
    static void SelectCurrentScene()
    {
        string myScene = EditorSceneManager.GetActiveScene().path;

        if (myScene == "")
        {
            Debug.LogFormat("<b>" + "Your scene is not saved yet!" + "</b>");
        }
        else
        {
            Debug.LogFormat("<b><color=Blue>" + myScene + "</color></b>");
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(myScene));
        }

    }


  
    public static void DiffPrefab()
    {

        var propertyMods = PrefabUtility.GetPropertyModifications(Selection.activeGameObject);
        if (propertyMods != null)
        {
            var window = EditorWindow.GetWindow<ToolsPrefabDiff>(true, "Diff Prefab", true);

            window.Values = propertyMods;
            window.parentSelected = Selection.activeGameObject;
            window.ShowPopup();
        }
        else
        {
            EditorUtility.DisplayDialog("Diff Prefab Warning!", "Game Object is not a prefab!", "OK");
        }

    }
    [MenuItem("GameObject/~ Diff Prefab ~", false, -1000)]
    public static void DiffPrefab2()
    {
        
        int count = 0;
        System.Collections.Generic.IEnumerable<PropertyModification>[] allvalue = new System.Collections.Generic.IEnumerable<PropertyModification>[Selection.objects.Length];
        GameObject[] allobj = new GameObject[Selection.objects.Length]; 
        foreach (var obj in Selection.objects)
        {
            var mods = PrefabUtility.GetPropertyModifications(obj);
            GameObject tempobj = obj as GameObject;
            if (mods == null)
            {
                EditorUtility.DisplayDialog("Diff Prefab Warning!", tempobj.name + " is not a prefab!", "OK");
                return;
            }
            else
            {
                allobj[count] = tempobj;
                allvalue[count] = mods;
            }
            count++;
        }
        var window = EditorWindow.GetWindow<ToolsPrefabDiff>(true, "Diff Prefab", true);
        window.allobj = allobj;
        window.allValues = allvalue;
        window.ShowPopup();
        

    }


    [MenuItem("GameObject/!!!!!! Make UI Prefab !!!!!!", false, -1000)]
    public static void CreateUIPrefab()
    {
        UIEditorTools.CreateUIPrefab();
    }
	
	//####################################################################
    // Highlight
    [MenuItem("GameObject/--- Highlight/Tag Highlighted", false, 0)]
    static void TagHighlighted()
    {
        Selection.activeTransform.tag = "Highlight";
    }

    [MenuItem("GameObject/--- Highlight/Remove Highlighted", false, 0)]
    static void RemoveHighlighted()
    {
        Selection.activeTransform.tag = "Untagged";
    }

    [MenuItem("GameObject/--- Highlight/Find Highlighted", false, 0)]
    static void FindHighlighted()
    {
        //GameObject[] highlights = GameObject.FindGameObjectsWithTag("Highlight");
        //Selection.objects = highlights;

        GameObject[] objs = new GameObject[] { };
        objs = Resources.FindObjectsOfTypeAll<GameObject>() as GameObject[];
        //Debug.Log("Orig Length :" + objs.Length);

        for (int i = 0; i < objs.Length; i++)
        {
            if (!objs[i].CompareTag("Highlight"))
            {
                objs[i] = null;
            }
        }
        Selection.objects = objs;
        //Debug.Log("New Length :" + objs.Length);
    }
    //####################################################################

    [MenuItem("GameObject/Reset Transform 0,0,0 Scale 1,1,1", false, -1000)]
    public static void ResetTransform()
    {
        RectTransform m_RectTransform;
        m_RectTransform = Selection.activeGameObject.GetComponent<RectTransform>();

        if (!m_RectTransform)
        {
            //No Rect Transform, no anchors in Transform
            Selection.activeTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else
        {
            //Have RectTransform, have anchors, so use anchors
            m_RectTransform.anchoredPosition = new Vector3(0.0f, 0.0f, 0.0f);
        }
        Selection.activeTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
	
	[MenuItem("GameObject/Reset Scale 1,1,1", false, -1000)]
    public static void ResetScale()
    {
        Selection.activeTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    [MenuItem("Assets/", false, 1000)]
    static void Seperator1()
    {

    }
    
    [MenuItem("Assets/UI_GOGOGO/---> Scene", false, 10000)]
    static void UIScene()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Scenes/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Scene (HUD)", false, 10001)]
    static void UISceneCombat()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Scenes/aaa_HUD/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Scene (Dialog)", false, 10001)]
    static void UISceneDialog()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Scenes/aaa_Dialog/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Scene (SystemMessage)", false, 10001)]
    static void UISceneSystemMessage()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Scenes/aaa_SystemMessage/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Textures", false, 10002)]
    static void UITextures()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Textures/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Widgets", false, 10004)]
    static void UIWidgets()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Widgets/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Scripts", false, 10005)]
    static void UIScripts()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Scripts/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Sound", false, 10006)]
    static void UISound()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Sound/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Icons", false, 10007)]
    static void Icons()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Icons/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> GameIcon", false, 10008)]
    static void GameIconFolder()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Widgets/GameIcon/GameIcon.unity");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> Tabs", false, 10008)]
    static void TabFolder()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_ZDSP/Widgets/Tabs/Tabs.unity");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [MenuItem("Assets/UI_GOGOGO/---> UI (PiLiQ)", false, 10008)]
    static void PiLiUIScenesFolder()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/UI_PiLiQ/root.tif");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

	//####################################################################
	//Editor Only
	
	//GameIcon
    [MenuItem("GameObject/ZDGUI/EditorOnly/GameIcon", false, -110)]
    static void GameIcon()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/GameIcon/GameIcon_Equip/P_GameIcon_Equip/GameIcon_Equip.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
		Selection.activeObject.name = "GameIcon_Equip_SAMPLEDELETE";
    }
	
	//Chinese Text
    [MenuItem("GameObject/ZDGUI/EditorOnly/ChineseText", false, -110)]
    static void ChineseText()
    {
        //Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ChineseText.prefab", typeof(GameObject));
        //CreateAsChild(uiWidget);
        Selection.activeGameObject.GetComponent<Text>().text = "咖哩魚頭";
    }

	//Image_REFFFFFFFFFFFFFFFFFF
    [MenuItem("GameObject/ZDGUI/EditorOnly/Image_REFFFFFFFFFFFFFFFF", false, -110)]
    static void ImageREFFFFF()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Image_REFFFFFFFFFFFFFFFFFF.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	//3D Cube
    [MenuItem("GameObject/ZDGUI/EditorOnly/3DCube", false, -110)]
    static void EDITORONLY3DTEST()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/EditorOnly3DCube/EDITOR_ONLY_3D_CUBE.prefab", typeof(GameObject));
        CreateInRootNoBreak(uiWidget);
    }

	//Test Button
    [MenuItem("GameObject/ZDGUI/EditorOnly/TestButton", false, -110)]
    static void TestButton()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/TestButton/TestButton.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	//Avatar Man
	[MenuItem("GameObject/ZDGUI/EditorOnly/Avatar3D_Man (set layer to UI)", false, -110)]
    static void Avatar3D_Man()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/Models/NOTFORBUILD/performance_test/prefab/prefab_testman_01.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	//Avatar Woman
	[MenuItem("GameObject/ZDGUI/EditorOnly/Avatar3D_Female (set layer to UI)", false, -110)]
    static void Avatar3D_Female()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/Models/NOTFORBUILD/performance_test/prefab/prefab_testfemale_01.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //####################################################################
    //MISC
	
	//FPS Counter
    [MenuItem("GameObject/ZDGUI/Misc/FPS", false, -101)]
    static void FPS()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/FPS/Canvas_ssOverlay_FPS.prefab", typeof(GameObject));
        CreateInRootNoBreak(uiWidget);
    }

    //NewDot_HUD
    [MenuItem("GameObject/ZDGUI/Misc/NewDot_HUD", false, -100)]
    static void NewDot_HUD()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/NewDot/Image_NewDot_HUD.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //NewDot_Window
    [MenuItem("GameObject/ZDGUI/Misc/NewDot_UIWindow", false, -100)]
    static void NewDot_UIWindow()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/NewDot/Image_NewDot_UIWindow.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	//NewDot_ForUiMask
    [MenuItem("GameObject/ZDGUI/Misc/NewDot_GameIcon_ForUiMask", false, -100)]
    static void NewDot_GameIcon_ForUiMask()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/NewDot/Image_NewDot_GameIcon_ForUiMask.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	//GameObject_Panel (The Black Boring Panel)
    [MenuItem("GameObject/ZDGUI/Misc/GameObject_Panel (The Black Boring Panel)", false, -100)]
    static void GameObject_Panel()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/GameObject_Panel.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	//Image_Frame (Outline Frame)
    [MenuItem("GameObject/ZDGUI/Misc/Image_Frame (Outline Frame)", false, -100)]
    static void Image_Frame()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Image_Frame.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	//Image_SpriteMaskArea
    [MenuItem("GameObject/ZDGUI/Misc/Image_SpriteMaskArea", false, -100)]
    static void Image_SpriteMaskArea()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Image_SpriteMaskArea.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	[MenuItem("Assets/---> Localizer_Text.cs, Find Asset Reference. Heirarchy View, Root, Search", false, 10008)]
    static void Localizer_Text()
    {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/scripts/GUI/Common/UI/Localization/Localizer_Text.cs");
    }
	
	//LocalizerEditor
    [MenuItem("GameObject/ZDGUI/Misc/LocalizerEditor", false, -100)]
    static void LocalizerEditor()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/LocalizerEditor.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    //CoolDown
    [MenuItem("GameObject/ZDGUI/Misc/CoolDown", false, -100)]
    static void CoolDown()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/CoolDown/CoolDown.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Misc/Expander (Only Expander Button)", false, -98)]
    static void Expander()
    {
           Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Expander/P_Expander/Expander.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Misc/Expander_VerticalScroll (Vertical Scroll and Panel)", false, -98)]
    static void Expander_VerticalScroll()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Expander/P_Expander/Expander_VerticalScroll.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Misc/ComboBoxA", false, -97)]
    static void ComboBoxA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ComboBox/newnew/P_ComboBox/ComboBoxA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //####################################################################

    //Tabs
    [MenuItem("GameObject/ZDGUI/Tabs/TabHorizontalA (Tab Top)", false, 80)]
    static void TabHorizontalA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Tabs/P_Tab/TabHorizontalA.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Tabs/TabVerticalB (Tab Left)", false, 80)]
    static void TabVerticalB()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Tabs/P_Tab/TabVerticalB.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Tabs/TabVerticalB_Scroll (Tab Left and Scrolling)", false, 80)]
    static void TabVerticalB_Scroll()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Tabs/P_Tab/TabVerticalB_Scroll.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Tabs/TabVerticalC (Icon Only)", false, 80)]
    static void TabVerticalC()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Tabs/P_Tab/TabVerticalC.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    //####################################################################

    //TEXT
    [MenuItem("GameObject/ZDGUI/Text/Text (20, Default)", false, 51)]
    static void Text20()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Text/Text_20.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Text/Text (22, For Buttons)", false, 52)]
    static void Text22()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Text/Text_22.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Text/HeaderTitle", false, 52)]
    static void HeaderTitle()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/HeaderTitle/newnew/HeaderTitle.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Text/GameObject_HorizontalLayout", false, 52)]
    static void GameObject_HorizontalLayout()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Text/GameObject_HorizontalLayout.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    //####################################################################

    //BUTTONS
    [MenuItem("GameObject/ZDGUI/Buttons/ButtonA1", false, 101)]
    static void ButtonA1()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/newnew/ButtonA1.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Buttons/ButtonA2", false, 101)]
    static void ButtonA2()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/newnew/ButtonA2.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //------------------------------------------

    [MenuItem("GameObject/ZDGUI/Buttons/--LeftRight/Button_Left", false, 102)]
    static void Button_Left()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/Button_Left.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Buttons/--LeftRight/Button_Right", false, 102)]
    static void Button_Right()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/Button_Right.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Buttons/--ToggleButton/CustomToggleA (Template)", false, 102)]
    static void CustomToggleA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/CustomToggleA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Buttons/--ToggleButton/CheckBoxA", false, 102)]
    static void CheckBoxA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/CheckBoxA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Buttons/--ToggleButton/RadioButtonA", false, 102)]
    static void RadioButtonA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Button/RadioButtonA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }
	
	//------------------------------------------

    //SPINNER
    [MenuItem("GameObject/ZDGUI/Spinner/SpinnerA", false, 201)]
    static void SpinnerA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/SpinnerA/SpinnerA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }


    //####################################################################

    //INPUT FIELD
    [MenuItem("GameObject/ZDGUI/InputField/InputFieldA", false, 251)]
    static void InputFieldA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/InputField/InputFieldA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //####################################################################

    //SCROLL VIEW
    [MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewA (Vertical)", false, 301)]
    static void ScrollviewA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/newnew/ScrollviewA.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewB (Vertical, No Scrollbars)", false, 302)]
    static void ScrollviewB()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/newnew/ScrollviewB.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewC (Horizontal)", false, 303)]
    static void ScrollviewC()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/ScrollviewC.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
	
	[MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewC2 (Horizontal with BAR)", false, 303)]
    static void ScrollviewC2()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/newnew/ScrollviewC2.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }

    [MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewD (Carousel)", false, 303)]
    static void ScrollviewD()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/ScrollviewD.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }


    [MenuItem("GameObject/ZDGUI/Scrollview/ScrollviewS (Scroll Snap)", false, 304)]
    static void ScrollSnap()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ScrollView/newnew/ScrollviewS.prefab", typeof(GameObject));
        CreateAsChild(uiWidget);
    }
    //####################################################################

    //SLIDER
    [MenuItem("GameObject/ZDGUI/Slider/SliderA", false, 401)]
    static void Slider()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/Slider/SliderA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //####################################################################

    //PROGRESSBAR
    [MenuItem("GameObject/ZDGUI/ProgressBar/ProgressBarFillA (Horizontal Fill)", false, 451)]
    static void ProgressBarFillA()
    {
        Object uiWidget = AssetDatabase.LoadAssetAtPath("Assets/UI_ZDSP/Widgets/ProgressBarFill/ProgressBarFillA.prefab", typeof(GameObject));
        CreateAsChildNoBreak(uiWidget);
    }

    //####################################################################

    //####################################################################
    // Create widget as a child of selection
    //####################################################################

    //------------------------------------------
    //Create as a child object
    static void CreateAsChild(Object uiWidget)
    {
        var newPrefab = PrefabUtility.InstantiatePrefab(uiWidget) as GameObject;
        newPrefab.transform.SetParent(Selection.activeTransform, false);
        PrefabUtility.DisconnectPrefabInstance(newPrefab);
        Selection.activeObject = newPrefab;

    }

    static void CreateAsChildNoBreak(Object uiWidget)
    {
        var newPrefab = PrefabUtility.InstantiatePrefab(uiWidget) as GameObject;
        newPrefab.transform.SetParent(Selection.activeTransform, false);
        //PrefabUtility.DisconnectPrefabInstance(newPrefab);
        Selection.activeObject = newPrefab;
    }

    //Create In Root
    static void CreateInRootNoBreak(Object uiWidget)
    {
        var newPrefab = PrefabUtility.InstantiatePrefab(uiWidget) as GameObject;
        //PrefabUtility.DisconnectPrefabInstance(newPrefab);
        Selection.activeObject = newPrefab;
    }
}