using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class SetupDebugOverlay
{
    public static void Execute()
    {
        // Open SampleScene (should already be open, but ensure it)
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        // Find WorldStateManager in scene
        WorldStateManager wsm = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            wsm = go.GetComponent<WorldStateManager>();
            if (wsm != null) break;
        }
        if (wsm == null) { Debug.LogError("[SetupDebugOverlay] No WorldStateManager found in SampleScene."); return; }

        // Remove any existing DebugOverlay canvas
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "DebugOverlayCanvas") { Object.DestroyImmediate(go); break; }

        // Canvas
        var canvasGO = new GameObject("DebugOverlayCanvas");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel (top-left background)
        var panelGO   = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect  = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot     = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(280, 180);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.6f);

        // Layout group on panel
        var layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.padding  = new RectOffset(8, 8, 8, 8);
        layout.spacing  = 4;
        layout.childControlWidth   = true;
        layout.childControlHeight  = true;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        // Key list text
        var keyTxtGO   = new GameObject("KeyListText");
        keyTxtGO.transform.SetParent(panelGO.transform, false);
        var keyTxt = keyTxtGO.AddComponent<TextMeshProUGUI>();
        keyTxt.text       = "Keys: 0";
        keyTxt.fontSize   = 14;
        keyTxt.color      = Color.white;

        // Door list text
        var doorTxtGO  = new GameObject("DoorListText");
        doorTxtGO.transform.SetParent(panelGO.transform, false);
        var doorTxt = doorTxtGO.AddComponent<TextMeshProUGUI>();
        doorTxt.text      = "Doors: 0";
        doorTxt.fontSize  = 14;
        doorTxt.color     = Color.white;

        // DebugOverlay component on canvas root
        var overlay = canvasGO.AddComponent<DebugOverlay>();
        var oSO     = new SerializedObject(overlay);
        oSO.FindProperty("_worldStateManager").objectReferenceValue = wsm;
        oSO.FindProperty("_keyListText").objectReferenceValue       = keyTxt;
        oSO.FindProperty("_doorListText").objectReferenceValue      = doorTxt;
        oSO.FindProperty("_panel").objectReferenceValue             = panelGO;
        oSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupDebugOverlay] DebugOverlay added to SampleScene. Backtick toggles, R reloads scene.");
    }
}
