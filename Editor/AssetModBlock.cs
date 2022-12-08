using System.Collections.Generic;
using UnityEditor;

namespace RSync.Editor
{
    public class AssetModBlock : AssetModificationProcessor
    {
        private static bool CanOpenForEdit(string[] paths, List<string> outNotEditablePaths, StatusQueryOptions statusQueryOptions)
        {
            if (RSyncManager.IsClone) 
                outNotEditablePaths.AddRange(paths);
            return !RSyncManager.IsClone;
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            return RSyncManager.IsClone ? new string[] { } : paths;
        }

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            return RSyncManager.IsClone ? AssetDeleteResult.FailedDelete : AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            return RSyncManager.IsClone ? AssetMoveResult.FailedMove : AssetMoveResult.DidNotMove;
        }
    }
}
