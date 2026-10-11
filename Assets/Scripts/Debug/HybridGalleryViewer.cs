using UnityEngine;

namespace Untitled
{
    // Preview-scene controls use IMGUI, leaving the game's Input Actions and input settings untouched.
    public sealed class HybridGalleryViewer : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private HybridProp[] props;
        private int selected;
        private bool overview = true;
        private float yaw, pitch = 25f, distance = 4f;
        private Vector3 overviewPosition;
        private Quaternion overviewRotation;
        private float overviewSize;
        private Font chineseFont;

        public Camera ViewCamera => viewCamera;
        public HybridProp[] Props => props;
        public HybridProp Selected => props != null && props.Length > 0 ? props[selected] : null;

        private void Start()
        {
            if (viewCamera == null) return;
            overviewPosition = viewCamera.transform.position;
            overviewRotation = viewCamera.transform.rotation;
            overviewSize = viewCamera.orthographicSize;
            chineseFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 18);
        }

        public void FocusIndex(int index)
        {
            if (props == null || props.Length == 0) return;
            selected = (index % props.Length + props.Length) % props.Length;
            overview = false;
            yaw = 0f;
            pitch = 25f;
            distance = Mathf.Max(2.3f, Selected.DefaultSize.magnitude * 1.5f);
            UpdateCamera();
        }

        public void ScaleSelected(float value)
        {
            if (Selected != null) Selected.transform.localScale = Vector3.one * Mathf.Clamp(value, 0.1f, 3f);
        }

        private void UpdateCamera()
        {
            if (viewCamera == null || Selected == null || overview) return;
            Vector3 center = Selected.transform.position
                + Vector3.up * Selected.DefaultSize.y * Selected.transform.localScale.x * 0.5f;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            viewCamera.orthographic = false;
            viewCamera.transform.SetPositionAndRotation(center + rotation * Vector3.back * distance, rotation);
        }

        private void RestoreOverview()
        {
            overview = true;
            viewCamera.orthographic = true;
            viewCamera.orthographicSize = overviewSize;
            viewCamera.transform.SetPositionAndRotation(overviewPosition, overviewRotation);
        }

        private void OnGUI()
        {
            if (Selected == null || viewCamera == null) return;
            GUIStyle title = new GUIStyle(GUI.skin.label) { font = chineseFont, fontSize = 20, fontStyle = FontStyle.Bold };
            GUIStyle body = new GUIStyle(GUI.skin.label) { font = chineseFont, fontSize = 16 };
            GUILayout.BeginArea(new Rect(16f, 16f, Mathf.Min(530f, Screen.width - 32f), 245f), GUI.skin.box);
            GUILayout.Label(overview ? "HD-2D 素材展厅 / " + props.Length + " Prefabs"
                : (selected + 1) + "/" + props.Length + "  " + Selected.DisplayName, title);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("上一个")) FocusIndex(selected - 1);
            if (GUILayout.Button("下一个")) FocusIndex(selected + 1);
            if (GUILayout.Button("总览")) RestoreOverview();
            if (GUILayout.Button("恢复大小")) ScaleSelected(1f);
            GUILayout.EndHorizontal();
            if (!overview)
            {
                GUILayout.Label("大小 " + Selected.transform.localScale.x.ToString("0.00") + " 倍", body);
                ScaleSelected(GUILayout.HorizontalSlider(Selected.transform.localScale.x, 0.25f, 2.5f));
                GUILayout.Label("水平角度 / 俯角 / 距离", body);
                yaw = GUILayout.HorizontalSlider(yaw, 0f, 359f);
                pitch = GUILayout.HorizontalSlider(pitch, 0f, 65f);
                distance = GUILayout.HorizontalSlider(distance, 1f, 10f);
                UpdateCamera();
            }
            else GUILayout.Label("选择物件后可调整角度与大小；编辑器也可直接拖入 Prefab。", body);
            GUILayout.EndArea();
        }
    }
}
