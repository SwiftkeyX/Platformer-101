using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-level singleton that is the single source of truth for key/door state within a map.
/// Lives in each map scene — NOT in Bootstrap. State resets automatically on scene reload.
/// </summary>
public class WorldStateManager : MonoBehaviour
{
    // ── Static Instance (scene-level, not persistent) ──────────────────────
    public static WorldStateManager Instance { get; private set; }

    // ── State ───────────────────────────────────────────────────────────────
    private readonly HashSet<string> _collectedKeys = new HashSet<string>();
    private readonly HashSet<string> _unlockedDoors = new HashSet<string>();

    // ── Events ──────────────────────────────────────────────────────────────
    public event Action<string> OnKeyCollected;
    public event Action<string> OnDoorUnlocked;

    // ── Read-only collection properties (for DebugOverlay) ─────────────────
    public IReadOnlyCollection<string> CollectedKeys => _collectedKeys;
    public IReadOnlyCollection<string> UnlockedDoors => _unlockedDoors;

    // ── Lifecycle ───────────────────────────────────────────────────────────
    private void Awake()
    {
        // Duplicate guard — destroy any extra instance that appears in the same scene
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[WorldStateManager] Duplicate instance detected — destroying extra.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // Clear static reference so it returns null after the map scene unloads
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Records that a key has been collected and fires the corresponding door-unlock event.
    /// Idempotent — calling with the same keyId more than once has no effect after the first call.
    /// </summary>
    public void CollectKey(string keyId)
    {
        if (_collectedKeys.Contains(keyId))
        {
            return; // Already collected — idempotent, no double-fire
        }

        _collectedKeys.Add(keyId);
        OnKeyCollected?.Invoke(keyId);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[WorldStateManager] Key collected: '{keyId}'");
#endif

        // Design: one key unlocks the door with the matching id (1:1 pairing by convention).
        _unlockedDoors.Add(keyId);
        OnDoorUnlocked?.Invoke(keyId);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[WorldStateManager] Door unlocked: '{keyId}'");
#endif
    }

    /// <summary>Returns true if the specified key has been collected.</summary>
    public bool IsKeyCollected(string keyId) => _collectedKeys.Contains(keyId);

    /// <summary>Returns true if the door corresponding to this keyId has been unlocked.</summary>
    public bool IsDoorUnlocked(string keyId) => _unlockedDoors.Contains(keyId);
}
