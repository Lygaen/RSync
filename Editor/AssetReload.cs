using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RSync
{
    [InitializeOnLoad]
    public class AssetReload
    {
        static AssetReload()
        {
            if (RSyncManager.IsClone)
                EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (!RSyncManager.WasReloaded()) return;

            RSyncManagerWindow.Reload();
            RSyncManager.CleanReload();
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (RSyncManager.IsClone)
                return;

            RSyncManager.SyncAll();
        }
    }
}