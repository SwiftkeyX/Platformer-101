using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot editor script: creates InputReader.asset and assigns GameInput.inputactions to it.
/// Safe to run multiple times — skips creation if the asset already exists.
/// </summary>
public class CreateInputAssets
{
    public static void Execute()
    {
        const string actionAssetPath  = "Assets/Input/GameInput.inputactions";
        const string inputReaderPath  = "Assets/Input/InputReader.asset";

        // Load the InputActionAsset
        var actionAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(actionAssetPath);
        if (actionAsset == null)
        {
            Debug.LogError("[CreateInputAssets] Could not load GameInput.inputactions at: " + actionAssetPath);
            return;
        }

        // Create InputReader.asset if it doesn't exist
        var existing = AssetDatabase.LoadAssetAtPath<InputReader>(inputReaderPath);
        if (existing != null)
        {
            Debug.Log("[CreateInputAssets] InputReader.asset already exists — skipping creation.");
        }
        else
        {
            var inputReader = ScriptableObject.CreateInstance<InputReader>();
            AssetDatabase.CreateAsset(inputReader, inputReaderPath);
            Debug.Log("[CreateInputAssets] InputReader.asset created at " + inputReaderPath);
        }

        // Assign the InputActionAsset reference via SerializedObject
        var asset = AssetDatabase.LoadAssetAtPath<InputReader>(inputReaderPath);
        if (asset != null)
        {
            var so = new SerializedObject(asset);
            var prop = so.FindProperty("_inputActions");
            if (prop != null)
            {
                prop.objectReferenceValue = actionAsset;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                Debug.Log("[CreateInputAssets] Assigned GameInput.inputactions to InputReader._inputActions.");
            }
            else
            {
                Debug.LogWarning("[CreateInputAssets] Could not find '_inputActions' serialized property on InputReader.");
            }
        }

        AssetDatabase.Refresh();
    }
}
