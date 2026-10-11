using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Untitled.Editor
{
    [CustomEditor(typeof(CaveMap))]
    public sealed class CaveMapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var map = (CaveMap)target;
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (!map.HasEditableLayout)
                {
                    EditorGUILayout.HelpBox("Convert the import baseline once. Scene Tilemaps become the saved layout.", MessageType.Info);
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("layout"), new GUIContent("Import Baseline"));
                    serializedObject.ApplyModifiedProperties();
                    if (GUILayout.Button("Create Scene Tilemaps From Baseline")) CreateSceneLayout(map);
                    return;
                }

                EditorGUILayout.HelpBox("Paint Floor / River using CavePalette (cell Z = 0). River needs floor beneath it and remains walkable. Save synchronizes 3D meshes and colliders.", MessageType.Info);
                try { CaveMapBuilder.Validate(map); }
                catch (InvalidOperationException error) { EditorGUILayout.HelpBox(error.Message, MessageType.Error); }
                var floorRenderer = map.FloorTilemap.GetComponent<TilemapRenderer>();
                var riverRenderer = map.RiverTilemap.GetComponent<TilemapRenderer>();
                bool showGuides = EditorGUILayout.Toggle("Show Tilemap Guides", floorRenderer.enabled);
                if (showGuides != floorRenderer.enabled)
                {
                    Undo.RecordObjects(new UnityEngine.Object[] { floorRenderer, riverRenderer }, "Toggle layout guides");
                    floorRenderer.enabled = riverRenderer.enabled = showGuides;
                    EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
                }
                if (GUILayout.Button("Edit Floor Tilemap")) Selection.activeGameObject = map.FloorTilemap.gameObject;
                if (GUILayout.Button("Edit River Tilemap")) Selection.activeGameObject = map.RiverTilemap.gameObject;
                if (GUILayout.Button("Select Spawn Point")) Selection.activeGameObject = map.SpawnPoint.gameObject;
                if (GUILayout.Button("Rebuild 3D Map From Scene Tilemaps")) CaveMapBuilder.Rebuild(map);
            }
        }

        [MenuItem("Untitled/Cave/Rebuild Scene Geometry")]
        public static void RebuildSceneGeometry()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var maps = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<CaveMap>(true)).ToArray();
            if (maps.Length != 1) throw new InvalidOperationException("Open a scene with one CaveMap first.");
            ConfigureWallReveal(maps[0], scene);
            CaveMapBuilder.Rebuild(maps[0]);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Cave wall reveal saved: full walls, dynamic screen-space reveal and unchanged collisions.", maps[0]);
        }

        private static void ConfigureWallReveal(CaveMap map, UnityEngine.SceneManagement.Scene scene)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/CaveWallReveal.shader");
            if (shader == null || ShaderUtil.ShaderHasError(shader))
                throw new InvalidOperationException("Cave wall reveal shader is missing or has compilation errors.");
            var follow = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<CameraController>(true)).Single();
            if (follow.Target == null) throw new InvalidOperationException("Assign the camera target first.");
            var visual = follow.Target.GetComponentInChildren<MeshRenderer>();
            if (visual == null) throw new InvalidOperationException("Assign a player visual renderer.");
            foreach (var material in new[] { map.GroundMaterial, map.WallMaterial })
            {
                Undo.RecordObject(material, "Enable cave wall reveal material");
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            var reveal = map.GetComponent<CaveWallReveal>();
            if (reveal == null) reveal = Undo.AddComponent<CaveWallReveal>(map.gameObject);
            var settings = new SerializedObject(reveal);
            settings.FindProperty("viewCamera").objectReferenceValue = follow.GetComponent<Camera>();
            settings.FindProperty("targetVisual").objectReferenceValue = visual;
            settings.ApplyModifiedProperties();
        }

        [MenuItem("Untitled/Cave/Enable Scene Tilemap Editing %&t")]
        public static void EnableSceneEditing()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var maps = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<CaveMap>(true)).ToArray();
            if (maps.Length != 1) throw new InvalidOperationException("Open a scene with one CaveMap first.");
            CreateSceneLayout(maps[0]);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Cave Tilemap layout saved: " + CaveMap.ReadCells(maps[0].FloorTilemap).Count + " floor cells, " + CaveMap.ReadCells(maps[0].RiverTilemap).Count + " river cells.", maps[0]);
        }

        public static void CreateSceneLayout(CaveMap map)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            if (map.LayoutGrid != null || map.FloorTilemap != null || map.RiverTilemap != null || map.SpawnPoint != null)
                throw new InvalidOperationException("Scene editing already exists. It will not be overwritten by an import baseline.");
            var baseline = map.ImportBaseline;
            if (baseline == null || baseline.SourcePixelsPerCell <= 0 || baseline.CellSize <= 0f ||
                baseline.FloorCells.Count == 0 || !baseline.FloorCells.Contains(baseline.SpawnCell) ||
                !new HashSet<Vector2Int>(baseline.RiverCells).IsSubsetOf(new HashSet<Vector2Int>(baseline.FloorCells)))
                throw new InvalidOperationException("Assign a valid import baseline first.");
            if (string.IsNullOrEmpty(map.gameObject.scene.path))
                throw new InvalidOperationException("Save the scene first.");

            Tile floorTile = GetOrCreateTile("CaveFloorTile", Color.white);
            Tile riverTile = GetOrCreateTile("CaveRiverTile", new Color(0.2f, 0.7f, 1f));
            CreatePalette(floorTile, riverTile);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create cave scene layout");
            var gridObject = new GameObject("LayoutGrid", typeof(Grid));
            gridObject.transform.SetParent(map.transform, false);
            gridObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            gridObject.transform.localPosition = CaveCoordinates.PixelToLocal(Vector2.zero, baseline);
            var grid = gridObject.GetComponent<Grid>();
            grid.cellSize = new Vector3(baseline.CellSize, baseline.CellSize, 0f);
            Undo.RegisterCreatedObjectUndo(gridObject, "Create layout Grid");
            Tilemap floor = CreateTilemap("Floor", grid.transform, 0.02f);
            Tilemap river = CreateTilemap("River", grid.transform, 0.03f);
            SetCells(floor, baseline.FloorCells, floorTile);
            SetCells(river, baseline.RiverCells, riverTile);
            var spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(map.transform, false);
            spawn.transform.localPosition = CaveCoordinates.CellCenter(baseline.SpawnCell, baseline);
            Undo.RegisterCreatedObjectUndo(spawn, "Create spawn point");
            var settings = new SerializedObject(map);
            settings.FindProperty("layoutGrid").objectReferenceValue = grid;
            settings.FindProperty("floorTilemap").objectReferenceValue = floor;
            settings.FindProperty("riverTilemap").objectReferenceValue = river;
            settings.FindProperty("spawnPoint").objectReferenceValue = spawn.transform;
            // Reuse this scene's existing generated assets; the baseline no longer drives them.
            settings.FindProperty("geometryAssetPrefix").stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(baseline));
            settings.FindProperty("geometrySceneGuid").stringValue = AssetDatabase.AssetPathToGUID(map.gameObject.scene.path);
            settings.ApplyModifiedProperties();

            var player = map.transform.root.GetComponentInChildren<PlayerEntity>();
            if (player != null)
            {
                // Scene wiring only: the player prefab and movement code remain unchanged.
                var entryObject = new GameObject("CaveLevelEntry", typeof(CaveLevelEntry));
                entryObject.transform.SetParent(map.transform, false);
                var entry = new SerializedObject(entryObject.GetComponent<CaveLevelEntry>());
                entry.FindProperty("map").objectReferenceValue = map;
                entry.FindProperty("player").objectReferenceValue = player.GetComponent<CharacterController>();
                entry.ApplyModifiedProperties();
                Undo.RegisterCreatedObjectUndo(entryObject, "Create cave entry");
            }
            CaveMapBuilder.Rebuild(map);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = map.gameObject;
        }

        private static Tilemap CreateTilemap(string name, Transform parent, float height)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, height);
            var renderer = go.GetComponent<TilemapRenderer>();
            renderer.enabled = false;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Undo.RegisterCreatedObjectUndo(go, "Create editable Tilemap");
            return go.GetComponent<Tilemap>();
        }

        private static void SetCells(Tilemap tilemap, List<Vector2Int> cells, Tile tile)
        {
            Undo.RegisterCompleteObjectUndo(tilemap, "Fill scene layout");
            tilemap.SetTiles(cells.Select(c => new Vector3Int(c.x, c.y, 0)).ToArray(), cells.Select(_ => (TileBase)tile).ToArray());
            tilemap.CompressBounds();
        }

        private static Tile GetOrCreateTile(string name, Color color)
        {
            const string folder = "Assets/Configs/Cave/Tiles";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Configs/Cave", "Tiles");
            string path = folder + "/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null) return tile;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Sprites/Terrain/Cave/cave_floor_brown_tileset.png");
            if (texture == null) throw new InvalidOperationException("Cave floor texture is missing.");
            var sprite = Sprite.Create(texture, new Rect(94f, 94f, 32f, 32f), Vector2.one * 0.5f, 32f, 0, SpriteMeshType.FullRect);
            sprite.name = name + "Preview";
            tile = CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = sprite;
            tile.color = color;
            tile.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(tile, path);
            AssetDatabase.AddObjectToAsset(sprite, tile);
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void CreatePalette(Tile floor, Tile river)
        {
            const string path = "Assets/Configs/Cave/Tiles/CavePalette.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GridPaletteUtility.CreateNewPalette("Assets/Configs/Cave/Tiles", "CavePalette", GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual, new Vector3(1f, 1f, 0f), GridLayout.CellSwizzle.XYZ);
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Tilemap tilemap = contents.GetComponentInChildren<Tilemap>();
                tilemap.SetTile(Vector3Int.zero, floor);
                tilemap.SetTile(new Vector3Int(1, 0, 0), river);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        [MenuItem("Untitled/Cave/Import Exported Scene Layout")]
        private static void Import()
        {
            string json = EditorUtility.OpenFilePanel("Select exported cave cells", "", "json");
            if (string.IsNullOrEmpty(json)) return;
            string destination = EditorUtility.SaveFilePanelInProject("Save cave layout", "CaveLayout", "asset", "Save a new layout asset");
            if (string.IsNullOrEmpty(destination)) return;
            if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
                throw new InvalidOperationException("Choose a new asset path to preserve existing edited layouts.");
            ImportLayout(json, destination);
        }

        public static CaveMapLayout ImportLayout(string json, string destination)
        {
            if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
                throw new InvalidOperationException("Import to a new baseline asset; edited layouts are preserved.");
            var data = JsonUtility.FromJson<ImportData>(File.ReadAllText(json));
            if (data == null || data.formatVersion != 1 || data.sourcePixelsPerCell <= 0 || data.cellSize <= 0 ||
                data.floorCells == null || data.riverCells == null || data.floorCells.Length == 0)
                throw new InvalidDataException("Invalid cave export.");
            var floors = new HashSet<Vector2Int>(data.floorCells);
            if (floors.Count != data.floorCells.Length || !floors.Contains(data.spawnCell) || data.riverCells.Any(cell => !floors.Contains(cell)))
                throw new InvalidDataException("Duplicate floors, invalid spawn or river outside floor.");
            var layout = CreateInstance<CaveMapLayout>();
            layout.SourcePixelsPerCell = data.sourcePixelsPerCell;
            layout.SourceOriginPixels = data.sourceOriginPixels;
            layout.CellSize = data.cellSize;
            layout.SpawnCell = data.spawnCell;
            layout.FloorCells = data.floorCells.ToList();
            layout.RiverCells = data.riverCells.ToList();
            AssetDatabase.CreateAsset(layout, destination);
            AssetDatabase.SaveAssets();
            Selection.activeObject = layout;
            return layout;
        }

        [Serializable]
        private sealed class ImportData
        {
            public int formatVersion;
            public int sourcePixelsPerCell;
            public Vector2 sourceOriginPixels;
            public float cellSize;
            public Vector2Int spawnCell;
            public Vector2Int[] floorCells;
            public Vector2Int[] riverCells;
        }
    }
}
