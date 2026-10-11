using UnityEngine;

namespace Untitled
{
    public class GameManager : ManagerBase<GameManager>
    {
        public GameData GameData;

        public bool IsPaused { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            GameData = new GameData();
            EventCenter.Subscribe(GameEventType.ToggleESC, OnToggleMenu);
        }

        public void OpenMenu()
        {
            Pause();
            if (UIManager.Instance.Open(UIPanel.Menu) == null) Resume();
        }

        public void CloseMenu()
        {
            UIManager.Instance.Close(UIPanel.Menu);
            Resume();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
        }

        private void OnToggleMenu()
        {
            if (UIManager.Instance.IsOpen(UIPanel.Menu)) CloseMenu();
            else OpenMenu();
        }
    }
}
