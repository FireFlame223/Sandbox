using UnityEngine;

/// <summary>
/// Synchronizes the visual wheel transform with its corresponding WheelCollider's position and rotation.
/// </summary>
[DisallowMultipleComponent]
public class Wheel : MonoBehaviour
{
    private WheelCollider wheelCollider;

    /// <summary>
    /// Cache the WheelCollider component from the parent object at the start.
    /// </summary>
    private void Start()
    {
        wheelCollider = GetComponentInParent<WheelCollider>();
    }

    /// <summary>
    /// Updates the wheel's position and rotation to match the WheelCollider's physical state.
    /// Called once per frame, after all Update methods have been processed.
    /// </summary>
    private void LateUpdate()
    {
        if (!enabled) return;

        if (wheelCollider != null && wheelCollider.enabled)
        {
            // Retrieve the world position and rotation from the WheelCollider
            wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);

            // Apply the position and rotation to the visual wheel transform
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
