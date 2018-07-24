using UnityEngine;
using System.Collections;

public class OutlinePostEffect : MonoBehaviour
{
    Camera AttachedCamera;
    Camera TempCam;
    RenderTexture TempRT = null;

    Material Post_Mat;
    public Shader Simple, PostOutline;

    public int outline_thickness = 20;
    public int blur_iterations = 20;
    public Color32 outline_colour = new Color32(0, 255, 255, 255);

    void Start()
    {
        AttachedCamera = GetComponent<Camera>();
        TempCam = new GameObject().AddComponent<Camera>();
        TempCam.transform.SetParent(transform);
        TempCam.name = "OutlineCam";
        TempCam.enabled = false;

        var pcversion = Shader.Find("Custom/Post OutlinePC");

        if (pcversion != null)
            PostOutline = pcversion;

        Post_Mat = new Material(PostOutline); 

        Post_Mat.EnableKeyword("_SCENETEX");
        Post_Mat.EnableKeyword("_THICKNESS");
        Post_Mat.EnableKeyword("_BLUR");

        Simple = Shader.Find("Custom/DrawSimple");
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //set up a temporary camera
        TempCam.CopyFrom(AttachedCamera);
        TempCam.clearFlags = CameraClearFlags.Color;
        TempCam.backgroundColor = Color.black;

        //cull any layer that isn't the outline
        TempCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");

        //make the temporary rendertexture
        if (TempRT == null)
        {
            TempRT = new RenderTexture(source.width, source.height, 0);

            //put it to video memory
            TempRT.Create();
        }

        //set the camera's target texture when rendering
        TempCam.targetTexture = TempRT;

        //render all objects this camera can render, but with our custom shader.
        TempCam.RenderWithShader(Simple, "");

        Post_Mat.SetTexture("_SceneTex", source);
        Post_Mat.SetInt("_Thickness", outline_thickness);
        Post_Mat.SetInt("_Blur", blur_iterations);
        Post_Mat.SetColor("_Colour", outline_colour);

        //copy the temporary RT to the final image
        Graphics.Blit(TempRT, destination, Post_Mat);
    }
}