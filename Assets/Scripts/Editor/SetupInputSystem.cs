using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One-shot editor script: rebuilds GameInput.inputactions with the correct
/// Gameplay action map (Move/Jump/Interact) and creates InputReader.asset
/// with the InputActionAsset reference assigned.
/// Safe to run multiple times.
/// </summary>
public class SetupInputSystem
{
    private const string ActionAssetPath  = "Assets/Input/GameInput.inputactions";
    private const string InputReaderPath  = "Assets/Input/InputReader.asset";

    public static void Execute()
    {
        RebuildInputActionsAsset();
        AssetDatabase.Refresh();
        CreateInputReaderAsset();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SetupInputSystem] Done. InputReader.asset is ready.");
    }

    // ── Step 1: write the correct JSON for the .inputactions file ─────────────
    private static void RebuildInputActionsAsset()
    {
        // Build the asset programmatically using InputActionAsset API
        var asset = InputActionAsset.FromJson(BuildInputActionsJson());
        asset.name = "GameInput";

        // Overwrite the file on disk
        string fullPath = Path.GetFullPath(ActionAssetPath);
        File.WriteAllText(fullPath, asset.ToJson());

        Debug.Log("[SetupInputSystem] Wrote GameInput.inputactions to " + fullPath);
        AssetDatabase.ImportAsset(ActionAssetPath, ImportAssetOptions.ForceUpdate);
    }

    // ── Step 2: create or update InputReader.asset ────────────────────────────
    private static void CreateInputReaderAsset()
    {
        var actionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionAssetPath);
        if (actionAsset == null)
        {
            Debug.LogError("[SetupInputSystem] GameInput.inputactions not found after import.");
            return;
        }

        // Verify the Gameplay map survived
        var map = actionAsset.FindActionMap("Gameplay");
        if (map == null)
        {
            Debug.LogError("[SetupInputSystem] 'Gameplay' action map not found in GameInput.inputactions.");
            return;
        }
        Debug.Log("[SetupInputSystem] Gameplay map OK. Actions: " + map.actions.Count);

        // Create InputReader.asset if it does not exist
        var reader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
        if (reader == null)
        {
            reader = ScriptableObject.CreateInstance<InputReader>();
            AssetDatabase.CreateAsset(reader, InputReaderPath);
            Debug.Log("[SetupInputSystem] Created InputReader.asset at " + InputReaderPath);
        }
        else
        {
            Debug.Log("[SetupInputSystem] InputReader.asset already exists — updating reference.");
        }

        // Assign _inputActions via SerializedObject
        var so = new SerializedObject(reader);
        var prop = so.FindProperty("_inputActions");
        if (prop != null)
        {
            prop.objectReferenceValue = actionAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(reader);
            Debug.Log("[SetupInputSystem] Assigned GameInput.inputactions to InputReader._inputActions.");
        }
        else
        {
            Debug.LogError("[SetupInputSystem] Could not find '_inputActions' property on InputReader.");
        }
    }

    // ── JSON builder ───────────────────────────────────────────────────────────
    private static string BuildInputActionsJson()
    {
        // Build via InputActionAsset API so the schema version is always correct
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.name = "GameInput";

        var gameplay = asset.AddActionMap("Gameplay");

        // Move — Value/Vector2
        var move = gameplay.AddAction("Move", InputActionType.Value);
        move.expectedControlType = "Vector2";
        // WASD composite
        move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        // Arrow keys composite
        move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        // Gamepad left stick
        move.AddBinding("<Gamepad>/leftStick");

        // Jump — Button
        var jump = gameplay.AddAction("Jump", InputActionType.Button);
        jump.AddBinding("<Keyboard>/space");
        jump.AddBinding("<Gamepad>/buttonSouth");

        // Interact — Button
        var interact = gameplay.AddAction("Interact", InputActionType.Button);
        interact.AddBinding("<Keyboard>/e");
        interact.AddBinding("<Gamepad>/buttonWest");

        string json = asset.ToJson();
        Object.DestroyImmediate(asset);
        return json;
    }
}
