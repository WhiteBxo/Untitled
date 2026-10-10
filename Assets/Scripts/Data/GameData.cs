namespace Untitled
{
    public class GameData
    {
        public int Money;
        public InventoryData InventoryData;  
        public PackageData PackageData; 

        public GameData()
        {
            Money = 0;
            InventoryData = new InventoryData();
            PackageData = new PackageData();
        }
    }
}