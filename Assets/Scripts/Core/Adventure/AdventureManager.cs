using UnityEngine;

namespace Untitled
{
    public class AdventureManager : MonoBehaviour
    {
        [SerializeField]
        private float nightTime;

        private float timer;
        private PackageData packageData;

        private void Start()
        {
            packageData = GameManager.Instance.GameData.PackageData;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
        }

        public void StartAdventure()
        {
            timer = nightTime;
            packageData.Clear();
        }

        public void EndAdventure()
        {
            SettleItem();
            // 显示战利品结算界面

            // 进入下一天经营
        }
        
        private void SettleItem()
        {
            var inventoryData = GameManager.Instance.GameData.InventoryData;
            for (int i = 0; i < packageData.Items.Count; i++)
            {
                ItemData itemData = packageData.Items[i];
                inventoryData.AddItem(itemData.Id, itemData.Num);
            }
        }
    }
}
