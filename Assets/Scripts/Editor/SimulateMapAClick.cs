using UnityEditor;
using UnityEngine;

public static class SimulateMapAClick
{
    public static void Execute()
    {
        var selector = Object.FindFirstObjectByType<MapSelector>();
        if (selector == null) { Debug.LogError("[SimulateMapAClick] No MapSelector found in play mode — is MainMenu loaded?"); return; }
        selector.LoadMapA();
        Debug.Log("[SimulateMapAClick] Called MapSelector.LoadMapA().");
    }
}
