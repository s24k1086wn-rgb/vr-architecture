using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SafetyTraining.Editor
{
    /// <summary>
    /// Professional Editor Tool for procedurally generating construction site layouts.
    /// Integrates seamlessly with prefab systems and provides full undo/redo functionality.
    /// </summary>
    public class ConstructionSiteGeneratorWindow : EditorWindow
    {
        private enum PlacementMode
        {
            Random,
            Grid
        }

        [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();
        private PlacementMode placementMode = PlacementMode.Random;
        
        // Random placement parameters
        private int spawnCount = 20;
        private Vector2 areaSize = new Vector2(30f, 30f);
        private Vector3 centerPoint = Vector3.zero;

        // Grid placement parameters
        private Vector2 gridInterval = new Vector2(4f, 4f);
        private bool skipCellsRandomly = true;
        [Range(0f, 100f)] private float skipProbability = 30f;

        // Common alignment options
        private float heightOffset = 0.0f;
        private bool alignToGround = true;
        private int groundLayerIndex = 0; // Default to layer 0 (Default)
        private bool randomRotationY = true;
        private Transform parentGroupObject;

        private Vector2 scrollPosition;

        [MenuItem("Tools/Construction Site/Site Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConstructionSiteGeneratorWindow>("Site Generator");
            window.minSize = new Vector2(350, 450);
            window.Show();
        }

        private void OnGUI()
        {
            // Title Header
            EditorGUILayout.BeginVertical("box");
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label("Construction Site Layout Generator", titleStyle);
            EditorGUILayout.LabelField("Procedurally populate assets (scaffolding, barricades, cones) cleanly.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 1. Asset Pool Section
            DrawPrefabListSection();
            EditorGUILayout.Space(10);

            // 2. Spawn Options Configuration
            DrawSpawnConfigSection();
            EditorGUILayout.Space(10);

            // 3. Execution / Controls
            DrawExecutionSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPrefabListSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("1. Asset Pool (Register Prefabs)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Register prefabs (e.g. scaffolding, fences, cones) to randomly select from.", MessageType.Info);

            int listSize = prefabsToSpawn.Count;
            int newListSize = EditorGUILayout.IntField("Prefab Varieties", listSize);
            
            if (newListSize < 0) newListSize = 0;
            while (prefabsToSpawn.Count < newListSize) prefabsToSpawn.Add(null);
            while (prefabsToSpawn.Count > newListSize) prefabsToSpawn.RemoveAt(prefabsToSpawn.Count - 1);

            EditorGUI.indentLevel++;
            for (int i = 0; i < prefabsToSpawn.Count; i++)
            {
                prefabsToSpawn[i] = (GameObject)EditorGUILayout.ObjectField($"Variety {i + 1}", prefabsToSpawn[i], typeof(GameObject), false);
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Clear All Registered Prefabs", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Clear List?", "Are you sure you want to clear the registered prefabs?", "Yes", "No"))
                {
                    prefabsToSpawn.Clear();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSpawnConfigSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("2. Spawning Parameters", EditorStyles.boldLabel);

            placementMode = (PlacementMode)EditorGUILayout.EnumPopup("Spawning Mode", placementMode);
            centerPoint = EditorGUILayout.Vector3Field("Spawn Area Center", centerPoint);
            areaSize = EditorGUILayout.Vector2Field("Bound Dimensions (X, Z)", areaSize);

            if (placementMode == PlacementMode.Random)
            {
                spawnCount = EditorGUILayout.IntField("Count to Spawn", spawnCount);
                if (spawnCount < 1) spawnCount = 1;
            }
            else if (placementMode == PlacementMode.Grid)
            {
                gridInterval = EditorGUILayout.Vector2Field("Grid Spacing (X, Z)", gridInterval);
                if (gridInterval.x <= 0.1f) gridInterval.x = 0.1f;
                if (gridInterval.y <= 0.1f) gridInterval.y = 0.1f;

                skipCellsRandomly = EditorGUILayout.Toggle("Random Pattern Skip", skipCellsRandomly);
                if (skipCellsRandomly)
                {
                    skipProbability = EditorGUILayout.Slider("Skip Chance (%)", skipProbability, 0f, 100f);
                }
            }

            EditorGUILayout.Space(5);
            GUILayout.Label("Alignments & Grouping", EditorStyles.miniBoldLabel);
            
            heightOffset = EditorGUILayout.FloatField("Spawn Height Offset (Y)", heightOffset);
            alignToGround = EditorGUILayout.Toggle("Raycast to Ground", alignToGround);
            if (alignToGround)
            {
                groundLayerIndex = EditorGUILayout.LayerField("Ground Layer", groundLayerIndex);
            }
            randomRotationY = EditorGUILayout.Toggle("Random Y-axis Rotation", randomRotationY);
            parentGroupObject = (Transform)EditorGUILayout.ObjectField("Group Under Object", parentGroupObject, typeof(Transform), true);

            EditorGUILayout.EndVertical();
        }

        private void DrawExecutionSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("3. Action Hub", EditorStyles.boldLabel);

            bool hasValidPrefabs = false;
            foreach (var p in prefabsToSpawn)
            {
                if (p != null) { hasValidPrefabs = true; break; }
            }

            if (!hasValidPrefabs)
            {
                EditorGUILayout.HelpBox("Add at least one Prefab asset to Section 1 to enable generation.", MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(!hasValidPrefabs);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Layout Procedurally", GUILayout.Height(35)))
            {
                GenerateLayout();
            }
            GUI.backgroundColor = Color.white;

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Clear Generated Layout Assets"))
            {
                ClearGenerated();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void GenerateLayout()
        {
            List<GameObject> validPrefabs = new List<GameObject>();
            foreach (var p in prefabsToSpawn)
            {
                if (p != null) validPrefabs.Add(p);
            }

            if (validPrefabs.Count == 0) return;

            // Register undo group for single-operation rollback
            Undo.SetCurrentGroupName("Procedural Construction Site Generation");
            int undoGroupIndex = Undo.GetCurrentGroup();

            // Set up or search for group container
            Transform activeParent = parentGroupObject;
            if (activeParent == null)
            {
                GameObject newParent = GameObject.Find("Procedural_Layout_Group");
                if (newParent == null)
                {
                    newParent = new GameObject("Procedural_Layout_Group");
                    Undo.RegisterCreatedObjectUndo(newParent, "Create Layout Parent");
                }
                activeParent = newParent.transform;
            }

            int spawnTries = 0;

            if (placementMode == PlacementMode.Random)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                    
                    float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f) + centerPoint.x;
                    float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f) + centerPoint.z;
                    Vector3 startRayPos = new Vector3(rx, centerPoint.y + 100f, rz); // High above to guarantee hit

                    SpawnAndPlaceObject(prefab, startRayPos, activeParent);
                    spawnTries++;
                }
            }
            else if (placementMode == PlacementMode.Grid)
            {
                int cols = Mathf.FloorToInt(areaSize.x / gridInterval.x);
                int rows = Mathf.FloorToInt(areaSize.y / gridInterval.y);

                float startX = centerPoint.x - (areaSize.x / 2f);
                float startZ = centerPoint.z - (areaSize.y / 2f);

                for (int c = 0; c <= cols; c++)
                {
                    for (int r = 0; r <= rows; r++)
                    {
                        if (skipCellsRandomly && Random.Range(0f, 100f) < skipProbability)
                        {
                            continue;
                        }

                        GameObject prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                        
                        float px = startX + (c * gridInterval.x);
                        float pz = startZ + (r * gridInterval.y);
                        Vector3 startRayPos = new Vector3(px, centerPoint.y + 100f, pz);

                        SpawnAndPlaceObject(prefab, startRayPos, activeParent);
                        spawnTries++;
                    }
                }
            }

            Undo.CollapseUndoOperations(undoGroupIndex);
            Debug.Log($"[Site Generator] Successfully instantiated {spawnTries} prefabs as live links.");
        }

        private void SpawnAndPlaceObject(GameObject prefab, Vector3 startRayPos, Transform parent)
        {
            Vector3 finalPos = startRayPos;
            finalPos.y = centerPoint.y + heightOffset; // Default flat fallback

            if (alignToGround)
            {
                Ray ray = new Ray(startRayPos, Vector3.down);
                int mask = 1 << groundLayerIndex;
                if (Physics.Raycast(ray, out RaycastHit hit, 150f, mask))
                {
                    finalPos.y = hit.point.y + heightOffset;
                }
                else
                {
                    // Fallback to absolute center plane elevation if ray misses
                    finalPos.y = centerPoint.y + heightOffset;
                }
            }

            // Using PrefabUtility to preserve nested prefab connections correctly in the editor
            GameObject spawnedInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (spawnedInstance != null)
            {
                spawnedInstance.transform.position = finalPos;
                spawnedInstance.transform.SetParent(parent);

                // Angle application
                float yAngle = randomRotationY ? Random.Range(0f, 360f) : 0f;
                spawnedInstance.transform.rotation = Quaternion.Euler(0f, yAngle, 0f);

                Undo.RegisterCreatedObjectUndo(spawnedInstance, "Procedural Asset Placed");
            }
        }

        private void ClearGenerated()
        {
            if (parentGroupObject != null)
            {
                int count = parentGroupObject.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(parentGroupObject.GetChild(i).gameObject);
                }
                Debug.Log($"[Site Generator] Cleared {count} instances from registered Parent container.");
            }
            else
            {
                GameObject defaultGroup = GameObject.Find("Procedural_Layout_Group");
                if (defaultGroup != null)
                {
                    int count = defaultGroup.transform.childCount;
                    Undo.DestroyObjectImmediate(defaultGroup);
                    Debug.Log($"[Site Generator] Destroyed 'Procedural_Layout_Group' along with {count} procedurally placed children.");
                }
                else
                {
                    EditorUtility.DisplayDialog("No Parent Group", "No 'Procedural_Layout_Group' or explicitly registered Parent Group was found in the hierarchy to clear.", "OK");
                }
            }
        }
    }
}
