using System.Collections.Generic;
using UnityEngine;

namespace Untitled
{
    public sealed class CaveMap : MonoBehaviour
    {
        [SerializeField] private CaveMapLayout layout;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material riverMaterial;
        [SerializeField, Min(0.1f)] private float wallHeight = 1.5f;
        [SerializeField] private Transform geometryRoot;
        private HashSet<Vector2Int> floorIndex;

        public CaveMapLayout Layout => layout;
        public Material GroundMaterial => groundMaterial;
        public Material WallMaterial => wallMaterial;
        public Material RiverMaterial => riverMaterial;
        public float WallHeight => wallHeight;
        public Transform GeometryRoot => geometryRoot;
        public Vector3 SpawnPosition => CellToWorld(layout.SpawnCell);

        private void Awake()
        {
            floorIndex = new HashSet<Vector2Int>(layout.FloorCells);
        }

        public Vector3 CellToWorld(Vector2Int cell) => transform.TransformPoint(CaveCoordinates.CellCenter(cell, layout));
        public Vector2Int WorldToCell(Vector3 position) => CaveCoordinates.LocalToCell(transform.InverseTransformPoint(position), layout);

        public bool IsWalkable(Vector3 position)
        {
            floorIndex ??= new HashSet<Vector2Int>(layout.FloorCells);
            return floorIndex.Contains(WorldToCell(position));
        }
    }
}
