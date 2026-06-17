using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class GenerateProjectSnapshot
{
    public static void Execute()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Project Snapshot Index");
        sb.AppendLine($"Last updated: 2026-06-16");
        sb.AppendLine();
        sb.AppendLine("## Scenes");
        sb.AppendLine("| Scene | Path | Notes |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| Bootstrap | Assets/Scenes/Bootstrap.unity | Build index 0 — persistent; never unloaded; holds GameManager + SceneLoader |");
        sb.AppendLine("| MainMenu | Assets/Scenes/MainMenu.unity | Loaded additively by Bootstrap on startup; Canvas/MapSelector wired to Btn_MapA/B/C/Quit; EventSystem uses InputSystemUIInputModule |");
        sb.AppendLine("| SampleScene | Assets/Scenes/SampleScene.unity | Canonical Map A gameplay scene (registered in Build Settings); loaded by MapSelector.LoadMapA() via SceneLoader; contains Player, Ground, Key_A, Door_A, Main Camera, WorldStateManager, DebugOverlayCanvas |");
        sb.AppendLine();
        sb.AppendLine("## Scripts");
        sb.AppendLine("| Script | Path | Attached to |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| Bootstrap | Assets/Scripts/Bootstrap.cs | Bootstrap.unity root GameObject 'Bootstrap' |");
        sb.AppendLine("| GameManager | Assets/Scripts/GameManager.cs | Bootstrap/GameManager |");
        sb.AppendLine("| SceneLoader | Assets/Scripts/SceneLoader.cs | Bootstrap/SceneLoader |");
        sb.AppendLine("| InputReader | Assets/Scripts/InputReader.cs | ScriptableObject — Assets/Input/InputReader.asset |");
        sb.AppendLine("| PlayerStateManager | Assets/Scripts/PlayerStateManager.cs | SampleScene/Player (+ CharacterController); _inputReader → InputReader.asset, _data → PlayerData.asset, _cameraController → Main Camera; pure shell — state machine host only |");
        sb.AppendLine("| PlayerData | Assets/Scripts/PlayerData.cs | ScriptableObject — Assets/Data/PlayerData.asset |");
        sb.AppendLine("| CameraController | Assets/Scripts/CameraController.cs | SampleScene/Main Camera; _playerTransform → Player, _data → CameraData.asset |");
        sb.AppendLine("| CameraData | Assets/Scripts/CameraData.cs | ScriptableObject — Assets/Data/CameraData.asset |");
        sb.AppendLine("| WorldStateManager | Assets/Scripts/WorldStateManager.cs | SampleScene/WorldStateManager |");
        sb.AppendLine("| KeyPickup | Assets/Scripts/KeyPickup.cs | SampleScene/Key_A (Mesh is a child visual only) |");
        sb.AppendLine("| Door | Assets/Scripts/Door.cs | SampleScene/Door_A (Mesh is a child visual only) |");
        sb.AppendLine("| MapSelector | Assets/Scripts/MapSelector.cs | MainMenu/Canvas/MapSelector; _mapASceneName = \"SampleScene\" (test wiring), _mapBSceneName = \"Map_B\", _mapCSceneName = \"Map_C\" (Map_B/Map_C scenes do not exist yet) |");
        sb.AppendLine("| DebugOverlay | Assets/Scripts/DebugOverlay.cs | SampleScene/DebugOverlayCanvas; reads SampleScene/DebugOverlayCanvas/Panel/KeyListText + DoorListText |");
        sb.AppendLine("| GameInput | Assets/Input/GameInput.cs | Auto-generated from GameInput.inputactions |");
        sb.AppendLine();
        sb.AppendLine("## ScriptableObject Assets");
        sb.AppendLine("| Asset | Path | Purpose |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| InputReader | Assets/Input/InputReader.asset | Assign to PlayerStateManager via [SerializeField] |");
        sb.AppendLine("| GameInput (actions) | Assets/Input/GameInput.inputactions | Move/Jump/Interact bindings — WASD + gamepad |");
        sb.AppendLine("| PlayerData | Assets/Data/PlayerData.asset | Assign to PlayerStateManager via [SerializeField] |");
        sb.AppendLine("| CameraData | Assets/Data/CameraData.asset | Assign to CameraController via [SerializeField] |");
        sb.AppendLine();
        sb.AppendLine("## Prefabs");
        sb.AppendLine("| Prefab | Path |");
        sb.AppendLine("|---|---|");
        sb.AppendLine("| None yet | — |");
        sb.AppendLine();
        sb.AppendLine("## Editor Scripts");
        sb.AppendLine("| Script | Path | Purpose |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| CreateBootstrapScene | Assets/Scripts/Editor/CreateBootstrapScene.cs | One-time setup — created Bootstrap.unity |");
        sb.AppendLine("| CreateInputAssets | Assets/Scripts/Editor/CreateInputAssets.cs | One-time setup — created Input System assets |");
        sb.AppendLine("| SetupInputSystem | Assets/Scripts/Editor/SetupInputSystem.cs | One-time setup — configured Input System package |");
        sb.AppendLine("| FixInputActions | Assets/Scripts/Editor/FixInputActions.cs | One-time fix — corrected GameInput.inputactions bindings |");
        sb.AppendLine("| RewireBootstrapScene | Assets/Scripts/Editor/RewireBootstrapScene.cs | One-time setup — rewired Bootstrap scene references |");
        sb.AppendLine("| AddWorldStateManagerToSampleScene | Assets/Scripts/Editor/AddWorldStateManagerToSampleScene.cs | One-time setup — added WorldStateManager to SampleScene |");
        sb.AppendLine("| SetupPlayerTest | Assets/Scripts/Editor/SetupPlayerTest.cs | One-time setup — created PlayerData.asset, wired Player/PlayerStateManager/CameraController refs |");
        sb.AppendLine("| CreateCameraData | Assets/Scripts/Editor/CreateCameraData.cs | One-time setup — created CameraData.asset |");
        sb.AppendLine("| SetupKeyDoorTest | Assets/Scripts/Editor/SetupKeyDoorTest.cs | One-time setup — created Key_A (KeyPickup) and Door_A (Door) |");
        sb.AppendLine("| SetupMainMenu | Assets/Scripts/Editor/SetupMainMenu.cs | One-time setup — created MainMenu.unity, Canvas/MapSelector, EventSystem |");
        sb.AppendLine("| SetupDebugOverlay | Assets/Scripts/Editor/SetupDebugOverlay.cs | One-time setup — created DebugOverlayCanvas/Panel/KeyListText+DoorListText, attached DebugOverlay |");
        sb.AppendLine("| FixMainMenuInputModule | Assets/Scripts/Editor/FixMainMenuInputModule.cs | One-time fix — replaced legacy StandaloneInputModule with InputSystemUIInputModule on MainMenu/EventSystem |");
        sb.AppendLine("| MergeSampleScenes | Assets/Scripts/Editor/MergeSampleScenes.cs | One-time fix — merged stray root Assets/SampleScene.unity content into canonical Assets/Scenes/SampleScene.unity (stray file since deleted) |");
        sb.AppendLine("| SimulateMapAClick | Assets/Scripts/Editor/SimulateMapAClick.cs | Test utility — simulates clicking Map A in Play mode by calling MapSelector.LoadMapA() directly |");
        sb.AppendLine("| GenerateProjectSnapshot | Assets/Scripts/Editor/GenerateProjectSnapshot.cs | Regenerates this file |");

        var path = ".claude/docs/project-snapshot-index.md";
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[GenerateProjectSnapshot] Snapshot written to " + path);
    }
}
