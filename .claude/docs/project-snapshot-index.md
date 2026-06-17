# Project Snapshot Index
Last updated: 2026-06-17

## Scenes
| Scene | Path | Notes |
|---|---|---|
| Bootstrap | Assets/Scenes/Bootstrap.unity | Build index 0 — persistent; never unloaded; holds GameManager + SceneLoader |
| MainMenu | Assets/Scenes/MainMenu.unity | Loaded additively by Bootstrap on startup; Canvas/MapSelector wired to Btn_MapA/B/C/Quit; EventSystem uses InputSystemUIInputModule |
| SampleScene | Assets/Scenes/SampleScene.unity | Canonical Map A gameplay scene (registered in Build Settings); loaded by MapSelector.LoadMapA() via SceneLoader; contains Player, Ground, Key_A, Door_A, Main Camera, WorldStateManager, DebugOverlayCanvas |

## Scripts
| Script | Path | Attached to |
|---|---|---|
| Bootstrap | Assets/Scripts/Bootstrap.cs | Bootstrap.unity root GameObject 'Bootstrap' |
| GameManager | Assets/Scripts/GameManager.cs | Bootstrap/GameManager |
| SceneLoader | Assets/Scripts/SceneLoader.cs | Bootstrap/SceneLoader |
| InputReader | Assets/Scripts/InputReader.cs | ScriptableObject — Assets/Input/InputReader.asset |
| PlayerController | Assets/Scripts/PlayerController.cs | SampleScene/Player (+ CharacterController); _inputReader → InputReader.asset, _data → PlayerData.asset, _cameraController → Main Camera; state machine host — delegates Update to _currentState |
| PlayerStateBase | Assets/Scripts/Player/PlayerStateBase.cs | Abstract base — OnEnter/Update/OnExit; no scene attachment |
| IdleState | Assets/Scripts/Player/IdleState.cs | Grounded, no input; resets _hasDoubleJump on entry |
| WalkState | Assets/Scripts/Player/WalkState.cs | Grounded, moving at MoveSpeed; no sprint |
| RunState | Assets/Scripts/Player/RunState.cs | Grounded, moving at MoveSpeed × RunMultiplier |
| AirborneState | Assets/Scripts/Player/AirborneState.cs | Airborne — manages coyote window, double jump, air control |
| DashingState | Assets/Scripts/Player/DashingState.cs | Dash active — locks horizontal to DashSpeed, suppresses gravity |
| PlayerData | Assets/Scripts/PlayerData.cs | ScriptableObject — Assets/Data/PlayerData.asset |
| CameraController | Assets/Scripts/CameraController.cs | SampleScene/Main Camera; _playerTransform → Player, _data → CameraData.asset |
| CameraData | Assets/Scripts/CameraData.cs | ScriptableObject — Assets/Data/CameraData.asset |
| WorldStateManager | Assets/Scripts/WorldStateManager.cs | SampleScene/WorldStateManager |
| KeyPickup | Assets/Scripts/KeyPickup.cs | SampleScene/Key_A (Mesh is a child visual only) |
| Door | Assets/Scripts/Door.cs | SampleScene/Door_A (Mesh is a child visual only) |
| MapSelector | Assets/Scripts/MapSelector.cs | MainMenu/Canvas/MapSelector; _mapASceneName = "SampleScene" (test wiring), _mapBSceneName = "Map_B", _mapCSceneName = "Map_C" (Map_B/Map_C scenes do not exist yet) |
| DebugOverlay | Assets/Scripts/DebugOverlay.cs | SampleScene/DebugOverlayCanvas; reads SampleScene/DebugOverlayCanvas/Panel/KeyListText + DoorListText |
| GameInput | Assets/Input/GameInput.cs | Auto-generated from GameInput.inputactions |

## ScriptableObject Assets
| Asset | Path | Purpose |
|---|---|---|
| InputReader | Assets/Input/InputReader.asset | Assign to PlayerController via [SerializeField] |
| GameInput (actions) | Assets/Input/GameInput.inputactions | Move/Jump/Interact bindings — WASD + gamepad |
| PlayerData | Assets/Data/PlayerData.asset | Assign to PlayerController via [SerializeField] |
| CameraData | Assets/Data/CameraData.asset | Assign to CameraController via [SerializeField] |

## Prefabs
| Prefab | Path |
|---|---|
| None yet | — |

## Editor Scripts
| Script | Path | Purpose |
|---|---|---|
| CreateBootstrapScene | Assets/Scripts/Editor/CreateBootstrapScene.cs | One-time setup — created Bootstrap.unity |
| CreateInputAssets | Assets/Scripts/Editor/CreateInputAssets.cs | One-time setup — created Input System assets |
| SetupInputSystem | Assets/Scripts/Editor/SetupInputSystem.cs | One-time setup — configured Input System package |
| FixInputActions | Assets/Scripts/Editor/FixInputActions.cs | One-time fix — corrected GameInput.inputactions bindings |
| RewireBootstrapScene | Assets/Scripts/Editor/RewireBootstrapScene.cs | One-time setup — rewired Bootstrap scene references |
| AddWorldStateManagerToSampleScene | Assets/Scripts/Editor/AddWorldStateManagerToSampleScene.cs | One-time setup — added WorldStateManager to SampleScene |
| SetupPlayerTest | Assets/Scripts/Editor/SetupPlayerTest.cs | One-time setup — created PlayerData.asset, wired Player/PlayerController/CameraController refs |
| CreateCameraData | Assets/Scripts/Editor/CreateCameraData.cs | One-time setup — created CameraData.asset |
| SetupKeyDoorTest | Assets/Scripts/Editor/SetupKeyDoorTest.cs | One-time setup — created Key_A (KeyPickup) and Door_A (Door) |
| SetupMainMenu | Assets/Scripts/Editor/SetupMainMenu.cs | One-time setup — created MainMenu.unity, Canvas/MapSelector, EventSystem |
| SetupDebugOverlay | Assets/Scripts/Editor/SetupDebugOverlay.cs | One-time setup — created DebugOverlayCanvas/Panel/KeyListText+DoorListText, attached DebugOverlay |
| FixMainMenuInputModule | Assets/Scripts/Editor/FixMainMenuInputModule.cs | One-time fix — replaced legacy StandaloneInputModule with InputSystemUIInputModule on MainMenu/EventSystem |
| MergeSampleScenes | Assets/Scripts/Editor/MergeSampleScenes.cs | One-time fix — merged stray root Assets/SampleScene.unity content into canonical Assets/Scenes/SampleScene.unity (stray file since deleted) |
| SimulateMapAClick | Assets/Scripts/Editor/SimulateMapAClick.cs | Test utility — simulates clicking Map A in Play mode by calling MapSelector.LoadMapA() directly |
| GenerateProjectSnapshot | Assets/Scripts/Editor/GenerateProjectSnapshot.cs | Regenerates this file |
