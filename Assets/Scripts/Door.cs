using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    [SerializeField] private string _requiredKeyId;
    [SerializeField] private float  _openDistance  = 3.0f;
    [SerializeField] private float  _openDuration  = 0.8f;

    private enum DoorState { Locked, Opening, Open }
    private DoorState _state      = DoorState.Locked;
    private Collider  _collider;
    private bool      _subscribed;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_requiredKeyId))
            Debug.LogWarning($"[Door] _requiredKeyId is empty on '{name}'. Set it to match the corresponding KeyPickup._keyId.", this);

        if (_openDuration < 0.05f)
            _openDuration = 0.05f;
    }

    private void OnEnable()
    {
        Subscribe(); // succeeds if WorldStateManager.Awake already ran; Start() covers the race case
    }

    private void OnDisable()
    {
        if (!_subscribed || WorldStateManager.Instance == null) return;
        WorldStateManager.Instance.OnDoorUnlocked -= HandleDoorUnlocked;
        _subscribed = false;
    }

    private void Start()
    {
        // Fallback: subscribe if OnEnable fired before WorldStateManager.Awake set Instance
        Subscribe();

        if (WorldStateManager.Instance == null) return;
        if (WorldStateManager.Instance.IsDoorUnlocked(_requiredKeyId))
            InstantOpen();
    }

    private void Subscribe()
    {
        if (_subscribed || WorldStateManager.Instance == null) return;
        WorldStateManager.Instance.OnDoorUnlocked += HandleDoorUnlocked;
        _subscribed = true;
    }

    private void HandleDoorUnlocked(string keyId)
    {
        if (keyId != _requiredKeyId) return;
        if (_state == DoorState.Opening || _state == DoorState.Open) return;

        _state = DoorState.Opening;
        _collider.enabled = false;
        StartCoroutine(OpenCoroutine());
    }

    private IEnumerator OpenCoroutine()
    {
        float   elapsed  = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 endPos   = startPos + Vector3.up * _openDistance;

        while (elapsed < _openDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(elapsed / _openDuration));
            yield return null;
        }

        transform.localPosition = endPos;
        _state = DoorState.Open;
    }

    private void InstantOpen()
    {
        _state = DoorState.Open;
        _collider.enabled = false;
        transform.localPosition += Vector3.up * _openDistance;
    }
}
