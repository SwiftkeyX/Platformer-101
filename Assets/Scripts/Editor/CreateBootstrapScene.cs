using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CreateBootstrapScene
{
    public static void Execute()
    {
        // Create the Bootstrap scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add Bootstrap root GameObject
        var bootstrapGO = new GameObject("Bootstrap");
        bootstrapGO.AddComponent<Bootstrap>();

        // Save the scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/Bootstrap.unity");

        // Set build settings: Bootstrap at index 0, SampleScene at index 1
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        Debug.Log("[CreateBootstrapScene] Bootstrap.unity created at build index 0.");
    }
}
