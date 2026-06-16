using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class FixMainMenuInputModule
{
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);

        GameObject eventSystemGO = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "EventSystem") { eventSystemGO = go; break; }
        }
        if (eventSystemGO == null) { Debug.LogError("[FixMainMenuInputModule] EventSystem GameObject not found."); return; }

        var legacy = eventSystemGO.GetComponent<StandaloneInputModule>();
        if (legacy != null) Object.DestroyImmediate(legacy);

        var module = eventSystemGO.GetComponent<InputSystemUIInputModule>();
        if (module == null) module = eventSystemGO.AddComponent<InputSystemUIInputModule>();

        module.AssignDefaultActions();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixMainMenuInputModule] Removed StandaloneInputModule, added InputSystemUIInputModule with default actions. Scene saved.");
    }
}
