#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Data;
using Game.LevelEditor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Level Edit Controller için gereken arayüzü sağlar. 
    /// </summary>
    [CustomEditor(typeof(LevelEditorController))]
    public class LevelEditorControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _levelDatabaseProp;
        private SerializedProperty _targetLevelDataProp;
        private SerializedProperty _prefabRegistryProp;

        private SerializedProperty _gridRootProp;
        private SerializedProperty _queueRootProp;

        private SerializedProperty _gridSizeProp;
        private SerializedProperty _queueSlotCountProp;

        private SerializedProperty _timeLimitProp;
        private SerializedProperty _busesProp;
        
        private SerializedProperty _busPreviewRootProp;
        private SerializedProperty _busPreviewSpawnPointProp;
        
        private SerializedProperty _passengerOffsetProp;
        private SerializedProperty _obstacleOffsetProp;

        private SerializedProperty _paintModeEnabledProp;
        private SerializedProperty _paintModeProp;
        private SerializedProperty _paintPassengerColorProp;

        private bool _showGuide = true;

        private void OnEnable()
        {
            _levelDatabaseProp = serializedObject.FindProperty("levelDatabase");
            _targetLevelDataProp = serializedObject.FindProperty("targetLevelData");
            _prefabRegistryProp = serializedObject.FindProperty("prefabRegistry");

            _gridRootProp = serializedObject.FindProperty("gridRoot");
            _queueRootProp = serializedObject.FindProperty("queueRoot");

            _gridSizeProp = serializedObject.FindProperty("gridSize");
            _queueSlotCountProp = serializedObject.FindProperty("queueSlotCount");

            _timeLimitProp = serializedObject.FindProperty("timeLimit");
            _busesProp = serializedObject.FindProperty("buses");
            
            _busPreviewRootProp = serializedObject.FindProperty("busPreviewRoot");
            _busPreviewSpawnPointProp = serializedObject.FindProperty("busPreviewSpawnPoint");
            
            _passengerOffsetProp = serializedObject.FindProperty("passengerOffset");
            _obstacleOffsetProp = serializedObject.FindProperty("obstacleOffset");

            _paintModeEnabledProp = serializedObject.FindProperty("paintModeEnabled");
            _paintModeProp = serializedObject.FindProperty("paintMode");
            _paintPassengerColorProp = serializedObject.FindProperty("paintPassengerColor");

            SceneView.duringSceneGui += DuringSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
        

        #region Draw Layout
        private void DrawGuideSection()
        {
            _showGuide = EditorGUILayout.Foldout(_showGuide, "Quick Guide", true);

            if (!_showGuide)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "1) Referansları bağla.\n" +
                "2) Açılır listeden bir level seç ya da yeni bir level oluştur.\n" +
                "3) Grid Size ile leveldeki Grid boyutunu belirle.\n" +
                "4) Queue Slot Count ile kuyruk sırasını belirle(Default 5).\n" +
                "5) Paint Mode'u aç, Passenger / Obstacle / Empty seç.\n" +
                "6) Sahnede grid cell'lere sol tıklayarak leveli boya.\n" +
                "7) Time Limit ve Buses listesini doldur.\n" +
                "8) BAKE SELECTED LEVEL butonu ile seviyeyi kaydet.\n +" +
                "ÖNEMLİ!!!!: Clear All Cells griddeki bütün Passenger, Obstacle gibi entityleri yok eder! Dikkatli kullan"
                ,
                MessageType.Info);
        }

        private void DrawDataSection(LevelEditorController controller)
        {
            EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_levelDatabaseProp);
            EditorGUILayout.PropertyField(_prefabRegistryProp);

            LevelDatabase database = _levelDatabaseProp.objectReferenceValue as LevelDatabase;
            LevelData currentLevel = _targetLevelDataProp.objectReferenceValue as LevelData;

            if (database == null)
            {
                EditorGUILayout.HelpBox("Assign LevelDatabase first.", MessageType.Warning);
                return;
            }

            List<LevelData> levels = database.GetOrderedLevels();
            List<string> options = new();

            int currentIndex = -1;

            for (int i = 0; i < levels.Count; i++)
            {
                options.Add($"Level {levels[i].levelNumber:000} ({levels[i].name})");
                if (levels[i] == currentLevel)
                {
                    currentIndex = i;
                }
            }

            options.Add("Create New Level...");

            int popupIndex = currentIndex >= 0 ? currentIndex : 0;
            if (levels.Count == 0)
            {
                popupIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup("Editing Level", popupIndex, options.ToArray());

            if (newIndex == options.Count - 1)
            {
                CreateNewLevel(database, controller);
                serializedObject.Update();
                return;
            }

            if (levels.Count > 0 && newIndex >= 0 && newIndex < levels.Count)
            {
                if (_targetLevelDataProp.objectReferenceValue != levels[newIndex])
                {
                    _targetLevelDataProp.objectReferenceValue = levels[newIndex];
                    serializedObject.ApplyModifiedProperties();

                    controller.LoadSelectedLevelToEditor();
                    EditorUtility.SetDirty(controller);
                }
            }

            EditorGUILayout.PropertyField(_targetLevelDataProp, new GUIContent("Selected Level Asset"));
        }

        private void DrawRootsSection()
        {
            EditorGUILayout.LabelField("Scene Roots", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_gridRootProp);
            EditorGUILayout.PropertyField(_queueRootProp);
        }

        private void DrawLayoutSection()
        {
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_gridSizeProp);
            EditorGUILayout.PropertyField(_queueSlotCountProp);
        }

        private void DrawLevelSettingsSection()
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_timeLimitProp);
            EditorGUILayout.PropertyField(_busesProp, true);
            EditorGUILayout.PropertyField(_busPreviewRootProp);
            EditorGUILayout.PropertyField(_busPreviewSpawnPointProp);
        }

        private void DrawOffsetsSection()
        {
            EditorGUILayout.LabelField("Preview / Spawn Offsets", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_passengerOffsetProp);
            EditorGUILayout.PropertyField(_obstacleOffsetProp);

            EditorGUILayout.HelpBox(
                "Burası opsiyoneldir. Olası bir entitylerin Tile'a tam şekilde snaplememesi durumunda offset vererek ayarlanabilir.",
                MessageType.None);
        }

        private void DrawPaintSection()
        {
            EditorGUILayout.LabelField("Paint Tool", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_paintModeEnabledProp, new GUIContent("Enable Paint Mode"));
            EditorGUILayout.PropertyField(_paintModeProp);

            CellPaintMode mode = (CellPaintMode)_paintModeProp.enumValueIndex;
            if (mode == CellPaintMode.Passenger)
            {
                EditorGUILayout.PropertyField(_paintPassengerColorProp);
            }

            if (_paintModeEnabledProp.boolValue)
            {
                EditorGUILayout.HelpBox("Scene görünümünde grid cell'lere sol tıklayarak boya.", MessageType.Info);
            }
        }

        private void DrawActionButtons(LevelEditorController controller)
        {
            EditorGUILayout.Space(4f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Selected Level", GUILayout.Height(28f)))
            {
                controller.LoadSelectedLevelToEditor();
            }

            if (GUILayout.Button("Clear All Cells", GUILayout.Height(28f)))
            {
                controller.ClearAllCells();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6f);

            if (GUILayout.Button("BAKE SELECTED LEVEL", GUILayout.Height(42f)))
            {
                controller.BakeSelectedLevel();
            }
        }
        
        #endregion
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LevelEditorController controller = (LevelEditorController)target;

            DrawGuideSection();
            EditorGUILayout.Space(8f);

            DrawDataSection(controller);
            EditorGUILayout.Space(8f);

            DrawRootsSection();
            EditorGUILayout.Space(8f);

            EditorGUI.BeginChangeCheck();

            DrawLayoutSection();
            EditorGUILayout.Space(8f);

            DrawLevelSettingsSection();
            EditorGUILayout.Space(8f);

            DrawOffsetsSection();
            EditorGUILayout.Space(8f);

            DrawPaintSection();
            EditorGUILayout.Space(10f);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                controller.SyncEditorVisuals();
                EditorUtility.SetDirty(controller);
            }

            DrawActionButtons(controller);
        }

        //Paint Mode'da istediğimiz cell'e tıklayarak Passenger veya Obstacle ekleyebilmemizi sağlar
        private void DuringSceneGUI(SceneView sceneView)
        {
            if (target == null)
            {
                return;
            }

            LevelEditorController controller = (LevelEditorController)target;
            if (!controller.PaintModeEnabled)
            {
                return;
            }

            Event currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0 || currentEvent.alt)
            {
                return;
            }

            GameObject pickedObject = HandleUtility.PickGameObject(currentEvent.mousePosition, false);

            if (pickedObject == null)
            {
                return;
            }

            LevelCell levelCell = pickedObject.GetComponentInParent<LevelCell>();
            if (levelCell == null)
            {
                return;
            }

            Undo.RecordObject(levelCell, "Paint Level Cell");
            controller.ApplyBrushToCell(levelCell);
            EditorUtility.SetDirty(levelCell);

            currentEvent.Use();
        }
        
        
        //Database içerisinde yeni bir Level oluşturup, asset olarak Database içerisine ekler.
        private void CreateNewLevel(LevelDatabase database, LevelEditorController controller)
        {
            int nextLevelNumber = database.GetNextLevelNumber();
            string defaultName = $"Level_{nextLevelNumber:000}";
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Level",
                defaultName,
                "asset",
                "Select a location for the new LevelData asset.");

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            LevelData newLevel = CreateInstance<LevelData>();
            newLevel.levelNumber = nextLevelNumber;
            newLevel.gridSize = controller.GridSize;
            newLevel.cellSize = 1f;
            newLevel.timeLimit = controller.TimeLimit;

            AssetDatabase.CreateAsset(newLevel, path);
            AssetDatabase.SaveAssets();

            database.AddLevel(newLevel);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            controller.SetTargetLevelData(newLevel);
            controller.ClearAllCells();
            controller.SyncEditorVisuals();

            serializedObject.Update();
            serializedObject.FindProperty("targetLevelData").objectReferenceValue = newLevel;
            serializedObject.ApplyModifiedProperties();

            Selection.activeObject = newLevel;
        }
    }
}
#endif