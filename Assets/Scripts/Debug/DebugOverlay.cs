using UnityEngine;
using UnityEngine.InputSystem;

namespace Untitled
{
    /// <summary>
    /// 开发者调试面板：IMGUI 绘制，F1 开关，由 DebugManager 在开发构建中创建。
    /// 热键直接查询 Input System 键盘，不改动游戏的 Input Actions。
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private const int WindowId = 57291;
        private const float CoarseStep = 0.5f;

        private Rect windowRect = new(16f, 16f, 360f, 0f);
        private Font chineseFont;
        private GUIStyle windowStyle;
        private GUIStyle valueStyle;
        private GUIStyle labelStyle;
        private GUIStyle hintStyle;
        private bool showHint = true;

        private void Awake()
        {
            // IMGUI 默认字体不含中文字形，这里用系统字体绘制面板文字
            chineseFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 18);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[Key.F1].wasPressedThisFrame) return;

            DebugManager.Instance.TogglePanel();
        }

        private void OnGUI()
        {
            DebugManager debug = DebugManager.Instance;
            if (debug == null) return;

            EnsureStyles();

            if (!debug.IsPanelOpen)
            {
                if (showHint) DrawHint();
                return;
            }

            showHint = false;
            windowRect = GUILayout.Window(WindowId, windowRect, DrawPanel, "开发者调试（F1 关闭）", windowStyle);
        }

        private void DrawPanel(int windowId)
        {
            DebugManager debug = DebugManager.Instance;

            GUILayout.BeginHorizontal();
            GUILayout.Label("玩家移速倍数", labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("×" + debug.MoveSpeedMultiplier.ToString("0.00"), valueStyle);
            GUILayout.EndHorizontal();

            debug.MoveSpeedMultiplier = GUILayout.HorizontalSlider(
                debug.MoveSpeedMultiplier, DebugManager.MoveSpeedMultiplierMin, DebugManager.MoveSpeedMultiplierMax);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-0.5")) debug.AddMoveSpeedMultiplier(-CoarseStep);
            if (GUILayout.Button("+0.5")) debug.AddMoveSpeedMultiplier(CoarseStep);
            if (GUILayout.Button("重置")) debug.ResetMoveSpeedMultiplier();
            if (GUILayout.Button("关闭")) debug.ClosePanel();
            GUILayout.EndHorizontal();

            GUILayout.Label(DescribePlayerSpeed(debug), labelStyle);

            GUI.DragWindow();
        }

        private static string DescribePlayerSpeed(DebugManager debug)
        {
            PlayerEntity player = debug.Player;
            if (player == null) return "当前场景没有找到 PlayerEntity";

            float speed = player.Speed * debug.MoveSpeedMultiplier;
            return "玩家基础速度 " + player.Speed.ToString("0.0") + " → 实际 " + speed.ToString("0.0") + " 世界单位/秒";
        }

        private void DrawHint()
        {
            GUILayout.BeginArea(new Rect(16f, 16f, 260f, 24f));
            GUILayout.Label("按 F1 打开开发者调试面板", hintStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (windowStyle != null) return;

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                font = chineseFont,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(12, 12, 28, 12)
            };
            valueStyle = new GUIStyle(GUI.skin.label)
            {
                font = chineseFont,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                font = chineseFont,
                fontSize = 15
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                font = chineseFont,
                fontSize = 15
            };
            hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
        }
    }
}
