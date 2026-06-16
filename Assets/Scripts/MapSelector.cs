using UnityEngine;

public class MapSelector : MonoBehaviour
{
    [SerializeField] private string _mapASceneName = "Map_A";
    [SerializeField] private string _mapBSceneName = "Map_B";
    [SerializeField] private string _mapCSceneName = "Map_C";

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_mapASceneName)) Debug.LogWarning("[MapSelector] _mapASceneName is empty.", this);
        if (string.IsNullOrEmpty(_mapBSceneName)) Debug.LogWarning("[MapSelector] _mapBSceneName is empty.", this);
        if (string.IsNullOrEmpty(_mapCSceneName)) Debug.LogWarning("[MapSelector] _mapCSceneName is empty.", this);
    }

    public void LoadMapA() => LoadMap(_mapASceneName);
    public void LoadMapB() => LoadMap(_mapBSceneName);
    public void LoadMapC() => LoadMap(_mapCSceneName);

    public void Quit() => Application.Quit();

    private void LoadMap(string sceneName)
    {
        if (GameManager.Instance == null) { Debug.LogError("[MapSelector] GameManager.Instance is null."); return; }
        if (SceneLoader.Instance == null) { Debug.LogError("[MapSelector] SceneLoader.Instance is null."); return; }
        GameManager.Instance.SetState(GameState.Playing);
        SceneLoader.Instance.LoadScene(sceneName);
    }
}
