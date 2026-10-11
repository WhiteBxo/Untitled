using System.Collections.Generic;

namespace Untitled
{
    public class InventoryData
    {
        public List<ItemData> Items;
        private Dictionary<int, ItemData> itemDict;

        public InventoryData()
        {
            Items = new List<ItemData>();
            itemDict = new Dictionary<int, ItemData>();
        }   

        /// <summary>获得物品</summary>
        public void AddItem(int id, int num)
        {
            if (itemDict.TryGetValue(id, out var existItem))
            {
                existItem.Num += num;
            }
            else
            {
                var item = new ItemData(id, num);
                Items.Add(item);
                itemDict.Add(id, item);
            }
        }

        /// <summary>消耗物品</summary>
        public bool ConsumeItem(int id, int num)
        {
            if (!itemDict.TryGetValue(id, out var item))
                return false;

            if (item.Num < num)
                return false;

            item.Num -= num;

            if (item.Num <= 0)
            {
                Items.Remove(item);
                itemDict.Remove(id);
            }
            return true;
        }

        /// <summary>查询物品数量</summary>
        public int GetItemCount(int itemId)
        {
            itemDict.TryGetValue(itemId, out var item);
            return item?.Num ?? 0;
        }

        /// <summary>是否拥有足够物品</summary>
        public bool HasItem(int itemId, int num)
        {
            return GetItemCount(itemId) >= num;
        }

        /// <summary>清空仓库</summary>
        public void Clear()
        {
            Items.Clear();
            itemDict.Clear();
        }
    }
}