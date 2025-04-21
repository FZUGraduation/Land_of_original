using UnityEditor;
using UnityEngine;

public class AutoRefresh
{
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
        EditorApplication.playModeStateChanged += OnPlaymodeChanged;
    }

    // private static void OnPlayModeStateChanged(PlayModeStateChange state)
    // {
    //     if (state == PlayModeStateChange.ExitingEditMode)
    //     {
    //         // 编译项目
    //         AssetDatabase.Refresh();
    //         //BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "TempBuild", BuildTarget.StandaloneWindows, BuildOptions.Development);
    //     }
    // }
    // static AutoRefresh()
    // {
    //     EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
    //     EditorApplication.playModeStateChanged += OnPlaymodeChanged;
    // }

    static void OnPlaymodeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {

            Debug.Log("[method]:ExitingEditMode");
            AssetDatabase.Refresh();
            bool autoRefresh = EditorPrefs.GetBool("kAutoRefresh", false);
            if (!autoRefresh)
            {
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}
