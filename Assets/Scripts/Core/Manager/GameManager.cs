using UnityEngine;

namespace Untitled
{
    public class GameManager : ManagerBase<GameManager>
    {
        public GameData GameData;
        
        public override void Initialize()
        {
            base.Initialize();
            GameData = new GameData();
        }
    }
}
