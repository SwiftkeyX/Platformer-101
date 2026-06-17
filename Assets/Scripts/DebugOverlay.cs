#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private WorldStateManager _worldStateManager;
    [SerializeField] private TextMeshProUGUI   _keyListText;
    [SerializeField] private TextMeshProUGUI   _doorListText;
    [SerializeField] private GameObject        _panel;
    [SerializeField] private Key _toggleKey = Key.Backquote;
    [SerializeField] private Key _reloadKey = Key.R;

    private void OnEnable()
    {
        if (_worldStateManager == null) { Debug.LogError("[DebugOverlay] _worldStateManager is not assigned."); return; }
        _worldStateManager.OnKeyCollected += HandleKeyCollected;
        _worldStateManager.OnDoorUnlocked += HandleDoorUnlocked;
    }

    private void OnDisable()
    {
        if (_worldStateManager == null) return;
        _worldStateManager.OnKeyCollected -= HandleKeyCollected;
        _worldStateManager.OnDoorUnlocked -= HandleDoorUnlocked;
    }

    private void Start() => RefreshDisplay();

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[_toggleKey].wasPressedThisFrame && _panel != null)
            _panel.SetActive(!_panel.activeSelf);

        if (kb[_reloadKey].wasPressedThisFrame && SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleKeyCollected(string keyId) => RefreshDisplay();
    private void HandleDoorUnlocked(string keyId)  => RefreshDisplay();

    private void RefreshDisplay()
    {
        if (_worldStateManager == null) return;

        if (_keyListText == null)
            Debug.LogWarning("[DebugOverlay] _keyListText is not assigned.");
        else
        {
            string state = GameManager.Instance != null ? GameManager.Instance.CurrentState.ToString() : "—";
            string keys  = "State: " + state + "\n"
                         + "Keys: " + _worldStateManager.CollectedKeys.Count + "\n";
            foreach (string k in _worldStateManager.CollectedKeys) keys += "  • " + k + "\n";
            _keyListText.text = keys;
        }

        if (_doorListText == null)
            Debug.LogWarning("[DebugOverlay] _doorListText is not assigned.");
        else
        {
            string doors = "Doors: " + _worldStateManager.UnlockedDoors.Count + "\n";
            foreach (string d in _worldStateManager.UnlockedDoors) doors += "  • " + d + "\n";
            _doorListText.text = doors;
        }
    }
}
#endif
