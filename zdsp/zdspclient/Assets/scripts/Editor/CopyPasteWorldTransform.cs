using UnityEngine;
using UnityEditor;
using System.Collections;

public class CopyPasteWorldTransform : MonoBehaviour {
    private static Transform mTransform;

	[MenuItem ("Tools/WorldTransform/Copy")]
	static void Copy() {
        var transforms = Selection.transforms;
        if (transforms.Length == 1)
            mTransform = transforms[0];
	}

    [MenuItem("Tools/WorldTransform/Paste")]
    static void Paste()
    {
        var transforms = Selection.transforms;
        if (transforms.Length == 1)
        {
            transforms[0].position = mTransform.position;
            transforms[0].rotation = mTransform.rotation;
            transforms[0].localScale = mTransform.localScale;
        }
    }
}
