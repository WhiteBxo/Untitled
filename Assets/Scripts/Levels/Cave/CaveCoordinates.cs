using UnityEngine;

namespace Untitled
{
    public static class CaveCoordinates
    {
        public static Vector3 CellCenter(Vector2Int cell, Grid grid)
        {
            Vector3 center = grid.GetCellCenterLocal(new Vector3Int(cell.x, cell.y, 0));
            // Exact XY -> XZ mapping avoids rotation rounding in saved chunk vertices.
            Vector3 local = grid.transform.localPosition + new Vector3(center.x, center.z, -center.y);
            return grid.transform.parent.TransformPoint(local);
        }

        public static Vector2Int WorldToCell(Vector3 world, Grid grid)
        {
            Vector3 local = grid.transform.parent.InverseTransformPoint(world) - grid.transform.localPosition;
            Vector3Int cell = grid.LocalToCell(new Vector3(local.x, -local.z, local.y));
            return new Vector2Int(cell.x, cell.y);
        }

        // Legacy pixel conversion is used only when explicitly importing a baseline.
        public static Vector3 PixelToLocal(Vector2 pixel, CaveMapLayout layout)
        {
            Vector2 offset = (pixel - layout.SourceOriginPixels) * (layout.CellSize / layout.SourcePixelsPerCell);
            return new Vector3(offset.x, 0f, -offset.y);
        }

        public static Vector3 CellCenter(Vector2Int cell, CaveMapLayout layout)
        {
            return PixelToLocal(((Vector2)cell + Vector2.one * 0.5f) * layout.SourcePixelsPerCell, layout);
        }

        public static Vector2Int LocalToCell(Vector3 local, CaveMapLayout layout)
        {
            Vector2 pixel = new Vector2(local.x, -local.z) * (layout.SourcePixelsPerCell / layout.CellSize) + layout.SourceOriginPixels;
            return new Vector2Int(Mathf.FloorToInt(pixel.x / layout.SourcePixelsPerCell), Mathf.FloorToInt(pixel.y / layout.SourcePixelsPerCell));
        }
    }
}
