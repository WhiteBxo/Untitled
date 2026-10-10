using System.Collections.Generic;

namespace Untitled
{
    public class PackageData
    {
        public List<ItemData> Items;

        public PackageData()
        {
            Items = new List<ItemData>();
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}