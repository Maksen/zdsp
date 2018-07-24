using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;

public class CustomPackageExporter : EditorWindow {
	Dictionary<string, bool> coreFolders = new Dictionary<string, bool>();
	ArrayList coreFolderKeys = new ArrayList();
	Dictionary<string, bool> assetFiles = new Dictionary<string, bool> ();
	ArrayList assetFileKeys = new ArrayList();
	Vector2 scrollPosition = new Vector2 ();
	string datapath = Application.dataPath.Replace ("Assets", "");
	Dictionary<string, bool> subfiletype = new Dictionary<string, bool>();
	List<string> filetype = new List<string>(){".prefab", ".shader", ".mat", ".bmp", ".png", ".psd", ".jpg", ".tif", ".tga", ".wav", ".fbx"};
	Dictionary<string, string[]> inheritFiles = new Dictionary<string, string[]> ();
	Dictionary<string, bool> exportFilesList = new Dictionary<string, bool> ();
	bool confirmExport = false;
	Vector2 exportlistScrollPosition = new Vector2 ();

	[MenuItem ("Tools/Custom Package Exporter")]
	static void Init()
	{
		CustomPackageExporter cpeWindow = (CustomPackageExporter)EditorWindow.GetWindow (typeof(CustomPackageExporter), true, "Custom Package Exporter");
		cpeWindow.Show ();
		cpeWindow.InitWindow ();
	}

	public void OnGUI()
	{
		if (!confirmExport) {
			GUILayout.BeginArea (new Rect (0, 0, position.width, 250));
			GUILayout.BeginHorizontal ();
			int index = 0;
			foreach (string key in coreFolderKeys) {
				if (index % 10 == 0)
					GUILayout.BeginVertical ();

				coreFolders [key] = GUILayout.Toggle (coreFolders [key], key, GUILayout.MinWidth (250));
				index++;

				if (index % 10 == 0 || index >= coreFolderKeys.Count)
					GUILayout.EndVertical ();
			}
			GUILayout.EndHorizontal ();

			EditorGUILayout.Separator ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("All", GUILayout.MaxWidth (100)))
				OnSelectedAll ();
			if (GUILayout.Button ("None", GUILayout.MaxWidth (100)))
				OnSelectedNone ();
			if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (100)))
				OnRefresh ();
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (0, 250, position.width, position.height - 300));
			scrollPosition = GUILayout.BeginScrollView (scrollPosition);
			foreach (string key in assetFileKeys) {
				assetFiles [key] = GUILayout.Toggle (assetFiles [key], key, GUILayout.MinWidth (250));
			}
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (0, 220, position.width, 50));
			GUILayout.BeginHorizontal ();
			foreach (string type in filetype) {
				subfiletype [type] = GUILayout.Toggle (subfiletype [type], type);
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (0, position.height - 50, position.width, 50));
			if (GUILayout.Button ("Confirm", GUILayout.MaxWidth (100))) {
				OnConfrim ();
			}
			GUILayout.EndArea ();
		} else {
			GUILayout.BeginArea (new Rect (0, 0, position.width, position.height-50));
			exportlistScrollPosition = GUILayout.BeginScrollView (exportlistScrollPosition);
			foreach(KeyValuePair<string, string[]> data in inheritFiles)
			{
				string key = data.Key;
				exportFilesList[key] = EditorGUILayout.BeginToggleGroup(key, exportFilesList[key]);
				string[] subfiles = data.Value;
				foreach(string file in subfiles)
				{
					exportFilesList[file] = EditorGUILayout.ToggleLeft(file, exportFilesList[file]);
				}
				EditorGUILayout.EndToggleGroup();
			}
			GUILayout.EndScrollView ();
			GUILayout.EndArea();

			GUILayout.BeginArea (new Rect (0, position.height - 50, position.width, 50));
			GUILayout.BeginHorizontal ();			
			if (GUILayout.Button ("Export", GUILayout.MaxWidth (100))) {
				OnExportAssets ();
			}
			if (GUILayout.Button ("Cancel", GUILayout.MaxWidth (100))) {
				OnCancel ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
		}
	}

	private void InitWindow()
	{
		subfiletype.Clear ();
		foreach (string type in filetype) {
			subfiletype.Add(type, true);
		}

		foreach (string file in Directory.GetFiles("Assets/")) {
			string name = file.Replace (".meta", "");

			if (!coreFolders.ContainsKey(name))
			{
				coreFolders.Add (name, false);
				coreFolderKeys.Add(name);
			}
		}
	}

	private void OnSelectedAll()
	{
		foreach (string key in coreFolderKeys)
			coreFolders[key] = true;
	}

	private void OnSelectedNone()
	{
		foreach (string key in coreFolderKeys)
			coreFolders[key] = false;
	}

	private void OnRefresh()
	{
		assetFiles.Clear ();
		assetFileKeys.Clear ();

		foreach (string key in coreFolderKeys) {
			if (coreFolders[key])
				GetFilesByPath(key);
		}
	}

	private void OnConfrim()
	{
		inheritFiles.Clear ();
		exportFilesList.Clear ();
		List<string> selectedfiles = new List<string> ();
		foreach (string key in assetFileKeys) {
			if (assetFiles[key])
			{
				selectedfiles.Add(key);
				inheritFiles.Add(key, new string[]{});
				exportFilesList.Add(key, true);
			}
		}

		if (selectedfiles.Count == 0) {
			EditorUtility.DisplayDialog ("Export Package Error", "None File Had Been Selected", "OK");
		} else {
			foreach(string filename in selectedfiles)
			{
				string[] dependencies = AssetDatabase.GetDependencies(new string[]{filename});
				List<string> subfile = new List<string>();
				foreach(string dependency in dependencies)
				{
					foreach(string type in filetype)
					{
						if (subfiletype[type] && dependency.Contains(type))
						{
							subfile.Add(dependency);
							if (!exportFilesList.ContainsKey(dependency))
								exportFilesList.Add(dependency, true);
						}
					}
				}
				inheritFiles[filename] = subfile.ToArray();
			}
			confirmExport = true;
		}
	}

	private void OnExportAssets()
	{
		string savepath = EditorUtility.SaveFilePanel ("Export Packages", datapath, "", "unitypackage");
		if (savepath != null) {
			List<string> filetoexport = new List<string>();
			foreach(KeyValuePair<string, bool> file in exportFilesList)
			{
				string key = file.Key;
				if (exportFilesList[key])
					filetoexport.Add(key);
			}
			AssetDatabase.ExportPackage(filetoexport.ToArray(), savepath);
			assetFiles.Clear ();
			assetFileKeys.Clear ();
			confirmExport = false;
		}
	}

	private void OnCancel()
	{
		confirmExport = false;
	}

	private void GetFilesByPath(string path)
	{
		string[] files = Directory.GetFiles (path+"/", "*", SearchOption.AllDirectories);
		foreach (string file in files) {
			if (!file.Contains(".meta"))
			{
				string name = file.Replace("\\","/");

				if (!assetFiles.ContainsKey(name))
				{
					assetFiles.Add(name, false);
					assetFileKeys.Add(name);
				}
			}
		}
	}
}
