using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a Icon on GameObjects in the Hierarchy that contain the Tag Highlight.
/// </summary>
[InitializeOnLoad]
public class HighlightHierarchyIcon
{
    private static readonly Texture2D Icon;

    static HighlightHierarchyIcon()
    {
        Icon = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/Highlight.tif", typeof(Texture2D)) as Texture2D;

        if (Icon == null)
        {
            Debug.LogError("Missing icon inside Gizmos folder. Highlight.tif");
            return;
        } 

        EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
    }

    private static void DrawIconOnWindowItem(int instanceID, Rect rect)
    {
        if (Icon == null)
        {
            return;
        }

        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null)
        {
            return;
        }


        if (gameObject.tag != null && gameObject.tag == "Highlight")
        {
            float iconWidth = 50;
            EditorGUIUtility.SetIconSize(new Vector2(iconWidth, 20));
            var iconDrawRect = new Rect(
                                   rect.xMax - (rect.width + 3),
                                   rect.yMin,
                                   rect.width,
                                   rect.height + 2                                   
                                   );
            var iconGUIContent = new GUIContent(Icon);
            EditorGUI.LabelField(iconDrawRect, iconGUIContent);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }
    }
}