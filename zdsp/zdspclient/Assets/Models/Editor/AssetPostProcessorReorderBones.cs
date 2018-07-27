// Original: https://gist.github.com/tsubaki/ea6ece1cd9a851ff977e#file-skinnedmeshupdater-cs
// Updated on 01/02/2018 by Brian Cheng
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

//sorts transform bone indexes in skinned mesh renderers so that we can swap skinned meshes at runtime
public class AssetPostProcessorReorderBones : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        Process(g);
    }

    public void Process(GameObject g)
    {
        if (assetPath != null)
            if (!assetPath.Contains("Models/Characters/Pc_job") && !assetPath.Contains("Models/Characters/Pc_fashion"))
                return;

        var rends = g.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (rends == null)
        {
            Debug.LogWarning("Unable to find any Renderers");
            return;
        }

        foreach (var rend in rends)
        {            
            //list of bones
            List<Transform> tList = rend.bones.ToList();

            //sort alphabetically
            tList.Sort(CompareTransform);

            //record bone index mappings (richardf advice)
            //build a Dictionary<int, int> that records the old bone index => new bone index mappings,
            //then run through every vertex and just do boneIndexN = dict[boneIndexN] for each weight on each vertex.
            Dictionary<int, int> remap = new Dictionary<int, int>();
            for (int i = 0; i < rend.bones.Length; i++)
            {
                remap[i] = tList.IndexOf(rend.bones[i]);
            }

            //remap bone weight indexes
            BoneWeight[] bw = rend.sharedMesh.boneWeights;
            for (int i = 0; i < bw.Length; i++)
            {
                bw[i].boneIndex0 = remap[bw[i].boneIndex0];
                bw[i].boneIndex1 = remap[bw[i].boneIndex1];
                bw[i].boneIndex2 = remap[bw[i].boneIndex2];
                bw[i].boneIndex3 = remap[bw[i].boneIndex3];
            }

            //remap bindposes
            Matrix4x4[] bp = new Matrix4x4[rend.sharedMesh.bindposes.Length];
            for (int i = 0; i < bp.Length; i++)
            {
                bp[remap[i]] = rend.sharedMesh.bindposes[i];
            }

            //assign new data
            rend.bones = tList.ToArray();
            rend.sharedMesh.boneWeights = bw;
            rend.sharedMesh.bindposes = bp;
        }
    }

    private static int CompareTransform(Transform A, Transform B)
    {
        return A.name.CompareTo(B.name);
    }
}

public class BoneDisplay : ScriptableWizard
{
    public GameObject Model = null;

    [MenuItem("Tools/Bone Display")]
    static public void OpenWindow()
    {
        ScriptableWizard.DisplayWizard<BoneDisplay>("Display bones");
    }

    public void OnWizardCreate()
    {
        if (Model == null)
            return;

        var assetproc = new AssetPostProcessorReorderBones();

        assetproc.Process(Model);

        var models = Model.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var model in models)
        {
            Debug.Log("Displaying model: " + model.name);

            int count = 0;
            foreach (var bone in model.bones)
            {
                Debug.Log("[" + count + "] " + bone.name);

                ++count;
            }
        }
    }
}