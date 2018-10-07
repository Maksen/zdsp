using UnityEngine;
using UnityEditor;
using System.IO;

namespace Teenotheque.SplatPainter
{
	public static class SplatPainterUtility
	{
		static void Create(int p_width, int p_height, Color32 p_color)
		{
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);

			if (path == "") 
				path = "Assets";
			else if (Path.GetExtension (path) != "") 
				path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New Splatmap.png");
			Texture2D tex = new Texture2D(p_width, p_height, TextureFormat.RGBA32, true);
			Color32[] colors = new Color32[p_width * p_height];
			
			for (int i = 0; i != colors.Length; ++i)
				colors[i] = p_color;
			
			tex.SetPixels32(colors);
			byte[] pngData = tex.EncodeToPNG();
			File.WriteAllBytes(assetPathAndName, pngData);
			Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(assetPathAndName);

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            ti.textureFormat = TextureImporterFormat.RGBA32;
#else
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.sRGBTexture = false;
#endif
            
            ti.wrapMode = TextureWrapMode.Clamp;
            AssetDatabase.ImportAsset(assetPathAndName);
            AssetDatabase.Refresh();
        }

		[MenuItem("Assets/Create/Splatmap/Black 32x32")]
		static void CreateBlack32x32()
		{
			Create(32, 32, new Color32(0, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Black 64x64")]
		static void CreateBlack64x64()
		{
			Create(64, 64, new Color32(0, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Black 128x128")]
		static void CreateBlack128x128()
		{
			Create(128, 128, new Color32(0, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Black 256x256")]
		static void CreateBlack256x256()
		{
			Create(256, 256, new Color32(0, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Black 512x512")]
		static void CreateBlack512x512()
		{
			Create(512, 512, new Color32(0, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Black 1024x1024")]
		static void CreateBlack1024x1024()
		{
			Create(1024, 1024, new Color32(0, 0, 0, 0));
		}

        [MenuItem("Assets/Create/Splatmap/Black 2048x2048")]
        static void CreateBlack2048x2048()
        {
            Create(2048, 2048, new Color32(0, 0, 0, 0));
        }

        [MenuItem("Assets/Create/Splatmap/Red 32x32")]
		static void CreateRed32x32()
		{
			Create(32, 32, new Color32(255, 0, 0, 0));
		}
		
		[MenuItem("Assets/Create/Splatmap/Red 64x64")]
		static void CreateRed64x64()
		{
			Create(64, 64, new Color32(255, 0, 0, 0));
		}
		
		[MenuItem("Assets/Create/Splatmap/Red 128x128")]
		static void CreateRed128x128()
		{
			Create(128, 128, new Color32(255, 0, 0, 0));
		}
		
		[MenuItem("Assets/Create/Splatmap/Red 256x256")]
		static void CreateRed256x256()
		{
			Create(256, 256, new Color32(255, 0, 0, 0));
		}
		
		[MenuItem("Assets/Create/Splatmap/Red 512x512")]
		static void CreateRed512x512()
		{
			Create(512, 512, new Color32(255, 0, 0, 0));
		}

		[MenuItem("Assets/Create/Splatmap/Red 1024x1024")]
		static void CreateRed1024x1024()
		{
			Create(1024, 1024, new Color32(255, 0, 0, 0));
		}

        [MenuItem("Assets/Create/Splatmap/Red 2048x2048")]
        static void CreateRed2048x2048()
        {
            Create(2048, 2048, new Color32(255, 0, 0, 0));
        }
    }
}
