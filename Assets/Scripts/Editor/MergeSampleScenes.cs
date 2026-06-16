using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MergeSampleScenes
{
    public static void Execute()
    {
        var destination = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        var source = EditorSceneManager.OpenScene("Assets/SampleScene.unity", OpenSceneMode.Additive);

        string[] toMove = { "Ground", "Player", "Key_A", "Door_A", "Main Camera" };

        GameObject oldDestCamera = null;
        foreach (var go in destination.GetRootGameObjects())
            if (go.name == "Main Camera") { oldDestCamera = go; break; }

        foreach (var name in toMove)
        {
            GameObject go = null;
            foreach (var root in source.GetRootGameObjects())
                if (root.name == name) { go = root; break; }

            if (go == null) { Debug.LogWarning($"[MergeSampleScenes] '{name}' not found in source scene."); continue; }

            SceneManager.MoveGameObjectToScene(go, destination);
        }

        if (oldDestCamera != null)
            Object.DestroyImmediate(oldDestCamera);

        EditorSceneManager.CloseScene(source, true);
        EditorSceneManager.SaveScene(destination);

        Debug.Log("[MergeSampleScenes] Moved Ground/Player/Key_A/Door_A/Main Camera into Assets/Scenes/SampleScene.unity. Old bare camera removed. Scene saved.");
    }
}
