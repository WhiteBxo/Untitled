using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Untitled.Editor
{
    [CustomEditor(typeof(CaveMap))]
    public sealed class CaveMapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Rebuild 3D Map From Layout"))
                CaveMapBuilder.Rebuild((CaveMap)target);
        }

        [MenuItem("Untitled/Cave/Import Exported Scene Layout")]
        private static void Import()
        {
            string json = EditorUtility.OpenFilePanel("Select exported Godot scene cells", "", "json");
            if (string.IsNullOrEmpty(json)) return;
            string destination = EditorUtility.SaveFilePanelInProject("Save cave layout", "CaveLayout", "asset", "Save a new layout asset");
            if (string.IsNullOrEmpty(destination)) return;
            if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
                throw new InvalidOperationException("Choose a new asset path to preserve existing edited layouts.");
            ImportLayout(json, destination);
        }

        public static CaveMapLayout ImportLayout(string json, string destination)
        {
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
