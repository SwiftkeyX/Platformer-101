using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupPlayerTest
{
    // Step A: create PlayerData asset and wire Bootstrap's startup scene
    public static void SetupAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");

        const string playerDataPath = "Assets/Data/PlayerData.asset";
        if (AssetDatabase.LoadAssetAtPath<PlayerData>(playerDataPath) == null)
        {
            var pd = ScriptableObject.CreateInstance<PlayerData>();
            AssetDatabase.CreateAsset(pd, playerDataPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SetupPlayerTest] Created PlayerData.asset");
        }
        else
        {
            Debug.Log("[SetupPlayerTest] PlayerData.asset already exists — skipped.");
        }

        // Set Bootstrap._startupScene
        var bootstrapScene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single);
        foreach (var go in bootstrapScene.GetRootGameObjects())
        {
            var b = go.GetComponent<Bootstrap>();
            if (b == null) continue;
            var bso = new SerializedObject(b);
            bso.FindProperty("_startupScene").stringValue = "SampleScene";
            bso.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.SaveScene(bootstrapScene);
        Debug.Log("[SetupPlayerTest] Bootstrap._startupScene = SampleScene");
    }

    // Step B: add CharacterController and PlayerController to existing Player GO,
    //         wire references. Run after the Player is already in the scene.
    public static void WirePlayerReferences()
    {
        var scene = EditorSceneManager.GetActiveScene();

        PlayerData playerData = AssetDatabase.LoadAssetAtPath<PlayerData>("Assets/Data/PlayerData.asset");
        InputReader inputReader = AssetDatabase.LoadAssetAtPath<InputReader>("Assets/Input/InputReader.asset");
        if (playerData == null || inputReader == null)
        {
            Debug.LogError("[SetupPlayerTest] PlayerData or InputReader asset missing. Run SetupAssets first.");
            return;
        }

        // Find Player
        GameObject playerGO = null;
        CameraController cameraController = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "Player") playerGO = go;
            var cam = go.GetComponentInChildren<Camera>();
            if (cam != null)
                cameraController = cam.gameObject.GetComponent<CameraController>()
                                ?? cam.gameObject.AddComponent<CameraController>();
        }
        if (playerGO == null) { Debug.LogError("[SetupPlayerTest] No 'Player' GO found in active scene."); return; }

        // Wire PlayerController serialized refs only (CharacterController added via MCP)
        PlayerController pc = playerGO.GetComponent<PlayerController>();
        if (pc == null) { Debug.LogError("[SetupPlayerTest] PlayerController component not found on Player."); return; }
        var so = new SerializedObject(pc);
        so.FindProperty("_inputReader").objectReferenceValue = inputReader;
        so.FindProperty("_data").objectReferenceValue = playerData;
        so.FindProperty("_cameraController").objectReferenceValue = cameraController;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupPlayerTest] PlayerController references wired.");
    }
}
