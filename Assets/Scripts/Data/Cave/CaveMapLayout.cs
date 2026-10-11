using System.Collections.Generic;
using UnityEngine;

namespace Untitled
{
    [CreateAssetMenu(menuName = "Untitled/Cave Map Layout")]
    public sealed class CaveMapLayout : ScriptableObject
    {
        public int SourcePixelsPerCell = 32;
        public Vector2 SourceOriginPixels;
        public float CellSize = 1f;
        public Vector2Int SpawnCell;
        public List<Vector2Int> FloorCells = new();
        public List<Vector2Int> RiverCells = new();
    }
}
