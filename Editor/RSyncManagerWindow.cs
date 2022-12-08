using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RSync.Editor
{
    public class RSyncManagerWindow : EditorWindow
    {
        private const string SshRegex = "([a-z-A-Z]+@)?(([a-z-A-Z]+\\.[a-z-A-Z]+)|([0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}))";
        private static string _publicIP = string.Empty;

        [MenuItem("RSync/Reload")]
        public static void Reload()
        {
            if(!RSyncManager.IsClone)
                return;

            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
        }
        
        [MenuItem("RSync/Manager")]
        private static void ShowWindow()
        {
            var window = GetWindow<RSyncManagerWindow>();
            window.titleContent = new GUIContent("RSync");
            window.Show();
        }

        private void OnGUI()
        {
            if (RSyncManager.IsClone)
            {
                EditorGUILayout.HelpBox("This is a project clone\nSorry but you won't be able to modify anything :/",
                    MessageType.Info);
                GUI.enabled = false;
                EditorGUILayout.TextField("Local IP", GetLocalIP());
                EditorGUILayout.TextField("Public IP", GetPublicIP());
                GUI.enabled = true;
                return;
            }

            for (var i = 0; i < RSyncManager.SshTargets.Count; i++)
            {
                var sshTarget = RSyncManager.SshTargets[i];
                var oldColor = GUI.contentColor;
                var invalidRegex = !Regex.IsMatch(sshTarget, SshRegex);
                if (!Regex.IsMatch(sshTarget, SshRegex))
                {
                    GUI.contentColor *= Color.red;
                    RSyncManager.SshTargets[i] = EditorGUILayout.TextField("SSH Target", sshTarget);
                    GUI.contentColor = oldColor;
                }
                else
                {
                    RSyncManager.SshTargets[i] = EditorGUILayout.TextField("SSH Target", sshTarget);
                }

                GUILayout.BeginHorizontal();
                if (invalidRegex || RSyncManager.InProgress)
                {
                    GUI.enabled = false;
                    GUILayout.Button(new GUIContent("Sync", RSyncManager.InProgress ? "A Sync is already in progress !" : "Invalid SSH target !"));
                    GUI.enabled = true;
                }
                else if (GUILayout.Button("Sync"))
                    RSyncManager.SyncSingle(sshTarget);

                if (GUILayout.Button("Remove")) 
                    RSyncManager.SshTargets.RemoveAt(i);
                GUILayout.EndHorizontal();
                
                EditorGUILayout.Separator();
            }
            if (GUILayout.Button("Add Target")) 
                RSyncManager.SshTargets.Add(string.Empty);
            EditorGUILayout.Separator();

            if (RSyncManager.InProgress)
            {
                GUI.enabled = false;
                GUILayout.Button(new GUIContent("Sync All", "A Sync is already in progress !"));
                GUI.enabled = true;
            }
            else if(GUILayout.Button("Sync All"))
                RSyncManager.SyncAll();
            EditorGUILayout.Separator();

            if (!RSyncManager.InProgress) return;
            
            GUI.enabled = false;
            EditorGUILayout.TextField("Syncing Target", RSyncManager.CurrentTarget);
            EditorGUILayout.TextField("Syncing File", RSyncManager.CurrentFile);
            GUI.enabled = true;
        }

        private string GetLocalIP()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return "NOT_CONNECTED";
            
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "NO_INTERFACE";
        }

        private string GetPublicIP()
        {
            if (_publicIP != string.Empty)
                return _publicIP;
            
            if (!NetworkInterface.GetIsNetworkAvailable())
                return _publicIP = "NOT_CONNECTED";
            
            var request = (HttpWebRequest)WebRequest.Create("https://ifconfig.me");
            request.UserAgent = "curl";
            request.Method = "GET";
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()!))
                {
                    _publicIP = reader.ReadToEnd();
                }
            }

            return _publicIP;
        }
    }
}