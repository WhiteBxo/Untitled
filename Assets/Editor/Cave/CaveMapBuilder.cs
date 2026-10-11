using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Untitled.Editor
{
    public static class CaveMapBuilder
    {
        private const int ChunkSize = 16;
        private const string MeshFolder = "Assets/Art/Models/Cave";

        public static void Rebuild(CaveMap map)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before rebuilding.");
            var layout = map.Layout;
            if (layout == null || layout.CellSize <= 0 || layout.SourcePixelsPerCell <= 0 ||
                map.GroundMaterial == null || map.WallMaterial == null || map.RiverMaterial == null)
                throw new InvalidOperationException("Assign a valid layout and three materials.");
            var floors = new HashSet<Vector2Int>(layout.FloorCells);
            var rivers = new HashSet<Vector2Int>(layout.RiverCells);
            if (floors.Count == 0 || !floors.Contains(layout.SpawnCell) || !rivers.IsSubsetOf(floors))
                throw new InvalidOperationException("Spawn and rivers must be on floor cells.");
            Directory.CreateDirectory(MeshFolder);
            AssetDatabase.Refresh();
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Rebuild cave geometry");
            if (map.GeometryRoot != null) Undo.DestroyObjectImmediate(map.GeometryRoot.gameObject);
            var root = new GameObject("Geometry");
            Undo.RegisterCreatedObjectUndo(root, "Create cave geometry");
            root.transform.SetParent(map.transform, false);
            var serialized = new SerializedObject(map);
            serialized.FindProperty("geometryRoot").objectReferenceValue = root.transform;
            serialized.ApplyModifiedProperties();

            var chunks = new Dictionary<Vector2Int, Chunk>();
            var collisionChunks = new Dictionary<Vector2Int, Chunk>();
            var waterChunks = new Dictionary<Vector2Int, Chunk>();
            Vector2Int Key(Vector2Int cell) => new(Mathf.FloorToInt((float)cell.x / ChunkSize), Mathf.FloorToInt((float)cell.y / ChunkSize));
            Chunk GetChunk(Vector2Int cell)
            {
                var key = Key(cell);
                if (!chunks.TryGetValue(key, out var value)) { value = new Chunk(); chunks.Add(key, value); }
                return value;
            }
            foreach (Vector2Int cell in floors.OrderBy(c => c.y).ThenBy(c => c.x))
            {
                Vector3 center = CaveCoordinates.CellCenter(cell, layout);
                Vector3 size = new(layout.CellSize, 0.3f, layout.CellSize);
                Vector2Int key = Key(cell);
                if (!collisionChunks.TryGetValue(key, out var collision))
                    collisionChunks.Add(key, collision = new Chunk());
                collision.Box(center + Vector3.down * 0.15f, size, 0);
                GetChunk(cell).Box(center + Vector3.down * (rivers.Contains(cell) ? 0.31f : 0.15f), size, 0);
                if (rivers.Contains(cell))
                {
                    if (!waterChunks.TryGetValue(key, out var water))
                        waterChunks.Add(key, water = new Chunk());
                    // Only exposed river edges receive foam; shared cell/chunk edges stay seamless.
                    var banks = new Color(
                        rivers.Contains(cell + Vector2Int.left) ? 0 : 1,
                        rivers.Contains(cell + Vector2Int.right) ? 0 : 1,
                        rivers.Contains(cell + Vector2Int.up) ? 0 : 1,
                        rivers.Contains(cell + Vector2Int.down) ? 0 : 1);
                    water.Top(center + Vector3.down * 0.055f, layout.CellSize, 0, banks);
                }
            }
            var walls = new HashSet<Vector2Int>();
            foreach (Vector2Int cell in floors)
                for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
                {
                    var candidate = cell + new Vector2Int(x, y);
                    if (!floors.Contains(candidate)) walls.Add(candidate);
                }
            foreach (Vector2Int cell in walls.OrderBy(c => c.y).ThenBy(c => c.x))
            {
                Vector3 center = CaveCoordinates.CellCenter(cell, layout);
                GetChunk(cell).Box(center + Vector3.up * map.WallHeight * 0.5f,
                    new Vector3(layout.CellSize, map.WallHeight, layout.CellSize), 1);
                Vector2Int key = Key(cell);
                if (!collisionChunks.TryGetValue(key, out var collision))
                    collisionChunks.Add(key, collision = new Chunk());
                collision.Box(center + Vector3.up * map.WallHeight * 0.5f,
                    new Vector3(layout.CellSize, map.WallHeight, layout.CellSize), 1);
            }
            string assetPrefix = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(layout));
            foreach (var pair in chunks.OrderBy(c => c.Key.y).ThenBy(c => c.Key.x))
            {
                string name = "Chunk_" + pair.Key.x + "_" + pair.Key.y;
                string path = MeshFolder + "/" + assetPrefix + "_" + name + ".asset";
                var saved = SaveMesh(pair.Value.Build(name), path);
                var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                Undo.RegisterCreatedObjectUndo(go, "Create cave chunk");
                go.transform.SetParent(root.transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = saved;
                go.GetComponent<MeshRenderer>().sharedMaterials = new[] { map.GroundMaterial, map.WallMaterial, map.RiverMaterial };
                // Keep the original flat walk surface, independently of the sunken water visual.
                go.GetComponent<MeshCollider>().sharedMesh = waterChunks.ContainsKey(pair.Key)
                    ? SaveMesh(collisionChunks[pair.Key].Build(name + "_Collision"), path.Replace(".asset", "_Collision.asset"))
                    : saved;
                if (waterChunks.TryGetValue(pair.Key, out var water))
                {
                    var surface = new GameObject("Water", typeof(MeshFilter), typeof(MeshRenderer));
                    Undo.RegisterCreatedObjectUndo(surface, "Create river surface");
                    surface.transform.SetParent(go.transform, false);
                    surface.GetComponent<MeshFilter>().sharedMesh = SaveMesh(water.Build(name + "_Water", true), path.Replace(".asset", "_Water.asset"));
                    var renderer = surface.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = map.RiverMaterial;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
            AssetDatabase.SaveAssets();
            Undo.CollapseUndoOperations(undoGroup);
        }

        private static Mesh SaveMesh(Mesh mesh, string path)
        {
            mesh.name = Path.GetFileNameWithoutExtension(path);
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
            Undo.RegisterCompleteObjectUndo(saved, "Update cave mesh");
            EditorUtility.CopySerialized(mesh, saved);
            EditorUtility.SetDirty(saved);
            UnityEngine.Object.DestroyImmediate(mesh);
            return saved;
        }

        private sealed class Chunk
        {
            private readonly List<Vector3> vertices = new();
            private readonly List<Vector3> normals = new();
            private readonly List<Vector2> uv = new();
            private readonly List<Color> colors = new();
            private readonly List<int>[] triangles = { new(), new(), new() };

            private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int material, Color banks = default)
            {
                int index = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                for (int i = 0; i < 4; i++) normals.Add(normal);
                for (int i = 0; i < 4; i++) colors.Add(banks);
                uv.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                bool forward = Vector3.Dot(Vector3.Cross(b - a, c - a), normal) > 0f;
                triangles[material].AddRange(forward
                    ? new[] { index, index + 1, index + 2, index, index + 2, index + 3 }
                    : new[] { index, index + 2, index + 1, index, index + 3, index + 2 });
            }

            public void Top(Vector3 center, float size, int material, Color banks = default)
            {
                float half = size * 0.5f;
                Quad(center + new Vector3(-half, 0, -half), center + new Vector3(half, 0, -half),
                    center + new Vector3(half, 0, half), center + new Vector3(-half, 0, half), Vector3.up, material, banks);
            }

            public void Box(Vector3 center, Vector3 size, int sideMaterial)
            {
                float x = size.x * 0.5f, y = size.y * 0.5f, z = size.z * 0.5f;
                Vector3 V(float px, float py, float pz) => center + new Vector3(px, py, pz);
                Top(center + Vector3.up * y, size.x, 0);
                Quad(V(-x,-y,-z), V(x,-y,-z), V(x,y,-z), V(-x,y,-z), Vector3.back, sideMaterial);
                Quad(V(x,-y,z), V(-x,-y,z), V(-x,y,z), V(x,y,z), Vector3.forward, sideMaterial);
                Quad(V(-x,-y,z), V(-x,-y,-z), V(-x,y,-z), V(-x,y,z), Vector3.left, sideMaterial);
                Quad(V(x,-y,-z), V(x,-y,z), V(x,y,z), V(x,y,-z), Vector3.right, sideMaterial);
                Quad(V(-x,-y,z), V(x,-y,z), V(x,-y,-z), V(-x,-y,-z), Vector3.down, sideMaterial);
            }

            public Mesh Build(string name, bool water = false)
            {
                var mesh = new Mesh { name = name, subMeshCount = water ? 1 : 3 };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uv);
                if (water) mesh.SetColors(colors);
                for (int i = 0; i < mesh.subMeshCount; i++) mesh.SetTriangles(triangles[i], i);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
