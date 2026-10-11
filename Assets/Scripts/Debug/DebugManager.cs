using UnityEngine;

namespace Untitled
{
    /// <summary>
    /// 开发者调试工具：集中保存调试参数与面板开关状态，由 Bootstrap 初始化。
    /// 调试值只存在于运行时，不写回预制体或配置资产。
    /// </summary>
    public class DebugManager : ManagerBase<DebugManager>
    {
        public const float MoveSpeedMultiplierMin = 0.1f;
        public const float MoveSpeedMultiplierMax = 10f;

        private const float MoveSpeedMultiplierDefault = 1f;

        private float moveSpeedMultiplier = MoveSpeedMultiplierDefault;
        private DebugOverlay overlay;

        /// <summary>调试面板是否打开。</summary>
        public bool IsPanelOpen { get; private set; }

        /// <summary>玩家移速倍数，1 为原始速度，超出范围会被夹取。</summary>
        public float MoveSpeedMultiplier
        {
            get => moveSpeedMultiplier;
            set => moveSpeedMultiplier = Mathf.Clamp(value, MoveSpeedMultiplierMin, MoveSpeedMultiplierMax);
        }

        /// <summary>当前场景的玩家，打开面板时重新查找，用于显示实时速度。</summary>
        public PlayerEntity Player { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            // 正式构建不创建调试面板；编辑器 Play Mode 与 Development Build 中为 true
            if (Debug.isDebugBuild) CreateOverlay();
        }

        public void OpenPanel()
        {
            IsPanelOpen = true;
            Player = UnityEngine.Object.FindObjectOfType<PlayerEntity>();
        }

        public void ClosePanel()
        {
            IsPanelOpen = false;
        }

        public void TogglePanel()
        {
            if (IsPanelOpen) ClosePanel();
            else OpenPanel();
        }

        /// <summary>按步长增减移速倍数，超出范围会被夹取。</summary>
        public void AddMoveSpeedMultiplier(float delta)
        {
            MoveSpeedMultiplier += delta;
        }

        /// <summary>恢复移速倍数默认值。</summary>
        public void ResetMoveSpeedMultiplier()
        {
            MoveSpeedMultiplier = MoveSpeedMultiplierDefault;
        }

        private void CreateOverlay()
        {
            if (overlay != null) return;

            GameObject overlayObject = new("DebugOverlay");
            overlay = overlayObject.AddComponent<DebugOverlay>();
        }
    }
}
