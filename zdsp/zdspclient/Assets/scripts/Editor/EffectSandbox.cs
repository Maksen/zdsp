using UnityEngine;
using System.Collections;

using UnityEditor;
using UnityEditor.SceneManagement;
public class EffectSandbox : EditorWindow
{

    [MenuItem("Tools/Add Effect Animtion Preview", false, 0)]
    static void addSandbox()
    {
        GameObject selection = Selection.activeGameObject;
        if (selection == null)
        {
            Debug.LogError("please select an model prefeb with animation");
            return;
        }
        Animator animtion = selection.GetComponent<Animator>();
        if (animtion != null)
        {
            PlayAnimationSandbox temp = selection.GetComponentInChildren<PlayAnimationSandbox>();
            if (temp == null)
            {
                GameObject holder = new GameObject();
                holder.name = "sandboxhelper";
                holder.tag = "EditorOnly";
                holder.transform.SetParent(selection.transform);
                temp = holder.AddComponent<PlayAnimationSandbox>();
                temp.target = selection;
                temp.Init();
                Selection.activeGameObject = holder;
            } 
        }
        //SceneNPCTools.CompileSceneAssets(true);
    }

}
