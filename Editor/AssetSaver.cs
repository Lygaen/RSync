using UnityEditor;
using UnityEditor.Callbacks;

namespace RSync
{
    public class AssetSaver : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            RSyncManager.SyncAll();
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            RSyncManager.SyncAll();
        }
    }
}