using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RewireBootstrapScene
{
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single);

        // Clear everything in the scene
        foreach (var go in scene.GetRootGameObjects())
            Object.DestroyImmediate(go);

        // Bootstrap root
        var bootstrapGO = new GameObject("Bootstrap");
        bootstrapGO.AddComponent<Bootstrap>();

        // GameManager as child
        var gmGO = new GameObject("GameManager");
        gmGO.transform.SetParent(bootstrapGO.transform);
        gmGO.AddComponent<GameManager>();

        // SceneLoader as child
        var slGO = new GameObject("SceneLoader");
        slGO.transform.SetParent(bootstrapGO.transform);
        slGO.AddComponent<SceneLoader>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RewireBootstrapScene] Done. Bootstrap has GameManager and SceneLoader as scene children (no DontDestroyOnLoad).");
    }
}
