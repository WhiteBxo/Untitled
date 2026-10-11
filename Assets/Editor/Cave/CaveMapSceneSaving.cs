using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Untitled.Editor
{
    [InitializeOnLoad]
    public static class CaveMapSceneSaving
    {
        static CaveMapSceneSaving()
        {
            EditorSceneManager.sceneSaving += SynchronizeGeometry;
            EditorApplication.playModeStateChanged += PreparePlayMode;
        }

        private static void SynchronizeGeometry(Scene scene, string path)
        {
            if (EditorApplication.isPlaying || string.IsNullOrEmpty(path)) return;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var map in root.GetComponentsInChildren<CaveMap>(true))
                {
                    if (!map.HasEditableLayout) continue;
                    try { CaveMapBuilder.EnsureCurrent(map); }
                    catch (InvalidOperationException error)
                    {
                        // Incomplete edits must still be saved; Play Mode validates them separately.
                        Debug.LogError("Cave layout saved, but geometry needs correction: " + error.Message, map);
                    }
                }
        }

        private static void PreparePlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            try
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded) continue;
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var map in root.GetComponentsInChildren<CaveMap>(true))
                            if (map.HasEditableLayout) CaveMapBuilder.EnsureCurrent(map);
                }
            }
            catch (InvalidOperationException error)
            {
                Debug.LogError("Cave layout cannot enter Play Mode: " + error.Message);
                EditorApplication.isPlaying = false;
            }
        }
    }
}
