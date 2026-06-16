using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rewrites GameInput.inputactions with the canonical Gameplay action map:
/// Move (WASD + Arrow Keys + Left Stick), Jump (Space + South), Interact (E + West).
/// Removes DefaultAction leftover. Safe to run multiple times.
/// </summary>
public class FixInputActions
{
    private const string ActionAssetPath = "Assets/Input/GameInput.inputactions";
    private const string InputReaderPath = "Assets/Input/InputReader.asset";

    public static void Execute()
    {
        // Build the asset programmatically
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.name = "GameInput";

        var gameplay = asset.AddActionMap("Gameplay");

        // Move — Value/Vector2
        var move = gameplay.AddAction("Move", InputActionType.Value);
        move.expectedControlType = "Vector2";
        move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
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

        // Write to disk
        string fullPath = Path.GetFullPath(ActionAssetPath);
        File.WriteAllText(fullPath, json);
        AssetDatabase.ImportAsset(ActionAssetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Re-assign reference on InputReader.asset in case import changed the object
        var actionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionAssetPath);
        var reader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
        if (reader != null && actionAsset != null)
        {
            var so = new SerializedObject(reader);
            var prop = so.FindProperty("_inputActions");
            if (prop != null)
            {
                prop.objectReferenceValue = actionAsset;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(reader);
                AssetDatabase.SaveAssets();
            }
        }

        // Verify
        var verify = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionAssetPath);
        var map = verify?.FindActionMap("Gameplay");
        if (map != null)
            Debug.Log($"[FixInputActions] Done. Gameplay map has {map.actions.Count} actions: " +
                      string.Join(", ", System.Linq.Enumerable.Select(map.actions, a => a.name)));
        else
            Debug.LogError("[FixInputActions] Gameplay map not found after rewrite.");
    }
}
