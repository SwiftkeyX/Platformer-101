using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupKeyDoorTest
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "SampleScene")
        {
            Debug.LogError("[SetupKeyDoorTest] Active scene is not SampleScene. Open SampleScene first.");
            return;
        }

        // 1 — Tag Player "Player"
        GameObject playerGO = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Player") { playerGO = go; break; }
        if (playerGO == null) { Debug.LogError("[SetupKeyDoorTest] No 'Player' GO found."); return; }
        playerGO.tag = "Player";

        // 2 — Key pickup (cube, trigger sphere, position 5m away)
        var keyGO = new GameObject("Key_A");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(keyGO, scene);
        keyGO.transform.position = new Vector3(5, 0.5f, 0);
        keyGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        // Visual mesh
        var keyMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyMesh.name = "Mesh";
        keyMesh.transform.SetParent(keyGO.transform);
        keyMesh.transform.localPosition = Vector3.zero;
        keyMesh.transform.localScale = Vector3.one;
        Object.DestroyImmediate(keyMesh.GetComponent<BoxCollider>());

        // Trigger sphere collider on root
        var keySphereCol = keyGO.AddComponent<SphereCollider>();
        keySphereCol.isTrigger = true;
        keySphereCol.radius = 1.2f;

        // KeyPickup component
        var kp = keyGO.AddComponent<KeyPickup>();
        var kpSO = new SerializedObject(kp);
        kpSO.FindProperty("_keyId").stringValue = "door_A";
        kpSO.ApplyModifiedPropertiesWithoutUndo();

        // 3 — Door (tall cube blocking path, 10m away)
        var doorGO = new GameObject("Door_A");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(doorGO, scene);
        doorGO.transform.position = new Vector3(10, 1.5f, 0);
        doorGO.transform.localScale = new Vector3(2f, 3f, 0.3f);

        // Visual mesh
        var doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorMesh.name = "Mesh";
        doorMesh.transform.SetParent(doorGO.transform);
        doorMesh.transform.localPosition = Vector3.zero;
        doorMesh.transform.localScale = Vector3.one;
        Object.DestroyImmediate(doorMesh.GetComponent<BoxCollider>());

        // Blocking collider on root
        var doorCol = doorGO.AddComponent<BoxCollider>();
        doorCol.isTrigger = false;

        // Door component
        var door = doorGO.AddComponent<Door>();
        var doorSO = new SerializedObject(door);
        doorSO.FindProperty("_requiredKeyId").stringValue = "door_A";
        doorSO.FindProperty("_openDistance").floatValue   = 3.0f;
        doorSO.FindProperty("_openDuration").floatValue   = 0.8f;
        doorSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupKeyDoorTest] Done. Player tagged, Key_A at (5,0.5,0), Door_A at (10,1.5,0). Walk right to collect key and open door.");
    }
}
