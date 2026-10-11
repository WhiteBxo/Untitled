using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Untitled
{
    public class UIManager : ManagerBase<UIManager>
    {
        private readonly Dictionary<string, PanelBase> panels = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> handles = new();

        private Transform root;

        public Transform Root
        {
            get
            {
                if (root == null)
                {
                    Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                    if (canvas != null) root = canvas.transform;
                }

                return root;
            }
        }

        public void SetRoot(Transform uiRoot)
        {
            root = uiRoot;
        }

        public PanelBase Open(string panelName, object data = null)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("[UIManager] 面板名为空，无法打开。");
                return null;
            }

            if (!panels.TryGetValue(panelName, out PanelBase instance))
            {
                instance = Create(panelName);
                if (instance == null) return null;
                panels[panelName] = instance;
            }

            instance.ExecuteInit();
            instance.ExecuteShow(data);
            return instance;
        }

        public T Open<T>(string panelName, object data = null) where T : PanelBase
        {
            return Open(panelName, data) as T;
        }

        public void Close(string panelName)
        {
            if (string.IsNullOrEmpty(panelName)) return;
            if (panels.TryGetValue(panelName, out PanelBase instance)) instance.ExecuteHide();
        }

        public void Close(PanelBase panel)
        {
            if (panel != null) panel.ExecuteHide();
        }

        public void CloseAll()
        {
            foreach (PanelBase instance in panels.Values)
            {
                instance.ExecuteHide();
            }
        }

        public bool IsOpen(string panelName)
        {
            return !string.IsNullOrEmpty(panelName)
                && panels.TryGetValue(panelName, out PanelBase instance)
                && instance.IsOpen;
        }

        public PanelBase Get(string panelName)
        {
            if (string.IsNullOrEmpty(panelName)) return null;
            panels.TryGetValue(panelName, out PanelBase instance);
            return instance;
        }

        public T Get<T>() where T : PanelBase
        {
            foreach (PanelBase instance in panels.Values)
            {
                if (instance is T typed) return typed;
            }

            return null;
        }

        public void Unload(string panelName)
        {
            if (string.IsNullOrEmpty(panelName)) return;

            if (panels.TryGetValue(panelName, out PanelBase instance))
            {
                panels.Remove(panelName);
                if (instance != null) UnityEngine.Object.Destroy(instance.gameObject);
            }

            if (handles.TryGetValue(panelName, out AsyncOperationHandle<GameObject> handle))
            {
                handles.Remove(panelName);
                if (handle.IsValid()) Addressables.ReleaseInstance(handle);
            }
        }

        public void UnloadAll()
        {
            List<string> opened = new(panels.Keys);
            foreach (string panelName in opened)
            {
                Unload(panelName);
            }
        }

        private PanelBase Create(string panelName)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(panelName, Root);
            GameObject instanceObject = handle.WaitForCompletion();

            if (instanceObject == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败：{panelName}（地址 \"{panelName}\"）。");
                if (handle.IsValid()) Addressables.ReleaseInstance(handle);
                return null;
            }

            PanelBase instance = instanceObject.GetComponent<PanelBase>();
            if (instance == null)
            {
                Debug.LogError($"[UIManager] 面板预制体根节点缺少 PanelBase 组件：{panelName}。");
                Addressables.ReleaseInstance(handle);
                return null;
            }

            instance.gameObject.SetActive(false);
            instance.name = panelName;
            instance.PanelName = panelName;
            handles[panelName] = handle;
            return instance;
        }
    }
}
