using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private string _keyId;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_keyId))
            Debug.LogWarning($"[KeyPickup] _keyId is empty on '{name}'. Set it to match the target Door's _requiredKeyId.", this);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (WorldStateManager.Instance == null)
        {
            Debug.LogError("[KeyPickup] WorldStateManager.Instance is null. Is Bootstrap loaded?");
            return;
        }

        WorldStateManager.Instance.CollectKey(_keyId);
        gameObject.SetActive(false);
    }
}
