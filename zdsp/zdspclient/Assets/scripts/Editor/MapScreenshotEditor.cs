using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MapScreenshot))]
public class MapScreenshotEditor : Editor
{
    private GameObject mcp;
    private Camera mcpcam;
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        
        if (GUILayout.Button("Save Screenshot"))
        {
            mcp = GameObject.Find("MapSize");
            //string[] strs = EditorApplication.currentScene.Split(char.Parse("/"));
            //string lname = strs[strs.Length - 1];
            //strs = lname.Split(char.Parse("."));
            var lname = EditorSceneManager.GetActiveScene().name;
            string path = EditorUtility.SaveFilePanel("Save Map Screenshot", "", "map_" + lname + ".png", "png");

            if (path != "")
            {
                mcpcam = mcp.GetComponent<Camera>();
                if (mcpcam == null) mcpcam = mcp.AddComponent<Camera>();                
                mcpcam.orthographic = true;
                mcpcam.orthographicSize = mcp.transform.localScale.x*256;
                mcpcam.aspect = 1.0f;
                mcpcam.farClipPlane = 1000f;
                mcpcam.nearClipPlane = 0.3f;
                Vector3 mcppos = mcp.transform.position;
                mcpcam.transform.position = new Vector3(mcppos.x, mcppos.y+1, mcppos.z);
                mcpcam.transform.LookAt(mcppos, new Vector3(0f, 0f, 1f));
                

                mcpcam.targetTexture = new RenderTexture(512,
                                                   512, 24,
                                                   RenderTextureFormat.ARGB32);

                RenderTexture currentRT = RenderTexture.active;
                RenderTexture.active = mcpcam.targetTexture;
                mcpcam.Render();
                Texture2D image = new Texture2D(mcpcam.targetTexture.width, mcpcam.targetTexture.height, TextureFormat.ARGB32, false);
                image.ReadPixels(new Rect(0, 0, mcpcam.targetTexture.width, mcpcam.targetTexture.height), 0, 0);
                image.Apply();
                RenderTexture.active = currentRT;
                byte[] imgdata = image.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, imgdata);
                Debug.Log("screenshot saved to " + path);
                DestroyImmediate(mcpcam);
                mcpcam = null;
            }            
        }
    }

}

