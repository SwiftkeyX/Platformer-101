using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class SetupMainMenu
{
    public static void Execute()
    {
        // 1 — Create MainMenu.unity
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 2 — Canvas (Screen Space Overlay)
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 3 — EventSystem (required for button clicks)
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 4 — MapSelector GameObject (on Canvas)
        var selectorGO = new GameObject("MapSelector");
        selectorGO.transform.SetParent(canvasGO.transform, false);
        var selector = selectorGO.AddComponent<MapSelector>();

        // Set _mapASceneName = "SampleScene" for test (Map_A doesn't exist yet)
        var selectorSO = new SerializedObject(selector);
        selectorSO.FindProperty("_mapASceneName").stringValue = "SampleScene";
        selectorSO.FindProperty("_mapBSceneName").stringValue = "Map_B";
        selectorSO.FindProperty("_mapCSceneName").stringValue = "Map_C";
        selectorSO.ApplyModifiedPropertiesWithoutUndo();

        // 5 — Vertical Layout Group on MapSelector for auto-stacking
        var layout = selectorGO.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment      = TextAnchor.MiddleCenter;
        layout.childControlWidth   = true;
        layout.childControlHeight  = false;
        layout.childForceExpandWidth = true;
        layout.spacing             = 16;

        // Center the MapSelector rect
        var selectorRect = selectorGO.GetComponent<RectTransform>();
        selectorRect.anchorMin = new Vector2(0.3f, 0.3f);
        selectorRect.anchorMax = new Vector2(0.7f, 0.7f);
        selectorRect.offsetMin = Vector2.zero;
        selectorRect.offsetMax = Vector2.zero;

        // 6 — Buttons
        CreateButton(selectorGO, "Btn_MapA", "Map A",  selector, "LoadMapA");
        CreateButton(selectorGO, "Btn_MapB", "Map B",  selector, "LoadMapB");
        CreateButton(selectorGO, "Btn_MapC", "Map C",  selector, "LoadMapC");
        CreateButton(selectorGO, "Btn_Quit", "Quit",   selector, "Quit");

        // 7 — Save as Assets/Scenes/MainMenu.unity
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");

        // 8 — Add MainMenu to build settings (after Bootstrap=0, before SampleScene)
        var scenes = EditorBuildSettings.scenes;
        bool alreadyAdded = false;
        foreach (var s in scenes)
            if (s.path == "Assets/Scenes/MainMenu.unity") { alreadyAdded = true; break; }

        if (!alreadyAdded)
        {
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = scenes[0]; // Bootstrap stays at index 0
            newScenes[1] = new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true);
            for (int i = 1; i < scenes.Length; i++)
                newScenes[i + 1] = scenes[i];
            EditorBuildSettings.scenes = newScenes;
        }

        // 9 — Update Bootstrap._startupScene to "MainMenu"
        var bootstrapScene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Additive);
        foreach (var go in bootstrapScene.GetRootGameObjects())
        {
            var b = go.GetComponent<Bootstrap>();
            if (b == null) continue;
            var bso = new SerializedObject(b);
            bso.FindProperty("_startupScene").stringValue = "MainMenu";
            bso.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.SaveScene(bootstrapScene);
        EditorSceneManager.CloseScene(bootstrapScene, true);

        Debug.Log("[SetupMainMenu] MainMenu.unity created. Bootstrap now loads MainMenu. Map A is wired to SampleScene for testing.");
    }

    private static void CreateButton(GameObject parent, string goName, string label,
                                     MapSelector target, string methodName)
    {
        var btnGO  = new GameObject(goName);
        btnGO.transform.SetParent(parent.transform, false);
        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 50);

        var image  = btnGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var button = btnGO.AddComponent<Button>();

        // Add text child
        var txtGO  = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var text = txtGO.AddComponent<UnityEngine.UI.Text>();
        text.text      = label;
        text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize  = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color     = Color.white;

        // Wire onClick
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            button.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, methodName)
                as UnityEngine.Events.UnityAction
        );
    }
}
