using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public event Action<string> OnSceneLoaded;

    private bool _isLoading;
    private string _currentGameplayScene;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] LoadScene called with empty scene name.");
            return;
        }
        if (_isLoading)
        {
            Debug.LogWarning($"[SceneLoader] Already loading. Request for '{sceneName}' dropped.");
            return;
        }
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;

        if (!string.IsNullOrEmpty(_currentGameplayScene))
        {
            var unloadOp = SceneManager.UnloadSceneAsync(_currentGameplayScene);
            if (unloadOp != null)
                while (!unloadOp.isDone)
                    yield return null;
        }

        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        _currentGameplayScene = sceneName;
        _isLoading = false;
        OnSceneLoaded?.Invoke(sceneName);
    }
}
