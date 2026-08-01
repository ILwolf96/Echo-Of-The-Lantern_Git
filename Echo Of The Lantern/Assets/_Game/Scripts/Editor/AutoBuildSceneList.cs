#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;


namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Keeps the build scene list synchronized with the game's two canonical scenes.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoBuildSceneList
    {
        private const string MenuScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Menu.unity";
        private const string GameScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Game.unity";
        private const string SessionQueued = "EchoOfTheLantern.AutoBuildSceneList.Queued";
        private const string SessionRunning = "EchoOfTheLantern.AutoBuildSceneList.Running";


        static AutoBuildSceneList()
        {
            EditorApplication.delayCall += QueueRun;
        }


        internal static void QueueRun()
        {
            if (SessionState.GetBool(SessionRunning, false) || SessionState.GetBool(SessionQueued, false))
            {
                return;
            }


            SessionState.SetBool(SessionQueued, true);
            EditorApplication.delayCall -= ExecuteQueuedRun;
            EditorApplication.delayCall += ExecuteQueuedRun;
        }


        private static void ExecuteQueuedRun()
        {
            EditorApplication.delayCall -= ExecuteQueuedRun;


            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }


            if (!SessionState.GetBool(SessionQueued, false))
            {
                return;
            }


            SessionState.SetBool(SessionQueued, false);
            Run();
        }


        private static void Run()
        {
            if (SessionState.GetBool(SessionRunning, false))
            {
                return;
            }


            SessionState.SetBool(SessionRunning, true);


            try
            {
                EditorBuildSettingsScene[] scenes = BuildSceneArray();
                EditorBuildSettings.scenes = scenes;
                EditorBuildSettings.globalScenes = scenes;


                BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
                if (activeProfile != null)
                {
                    activeProfile.overrideGlobalScenes = true;
                    activeProfile.scenes = scenes;
                    EditorUtility.SetDirty(activeProfile);
                }


                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                SessionState.SetBool(SessionRunning, false);
            }
        }


        private static EditorBuildSettingsScene[] BuildSceneArray()
        {
            return new[]
            {
                CreateSceneEntry(MenuScenePath),
                CreateSceneEntry(GameScenePath)
            }.Where(scene => scene != null).ToArray();
        }


        private static EditorBuildSettingsScene CreateSceneEntry(string scenePath)
        {
            if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", scenePath))))
            {
                return null;
            }


            return new EditorBuildSettingsScene(scenePath, true);
        }
    }
}
#endif
