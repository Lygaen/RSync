using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RSync
{
    public static class RSyncManager
    {
        private const string CloneCheckFile = ".clone";
        private const string CloneReloadedFile = ".clone_reloaded";
        public static readonly List<string> SshTargets = new();
        [CanBeNull] 
        public static string CurrentTarget = null;
        [CanBeNull] 
        public static string CurrentFile = null;
        public static bool InProgress;

        public static bool IsClone
        {
            get
            {
                if (_cloneFileExists.HasValue) return _cloneFileExists.Value;
                
                var path = Path.Combine(GetProjectPath(), CloneCheckFile);
                
                return (_cloneFileExists = File.Exists(path)).Value;
            }
            private set => _cloneFileExists = value;
        }
        private static bool? _cloneFileExists;

        public static void SyncAll()
        {
            if(IsClone || InProgress)
                return;
            
            foreach (var sshTarget in SshTargets)
            {
                SyncSingle(sshTarget);
            }
        }

        public static bool WasReloaded()
        {
            return IsClone && File.Exists(Path.Combine(GetProjectPath(), CloneReloadedFile));
        }

        public static void SyncSingle(string curTarget)
        {
            
            if (IsClone || InProgress)
                return;
            
            Debug.Log($"Starting cloning to {curTarget} !");
            CurrentTarget = curTarget;
            
            InProgress = true;
            var projPath = GetProjectPath();

            new Thread(() => { 
                LaunchProcess("ssh", curTarget + " " +
                                     "\"mkdir -p " + projPath + "\"");
                LaunchProcess("rsync", "-avh " +
                                       projPath + " " +
                                       curTarget + ":" + Path.Combine(projPath, ".."));
                LaunchProcess("ssh", curTarget + " " +
                                     "\"cd " + projPath + " && touch " + CloneCheckFile + "\"");
                LaunchProcess("ssh", curTarget + " " +
                                     "\"cd " + projPath + " && touch " + CloneReloadedFile + "\"");
                InProgress = false;
                Debug.Log($"Cloned to {curTarget} !");
                CurrentTarget = null;
                CurrentFile = null;
            }).Start();
        }

        private static void LaunchProcess(string command, string args)
        {
            var pInfo = new ProcessStartInfo(command, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(pInfo);
            if (process == null)
                return;

            while (!process.StandardOutput.EndOfStream)
            {
                CurrentFile = process.StandardOutput.ReadLine();
            }

            process.WaitForExit();
            process.Close();
        }

        private static string GetProjectPath()
        {
            return Application.dataPath.Replace("/Assets", "");
        }

        public static void CleanReload()
        {
            File.Delete(Path.Combine(GetProjectPath(), CloneReloadedFile));
        }
    }
}