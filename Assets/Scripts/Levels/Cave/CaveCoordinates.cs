using UnityEngine;

namespace Untitled
{
    public static class CaveCoordinates
    {
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
