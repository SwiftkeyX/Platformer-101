using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }

    [SerializeField] private string _startupScene;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // No DontDestroyOnLoad — Bootstrap.unity is never unloaded; all other scenes load additively.
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(_startupScene))
            SceneLoader.Instance.LoadScene(_startupScene);
    }
}
