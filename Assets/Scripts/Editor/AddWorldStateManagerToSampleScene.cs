using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddWorldStateManagerToSampleScene
{
    public static void Execute()
    {
        const string scenePath = "Assets/Scenes/SampleScene.unity";

        // Open SampleScene in single mode for editing
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Check if a WorldStateManager GameObject already exists
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "WorldStateManager" && go.GetComponent<WorldStateManager>() != null)
            {
                Debug.Log("[AddWorldStateManagerToSampleScene] WorldStateManager already exists in SampleScene — skipping.");
                return;
            }
        }

        // Create the GameObject and add the component
        var wsm = new GameObject("WorldStateManager");
        wsm.AddComponent<WorldStateManager>();
        EditorSceneManager.MarkSceneDirty(scene);

        // Save
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[AddWorldStateManagerToSampleScene] WorldStateManager added and SampleScene saved.");
    }
}
