using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Teenotheque.SplatPainter
{
    public class SplatPainterWindow : EditorWindow
    {
        readonly string[,] DefaultPropertiesNames = new string[,]
        {
            {"_SplatMap01", "_Albedo01", "_Albedo02", "_Albedo03", "_Albedo04"},
            {"_SplatMap02", "_Albedo05", "_Albedo06", "_Albedo07", "_Albedo08"},
            {"_SplatMap03", "_Albedo09", "_Albedo10", "_Albedo11", "_Albedo12"},
            {"_SplatMap04", "_Albedo13", "_Albedo14", "_Albedo15", "_Albedo16"}
        };

        readonly string[,] TintPropertiesNames = new string[,]
        {
            {"_SplatMap01", "_TintColor01", "_TintColor02", "_TintColor03", "_TintColor04"},
            {"_SplatMap02", "_TintColor05", "_TintColor06", "_TintColor07", "_TintColor08"},
            {"_SplatMap03", "_TintColor09", "_TintColor10", "_TintColor11", "_TintColor12"},
            {"_SplatMap04", "_TintColor13", "_TintColor14", "_TintColor15", "_TintColor16"}
        };

        readonly string[,] NormalMapNames = new string[,]
        {
            {"_SplatMap01", "_NormalMap01", "_NormalMap02", "_NormalMap03", "_NormalMap04"}
        };

        readonly string[,] HeightMapNames = new string[,]
        {
            {"_SplatMap01", "_ParallaxMap01", "_ParallaxMap02", "_ParallaxMap03", "_ParallaxMap04"}
        };

        readonly string[,] IllumNames = new string[,]
        {
            {"_SplatMap01", "_Illum01", "_Illum02", "_Illum03", "_Illum04"}
        };

        readonly string[] ChannelUVs = new string[]
        {
            "UV0", "UV1"
        };

        readonly string[] ResolutionsNames = new string[]
        {
            "32", "64", "128", "256", "512", "1024", "2048"
        };

        readonly int[] Resolutions = new int[]
        {
            32, 64, 128, 256, 512, 1024, 2048
        };

        const int MaxBrushSize = 500;
        const int DefaultBrushSize = 50;
        const float DefaultBrushOpacity = 1f;
        const float WindowMinSizeX = 230f;
        const float BrushCellSize = 40f;
        const float DiffCellSize = 64f;
        const float LabelWidth = 60f;
        const float StartStopButtonHeight = 64f;
        const int StartStopButtonFontSize = 20;
        const float RayLength = 1000f;

        readonly Color32 ColorBlack = new Color32(0, 0, 0, 0);
        readonly Color32 ColorRed = new Color32(255, 0, 0, 0);
        readonly Color32 ColorGreen = new Color32(0, 255, 0, 0);
        readonly Color32 ColorBlue = new Color32(0, 0, 255, 0);
        readonly Color32 ColorAlpha = new Color32(0, 0, 0, 255);
        readonly List<Texture2D> SplatTextures = new List<Texture2D>();
        readonly List<List<Color32[]>> SplatColors = new List<List<Color32[]>>();
        readonly List<string> SplatPaths = new List<string>();
        readonly List<Texture> DiffTextures = new List<Texture>();
        readonly Texture2D[] BrushTextures = new Texture2D[20];
        readonly List<string> ExportTextureTypeNames = new List<string>();

        private Collider m_collider;
        private Material m_material;
        private MeshRenderer m_meshRenderer;
        private Vector2 m_scrollPosition;
        private int m_brushIndex;
        private int m_brushSize;
        private float m_brushOpacity;
        private int m_diffSelected;
        private int m_splatSelected;
        private bool m_isInDrawMode;
        private bool m_isPainting;
        private float[] m_paintedValues;
        private Color32 m_colorChannel;
        private int m_uvChannel;
        private int m_workSpaceMinX;
        private int m_workSpaceMinY;
        private int m_workSpaceMaxX;
        private int m_workSpaceMaxY;
        private int m_previewMinX;
        private int m_previewMinY;
        private int m_previewMaxX;
        private int m_previewMaxY;
        private bool m_isPreviewingBrush;
        private int m_exportResolutionIndex;
        private int m_exportTextureTypeIndex;
        private bool m_isShowingAdvanced;
        private bool m_isShowingExport;
        private bool m_isShowingAllProperties;
        private bool[] m_isShowingProperties;
        private string[,] m_propertiesNames;

        [SerializeField]
        private int m_historyIndex;

        [MenuItem("Window/Splat Painter")]
        static void Init()
        {
            SplatPainterWindow window = EditorWindow.GetWindow<SplatPainterWindow>("Splat Painter");
            window.minSize = new Vector2(WindowMinSizeX, window.minSize.y);
            window.Show();
        }

        void OnEnable()
        {
            // Load brush icons.
            for (int i = 0; i != BrushTextures.Length; ++i)
                BrushTextures[i] = (Texture2D)EditorGUIUtility.Load(string.Format("builtin_brush_{0}.png", i + 1));

            m_isInDrawMode = false;
            m_isPainting = false;
            m_diffSelected = 0;
            m_splatSelected = 0;
            m_brushIndex = 0;
            m_uvChannel = 0;
            m_brushSize = DefaultBrushSize;
            m_brushOpacity = DefaultBrushOpacity;
            m_exportResolutionIndex = 5;
            m_isShowingAllProperties = false;
            m_isShowingProperties = new bool[DefaultPropertiesNames.GetLength(0)];

            for (int i = 0; i != m_isShowingProperties.Length; ++i)
                m_isShowingProperties[i] = false;

            m_propertiesNames = new string[DefaultPropertiesNames.GetLength(0), 5];

            for (int i = 0; i != m_propertiesNames.GetLength(0); ++i)
            {
                for (int j = 0; j != m_propertiesNames.GetLength(1); ++j)
                    m_propertiesNames[i, j] = EditorPrefs.GetString(string.Format("SplatProp_{0}_{1}", i, j), DefaultPropertiesNames[i, j]);
            }

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            Undo.undoRedoPerformed -= MyUndoRedoPerformed;
            Undo.undoRedoPerformed += MyUndoRedoPerformed;
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Undo.undoRedoPerformed -= MyUndoRedoPerformed;
        }

        void OnLostFocus()
        {
            SavePropertiesNames();
        }

        void ShowHelp(string p_helpMessage)
        {
            EditorGUILayout.HelpBox(p_helpMessage, MessageType.Info);
            StopDrawing();
        }

        void SaveChange()
        {
            // Save splatmap textures.
            for (int i = 0; i != SplatTextures.Count; ++i)
            {
                byte[] pngData = SplatTextures[i].EncodeToPNG();
                File.WriteAllBytes(AssetDatabase.GetAssetPath(SplatTextures[i]), pngData);
            }

            AssetDatabase.Refresh();
        }

        void SavePropertiesNames()
        {
            for (int i = 0; i != m_propertiesNames.GetLength(0); ++i)
            {
                for (int j = 0; j != m_propertiesNames.GetLength(1); ++j)
                    EditorPrefs.SetString(string.Format("SplatProp_{0}_{1}", i, j), m_propertiesNames[i, j]);
            }
        }

        void StartDrawing()
        {
            m_isInDrawMode = true;
            m_historyIndex = 0;
            SplatPaths.Clear();
            SplatColors.Clear();

            // Change import setting of splatmap textures to make them readable.
            for (int i = 0; i != SplatTextures.Count; ++i)
            {
                SplatPaths.Add(AssetDatabase.GetAssetPath(SplatTextures[i]));
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(SplatPaths[i]);
                ti.isReadable = true;

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
                ti.textureFormat = TextureImporterFormat.RGBA32;
#else
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.sRGBTexture = false;
#endif

                AssetDatabase.ImportAsset(SplatPaths[i]);
            }

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            EditorUtility.SetSelectedWireframeHidden(m_meshRenderer, true);
#else
            EditorUtility.SetSelectedRenderState(m_meshRenderer, EditorSelectedRenderState.Hidden);
#endif
            
            // Add first history.
            List<Color32[]> firstSplatColors = new List<Color32[]>();

            for (int i = 0; i != SplatTextures.Count; ++i)
                firstSplatColors.Add(SplatTextures[i].GetPixels32());

            SplatColors.Add(firstSplatColors);
            m_isPreviewingBrush = false;
        }

        void StopDrawing()
        {
            if (m_isInDrawMode)
            {
                CancelPreview();
                SaveChange();
                m_isInDrawMode = false;
                m_historyIndex = 0;
                SplatColors.Clear();

                // Reset import setting of splatmap textures like it was before.
                for (int i = 0; i != SplatTextures.Count; ++i)
                {
                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(SplatPaths[i]);
                    ti.isReadable = false;
                    AssetDatabase.ImportAsset(SplatPaths[i]);
                }

                AssetDatabase.Refresh();
            }

            m_collider = null;

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            EditorUtility.SetSelectedWireframeHidden(m_meshRenderer, false);
#else
            EditorUtility.SetSelectedRenderState(m_meshRenderer, EditorSelectedRenderState.Highlight);
#endif
        }

        void OnGUI()
        {
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            ShowBrushProperties();
            string helpMessage = UpdateObject();
            bool isValid = helpMessage.Equals("");

            // If no valid mesh selected.
            if (!isValid)
            {
                ShowHelp(helpMessage);
            }
            else
            {
                ShowStartStopButton();
                ShowTextures();
            }

            ShowAdvanced(helpMessage);
            EditorGUILayout.EndScrollView();
        }

        void ShowBrushProperties()
        {
            // Set brush properties (Shape, Size and Opacity).
            int xCount = Mathf.FloorToInt(position.width / BrushCellSize);
            float gridHeight = ((BrushTextures.Length / xCount) + Mathf.Clamp(BrushTextures.Length % xCount, 0, 1)) * BrushCellSize;
            m_brushIndex = GUILayout.SelectionGrid(m_brushIndex, BrushTextures, xCount, GUILayout.Height(gridHeight), GUILayout.Width(position.width - 10f));
            EditorGUIUtility.labelWidth = LabelWidth;
            m_brushSize = EditorGUILayout.IntSlider("Size", m_brushSize, 1, MaxBrushSize);
            m_brushOpacity = EditorGUILayout.Slider("Opacity", m_brushOpacity, 0f, 1f);
            EditorGUIUtility.labelWidth = 0f;
        }

        void ShowStartStopButton()
        {
            GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
            bigButtonStyle.fontSize = StartStopButtonFontSize;

            // Show button for drawing (Start or Stop).
            if (!m_isInDrawMode)
            {
                GUI.color = Color.green;

                if (GUILayout.Button("Start Drawing", bigButtonStyle, GUILayout.Height(StartStopButtonHeight), GUILayout.Width(position.width - 10f)))
                    StartDrawing();

                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.red;

                if (GUILayout.Button("Stop Drawing", bigButtonStyle, GUILayout.Height(StartStopButtonHeight), GUILayout.Width(position.width - 10f)))
                    StopDrawing();

                GUI.color = Color.white;
            }
        }

        void ShowTextures()
        {
            // Show albedo textures for the user to select.
            if (DiffTextures.Count != 0)
            {
                if (m_diffSelected >= DiffTextures.Count)
                    m_diffSelected = 0;

                int xCount = Mathf.FloorToInt(position.width / DiffCellSize);
                float gridHeight = ((DiffTextures.Count / xCount) + Mathf.Clamp(DiffTextures.Count % xCount, 0, 1)) * DiffCellSize;
                m_diffSelected = GUILayout.SelectionGrid(m_diffSelected, DiffTextures.ToArray(), xCount, GUILayout.Height(gridHeight), GUILayout.Width(position.width - 10f));

                // Set color channel according to the selected albedo texture.
                for (int i = 0; i != m_propertiesNames.GetLength(0); ++i)
                {
                    for (int j = 1; j != m_propertiesNames.GetLength(1); ++j)
                    {
                        if (m_material.HasProperty(m_propertiesNames[i, j]))
                        {
                            if (DiffTextures[m_diffSelected] == AssetPreview.GetAssetPreview(m_material.GetTexture(m_propertiesNames[i, j])))
                            {
                                if (m_material.HasProperty(m_propertiesNames[i, 0]))
                                {
                                    Texture2D splatTexture = (Texture2D)m_material.GetTexture(m_propertiesNames[i, 0]);

                                    for (int k = 0; k != SplatTextures.Count; ++k)
                                    {
                                        if (SplatTextures[k] == splatTexture)
                                            m_splatSelected = k;
                                    }
                                }

                                switch (j)
                                {
                                    case 1: m_colorChannel = ColorRed; break;
                                    case 2: m_colorChannel = ColorGreen; break;
                                    case 3: m_colorChannel = ColorBlue; break;
                                    default: m_colorChannel = ColorAlpha; break;
                                }
                            }
                        }
                    }
                }
            }
        }

        void ShowAdvanced(string p_helpMessage)
        {
            EditorGUILayout.Space();
            m_isShowingAdvanced = EditorGUILayout.Foldout(m_isShowingAdvanced, "Advanced");

            if (m_isShowingAdvanced)
            {
                ++EditorGUI.indentLevel;

                ShowUVChannel();
                ShowPropertiesNames();
                ShowExport(p_helpMessage);

                --EditorGUI.indentLevel;
            }
        }

        void ShowUVChannel()
        {
            EditorGUIUtility.labelWidth = LabelWidth;
            m_uvChannel = EditorGUILayout.Popup("UV Set", m_uvChannel, ChannelUVs);
            EditorGUIUtility.labelWidth = 0f;
        }

        void ShowExport(string p_helpMessage)
        {
            EditorGUILayout.Space();
            m_isShowingExport = EditorGUILayout.Foldout(m_isShowingExport, "Export to flat texture");

            if (m_isShowingExport)
            {
                if (!p_helpMessage.Equals(""))
                {
                    EditorGUILayout.HelpBox(p_helpMessage, MessageType.Info);
                }
                else
                {
                    ++EditorGUI.indentLevel;
                    EditorGUIUtility.labelWidth = 100f;

                    ExportTextureTypeNames.Clear();
                    ExportTextureTypeNames.Add("Albedo");

                    if (m_material.HasProperty("_NormalMap01"))
                    {
                        ExportTextureTypeNames.Add("Normal Map");
                    }

                    if (m_material.HasProperty("_ParallaxMap01"))
                    {
                        ExportTextureTypeNames.Add("Height Map");
                    }

                    if (m_material.HasProperty("_Illum01"))
                    {
                        ExportTextureTypeNames.Add("Illum Map");
                    }

                    if (ExportTextureTypeNames.Count > 1)
                    {
                        m_exportTextureTypeIndex = EditorGUILayout.Popup("Texture", m_exportTextureTypeIndex, ExportTextureTypeNames.ToArray());
                    }
                    else
                    {
                        m_exportTextureTypeIndex = 0;
                    }

                    m_exportResolutionIndex = EditorGUILayout.Popup("Resolution", m_exportResolutionIndex, ResolutionsNames);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);

                    // Export button.
                    if (GUILayout.Button("Export", GUILayout.Width(100f)))
                    {
                        ExportFlatTexture();
                    }

                    GUILayout.EndHorizontal();

                    EditorGUILayout.HelpBox("It might take a minute to export.", MessageType.Info);

                    EditorGUIUtility.labelWidth = 0f;
                    --EditorGUI.indentLevel;
                }
            }
        }

        void ShowPropertiesNames()
        {
            EditorGUILayout.Space();
            m_isShowingAllProperties = EditorGUILayout.Foldout(m_isShowingAllProperties, "Shader Properties");

            if (m_isShowingAllProperties)
            {
                ++EditorGUI.indentLevel;
                EditorGUIUtility.labelWidth = 100f;

                GUILayout.BeginHorizontal();
                GUILayout.Space(40f);

                // Reset button.
                if (GUILayout.Button("Reset", GUILayout.Width(100f)))
                {
                    for (int i = 0; i != m_propertiesNames.GetLength(0); ++i)
                    {
                        for (int j = 0; j != m_propertiesNames.GetLength(1); ++j)
                            m_propertiesNames[i, j] = DefaultPropertiesNames[i, j];
                    }
                }

                GUILayout.EndHorizontal();

                // For each layer.
                for (int i = 0; i != m_isShowingProperties.Length; ++i)
                {
                    m_isShowingProperties[i] = EditorGUILayout.Foldout(m_isShowingProperties[i], string.Format("Layer {0}", i + 1));

                    if (m_isShowingProperties[i])
                    {
                        ++EditorGUI.indentLevel;
                        m_propertiesNames[i, 0] = EditorGUILayout.TextField("Splat", m_propertiesNames[i, 0]);
                        m_propertiesNames[i, 1] = EditorGUILayout.TextField("Alb (R)", m_propertiesNames[i, 1]);
                        m_propertiesNames[i, 2] = EditorGUILayout.TextField("Alb (G)", m_propertiesNames[i, 2]);
                        m_propertiesNames[i, 3] = EditorGUILayout.TextField("Alb (B)", m_propertiesNames[i, 3]);
                        m_propertiesNames[i, 4] = EditorGUILayout.TextField("Alb (A)", m_propertiesNames[i, 4]);
                        --EditorGUI.indentLevel;
                    }
                }

                EditorGUIUtility.labelWidth = 0f;
                --EditorGUI.indentLevel;
            }
        }

        void OnSelectionChange()
        {
            StopDrawing();
            UpdateObject();
            Repaint();
        }

        string UpdateObject()
        {
            // Check if GameObject is selected.
            if (Selection.activeGameObject == null)
                return "GameObject must be selected.";

            // Check if GameObject has MeshCollider.
            MeshCollider collider = Selection.activeGameObject.GetComponent<MeshCollider>();

            if (collider == null)
                collider = Selection.activeGameObject.GetComponentInChildren<MeshCollider>();

            if (collider == null)
                return "GameObject must have a MeshCollider.";

            // Check if GameObject has MeshRenderer with Material.
            m_meshRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();

            if (m_meshRenderer == null)
                m_meshRenderer = Selection.activeGameObject.GetComponentInChildren<MeshRenderer>();

            if (m_meshRenderer == null)
                return "GameObject must have a MeshRenderer.";

            if (m_meshRenderer.sharedMaterial == null)
                return "MeshRenderer must have a material.";

            m_material = m_meshRenderer.sharedMaterial;
            m_collider = collider;
            DiffTextures.Clear();
            SplatTextures.Clear();
            bool shaderIsValid = false;

            // Check if material has valid shader.
            for (int i = 0; i != m_propertiesNames.GetLength(0); ++i)
            {
                // Check if SplatMapXX Property exists.
                if (m_material.HasProperty(m_propertiesNames[i, 0]))
                {
                    shaderIsValid = true;
                    Texture splatTex = m_material.GetTexture(m_propertiesNames[i, 0]);

                    // Check if SplatMapXX Texture is assigned.
                    if (splatTex != null)
                    {
                        SplatTextures.Add((Texture2D)splatTex);

                        for (int j = 1; j != m_propertiesNames.GetLength(1); ++j)
                        {
                            // Check if AlbedoXX Property exists.
                            if (m_material.HasProperty(m_propertiesNames[i, j]))
                            {
                                Texture tex = m_material.GetTexture(m_propertiesNames[i, j]);

                                // Check if AlbedoXX Texture is assigned.
                                if (tex != null)
                                    DiffTextures.Add(AssetPreview.GetAssetPreview(tex));
                            }
                        }
                    }
                }
            }

            if (!shaderIsValid)
                return "Material must have a valid shader.";

            if (DiffTextures.Count == 0)
                return "Material must have textures assigned.";

            return string.Empty;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            // If mouse is released while painting, then stop painting.
            if (m_isPainting && Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
                m_isPainting = false;

            // If in drawing mode.
            if (m_isInDrawMode && m_collider != null)
            {
                // Disable Selection.
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                // Do a raycast from cursor.
                RaycastHit hit;
                Ray ray = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x,
                                                                      (float)Camera.current.pixelHeight - Event.current.mousePosition.y,
                                                                      0f));
                // If cursor is on selected mesh.
                if (m_collider.Raycast(ray, out hit, RayLength))
                {
                    int width = SplatTextures[0].width;
                    int height = SplatTextures[0].height;

                    Vector2 textCoord = m_uvChannel == 0 ? hit.textureCoord : hit.textureCoord2;

                    // Get UV.
                    int u = (int)((float)(width - 1) * Mathf.Clamp01(textCoord.x));
                    int v = (int)((float)(height - 1) * Mathf.Clamp01(textCoord.y));

                    bool isPreview = true;
                    
					// If holding left click.
					if (m_isPainting && !Event.current.alt)
                    {
                        isPreview = false;
                        EditTexture(u, v, false);
                    }

                    // If first left click.
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
                    {
                        isPreview = false;
                        Undo.RecordObject(this, "Paint");
                        m_isPainting = true;
                        m_paintedValues = new float[width * height];
                        ++m_historyIndex;

                        // Erase the future.
                        if (m_historyIndex != SplatColors.Count)
                            SplatColors.RemoveRange(m_historyIndex, SplatColors.Count - m_historyIndex);

                        // Add new future.
                        List<Color32[]> newSplatColors = new List<Color32[]>();

                        for (int i = 0; i != SplatTextures.Count; ++i)
                            newSplatColors.Add(SplatTextures[i].GetPixels32());

                        SplatColors.Add(newSplatColors);

                        m_workSpaceMinX = int.MaxValue;
                        m_workSpaceMinY = int.MaxValue;
                        m_workSpaceMaxX = 0;
                        m_workSpaceMaxY = 0;
                        EditTexture(u, v, false);
                    }

                    if (isPreview && !Event.current.alt)
                    {
                        m_paintedValues = new float[width * height];
                        m_workSpaceMinX = int.MaxValue;
                        m_workSpaceMinY = int.MaxValue;
                        m_workSpaceMaxX = 0;
                        m_workSpaceMaxY = 0;

                        List<Color32[]> newSplatColors = new List<Color32[]>();

                        for (int i = 0; i != SplatTextures.Count; ++i)
                        {
                            newSplatColors.Add((Color32[])(SplatColors[m_historyIndex][i].Clone()));
                        }
                            

                        ++m_historyIndex;
                        SplatColors.Insert(m_historyIndex, newSplatColors);

                        CancelPreview();
                        EditTexture(u, v, true);

                        SplatColors.RemoveAt(m_historyIndex);
                        --m_historyIndex;
                    }
                }
                else
                {
                    CancelPreview();
                }
            }
        }

        void CancelPreview()
        {
            if (m_isPreviewingBrush)
            {
                int width = SplatTextures[0].width;

                for (int i = 0; i != SplatTextures.Count; ++i)
                {
                    Color32[] pixelsToApply = new Color32[(m_previewMaxX - m_previewMinX + 1) * (m_previewMaxY - m_previewMinY + 1)];
                    int index = 0;

                    for (int y = m_previewMinY; y != m_previewMaxY + 1; ++y)
                    {
                        for (int x = m_previewMinX; x != m_previewMaxX + 1; ++x)
                        {
                            pixelsToApply[index] = SplatColors[m_historyIndex][i][y * width + x];
                            ++index;
                        }
                    }

                    SplatTextures[i].SetPixels32(m_previewMinX, m_previewMinY, m_previewMaxX - m_previewMinX + 1, m_previewMaxY - m_previewMinY + 1, pixelsToApply);
                    SplatTextures[i].Apply();
                }

                m_isPreviewingBrush = false;
            }
        }

        void EditTexture(int p_x, int p_y, bool p_preview)
        {
            int width = SplatTextures[0].width;
            int minXNotClamped = p_x - ((m_brushSize - 1) / 2);
            int minYNotClamped = p_y - ((m_brushSize - 1) / 2);
            int minX = Mathf.Max(0, minXNotClamped);
            int minY = Mathf.Max(0, minYNotClamped);
            int maxX = Mathf.Min(SplatTextures[0].width - 1,  minX + (m_brushSize - 1));
            int maxY = Mathf.Min(SplatTextures[0].height - 1, minY + (m_brushSize - 1));
            int index = 0;
            float paintedValue;
            Color32 oldColor;
            m_workSpaceMinX = Mathf.Min(m_workSpaceMinX, minX);
            m_workSpaceMinY = Mathf.Min(m_workSpaceMinY, minY);
            m_workSpaceMaxX = Mathf.Max(m_workSpaceMaxX, maxX);
            m_workSpaceMaxY = Mathf.Max(m_workSpaceMaxY, maxY);

            if (p_preview)
            {
                m_previewMinX = minX;
                m_previewMinY = minY;
                m_previewMaxX = maxX;
                m_previewMaxY = maxY;
                m_isPreviewingBrush = true;
            }
            
            // Foreach pixel inside the brush zone.
            for (int y = minY; y != maxY + 1; ++y)
            {
                for (int x = minX; x != maxX + 1; ++x)
                {
                    index = y * width + x;
                    m_paintedValues[index] = Mathf.Max(m_paintedValues[index],
                                                    BrushTextures[m_brushIndex].GetPixelBilinear((float)(x - minXNotClamped + 1) / (float)(m_brushSize + 1),
                                                                                                (float)(y - minYNotClamped + 1) / (float)(m_brushSize + 1)).a * m_brushOpacity);
                }
            }
            
            // Foreach pixel in the splatmap.
            for (int y = m_workSpaceMinY; y != m_workSpaceMaxY + 1; ++y)
            {
                for (int x = m_workSpaceMinX; x != m_workSpaceMaxX + 1; ++x)
                {
                    index = y * width + x;
                    paintedValue = m_paintedValues[index];

                    // Set pixel color.
                    for (int i = 0; i != SplatTextures.Count; ++i)
                    {
                        oldColor = SplatColors[m_historyIndex - 1][i][index];

                        if (paintedValue < 0.01f)
                        {
                            SplatColors[m_historyIndex][i][index] = oldColor;
                        }
                        else if (paintedValue > 0.99f)
                        {
                            if (i == m_splatSelected)
                                SplatColors[m_historyIndex][i][index] = m_colorChannel;
                            else
                                SplatColors[m_historyIndex][i][index] = ColorBlack;
                        }
                        else
                        {
                            if (i == m_splatSelected)
                            {
                                SplatColors[m_historyIndex][i][index] = new Color32((byte)((int)oldColor.r + (int)(paintedValue * ((int)m_colorChannel.r - (int)oldColor.r))),
                                                                        (byte)((int)oldColor.g + (int)(paintedValue * ((int)m_colorChannel.g - (int)oldColor.g))),
                                                                        (byte)((int)oldColor.b + (int)(paintedValue * ((int)m_colorChannel.b - (int)oldColor.b))),
                                                                        (byte)((int)oldColor.a + (int)(paintedValue * ((int)m_colorChannel.a - (int)oldColor.a))));
                            }
                            else
                            {
                                SplatColors[m_historyIndex][i][index] = new Color32((byte)((int)oldColor.r + (int)(paintedValue * -(int)oldColor.r)),
                                                                       (byte)((int)oldColor.g + (int)(paintedValue * -(int)oldColor.g)),
                                                                       (byte)((int)oldColor.b + (int)(paintedValue * -(int)oldColor.b)),
                                                                       (byte)((int)oldColor.a + (int)(paintedValue * -(int)oldColor.a)));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i != SplatTextures.Count; ++i)
            {
                Color32[] pixelsToApply = new Color32[(maxX - minX + 1) * (maxY - minY + 1)];
                index = 0;

                for (int y = minY; y != maxY + 1; ++y)
                {
                    for (int x = minX; x != maxX + 1; ++x)
                    {
                        pixelsToApply[index] = SplatColors[m_historyIndex][i][y * width + x];
                        ++index;
                    }
                }
                
                SplatTextures[i].SetPixels32(minX, minY, maxX - minX + 1, maxY - minY + 1, pixelsToApply);
                SplatTextures[i].Apply();
            }
        }

        void MyUndoRedoPerformed()
        {
            if (m_isInDrawMode)
            {
                for (int i = 0; i != SplatTextures.Count; ++i)
                {
                    Color32[] pixelsToApply = SplatColors[m_historyIndex][i];
                    SplatTextures[i].SetPixels32(pixelsToApply);
                    SplatTextures[i].Apply();
                }
            }
        }

        void ExportFlatTexture()
        {
            int resolution = Resolutions[m_exportResolutionIndex];
            Color32[] flatColors = new Color32[resolution * resolution];
            float sizePixel = 1f / resolution;
            List<Texture2D> textures2D = new List<Texture2D>();
            List<bool> isReadables = new List<bool>();
            List<Vector2> scales = new List<Vector2>();
            List<Vector2> offsets = new List<Vector2>();
            List<Color> tintColors = new List<Color>();
            int flatColorIndex = 0;
            string[,] propertiesName = m_propertiesNames;
            bool hasTintColor = m_exportTextureTypeIndex == 0 && m_material.HasProperty("_TintColor01");
            bool isNormalMap = m_exportTextureTypeIndex == 1;

            if (m_exportTextureTypeIndex == 1)
                propertiesName = NormalMapNames;
            else if (m_exportTextureTypeIndex == 2)
                propertiesName = HeightMapNames;
            else if (m_exportTextureTypeIndex == 3)
                propertiesName = IllumNames;

            // Fill Texture2D List (and enable isReadable).
            for (int splatIndex = 0; splatIndex != SplatTextures.Count; ++splatIndex)
            {
                for (int texIndex = 0; texIndex != 5; ++texIndex)
                {
                    Texture2D tex2d = null;
                    bool isReadable = false;
                    Color tintColor = Color.white;
                    Vector2 scale = Vector2.one;
                    Vector2 offset = Vector2.zero;
                    string propertyName = propertiesName[splatIndex, texIndex];

                    if (hasTintColor && texIndex != 0)
                    {
                        tintColor = m_material.GetColor(TintPropertiesNames[splatIndex, texIndex]);
                    }

                    if (m_material.HasProperty(propertyName))
                    {
                        tex2d = (Texture2D)m_material.GetTexture(propertyName);

                        if (tex2d != null)
                        {
                            scale = m_material.GetTextureScale(propertyName);
                            offset = m_material.GetTextureOffset(propertyName);
                            string texPath = AssetDatabase.GetAssetPath(tex2d);
                            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texPath);
                            isReadable = importer.isReadable;
                            importer.isReadable = true;
                            AssetDatabase.ImportAsset(texPath);
                        }
                    }

                    tintColors.Add(tintColor);
                    isReadables.Add(isReadable);
                    textures2D.Add(tex2d);
                    offsets.Add(offset);
                    scales.Add(scale);
                }  
            }

            // Set flat colors.
            for (int y = 0; y != resolution; ++y)
            {
                for (int x = 0; x != resolution; ++x)
                {
                    Color finalColor = Color.black;

                    for (int splatIndex = 0; splatIndex != SplatTextures.Count; ++splatIndex)
                    {
                        float u = (float)x * sizePixel;
                        float v = (float)y * sizePixel;
                        int index00 = splatIndex * 5;
                        int index01 = index00 + 1;
                        int index02 = index00 + 2;
                        int index03 = index00 + 3;
                        int index04 = index00 + 4;

                        Color splatColor = GetPixelColor(u, v, scales[index00], offsets[index00], textures2D[index00], false);
                        Color tex01Color = GetPixelColor(u, v, scales[index01], offsets[index01], textures2D[index01], isNormalMap);
                        Color tex02Color = GetPixelColor(u, v, scales[index02], offsets[index02], textures2D[index02], isNormalMap);
                        Color tex03Color = GetPixelColor(u, v, scales[index03], offsets[index03], textures2D[index03], isNormalMap);
                        Color tex04Color = GetPixelColor(u, v, scales[index04], offsets[index04], textures2D[index04], isNormalMap);

                        if (hasTintColor)
                        {
                            finalColor += splatColor.r * tex01Color * tintColors[index01]
                                    + splatColor.g * tex02Color * tintColors[index02]
                                    + splatColor.b * tex03Color * tintColors[index03]
                                    + splatColor.a * tex04Color * tintColors[index04];
                        }
                        else
                        {
                            finalColor += splatColor.r * tex01Color
                                    + splatColor.g * tex02Color
                                    + splatColor.b * tex03Color
                                    + splatColor.a * tex04Color;
                        }
                    }

                    flatColors[flatColorIndex] = finalColor;
                    ++flatColorIndex;
                }
            }

            // Reset isReadable properties.
            for (int tex2DIndex = 0; tex2DIndex != textures2D.Count; ++tex2DIndex)
            {
                if (textures2D[tex2DIndex] != null)
                {
                    string texPath = AssetDatabase.GetAssetPath(textures2D[tex2DIndex]);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texPath);
                    importer.isReadable = isReadables[tex2DIndex];
                    AssetDatabase.ImportAsset(texPath);
                }
            }

            // Export texture.
            string path = EditorUtility.SaveFilePanelInProject("Save texture", "New Flat Texture", "png", "Please enter a file name to save the texture to");

            if (!path.Equals(""))
            {
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path);
                Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
                tex.SetPixels32(flatColors);
                byte[] pngData = tex.EncodeToPNG();
                File.WriteAllBytes(assetPathAndName, pngData);
                Object.DestroyImmediate(tex);
                AssetDatabase.Refresh();
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(assetPathAndName);
                ti.wrapMode = TextureWrapMode.Clamp;

                if (isNormalMap)
                {
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
                    ti.normalmap = true;
#else
                    ti.textureType = TextureImporterType.NormalMap;
#endif
                }

                AssetDatabase.ImportAsset(assetPathAndName);
                AssetDatabase.Refresh();
            }
        }

        private Color GetPixelColor(float p_u, float p_v, Vector2 p_scale, Vector2 p_offset, Texture2D p_tex2D, bool p_isNormalMap)
        {
            if (p_tex2D == null)
            {
                if (p_isNormalMap)
                    return new Color(0.5f, 0.5f, 1f);
                else
                    return Color.black;
            }
            else
            {
                p_u *= p_scale.x;
                p_v *= p_scale.y;

                p_u += p_offset.x;
                p_v += p_offset.y;

                return p_tex2D.GetPixelBilinear(p_u, p_v);
            }
        }
    }
}
