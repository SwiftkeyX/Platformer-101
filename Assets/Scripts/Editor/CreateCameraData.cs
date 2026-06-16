using UnityEditor;
using UnityEngine;

public static class CreateCameraData
{
    public static void Execute()
    {
        const string path = "Assets/Data/CameraData.asset";
        if (AssetDatabase.LoadAssetAtPath<CameraData>(path) != null)
        {
            Debug.Log("[CreateCameraData] CameraData.asset already exists — skipped.");
            return;
        }
        var asset = ScriptableObject.CreateInstance<CameraData>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[CreateCameraData] Created CameraData.asset at " + path);
    }
}
