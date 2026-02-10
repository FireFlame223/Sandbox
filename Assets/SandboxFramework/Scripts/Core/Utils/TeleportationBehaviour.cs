using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.TextCore.Text;

public class TeleportationBehaviour : MonoBehaviour
{
    // Reference to the Cinemachine virtual camera controlling the view
    public CinemachineCamera cinemachineCamera;
    // Reference to the CharacterController component for the player
    public CharacterController characterController;

    // Target transform to teleport to, set when teleportation is requested
    private Transform teleportationTarget = null;

    /// <summary>
    /// Initialization check to ensure a CharacterController is assigned.
    /// If missing, logs a warning and destroys this component.
    /// </summary>
    void Start()
    {
        if (characterController == null)
        {
            characterController = FindAnyObjectByType<CharacterController>();
        }
        if (characterController == null)
        {
            Debug.LogWarning("No character controller assigned to TeleportBehaviour.");
            Destroy(this);
            return;
        }
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        }
    }

    /// <summary>
    /// Called every frame after all Updates.
    /// Executes the teleportation immediately if a target was set.
    /// Resets the teleportation target after teleporting.
    /// </summary>
    void LateUpdate() 
    {
        if (enabled && teleportationTarget != null) {
            TeleportImmediatelyTo(teleportationTarget);
            teleportationTarget = null;
        }
    }

    /// <summary>
    /// Public method to request teleportation to a target transform.
    /// Sets the target to be processed in the next LateUpdate.
    /// </summary>
    /// <param name="targetTransform">Transform to teleport to</param>
    public void TeleportTo(Transform targetTransform)
    {
        if (enabled)
            teleportationTarget = targetTransform;
    }

    /// <summary>
    /// Immediately teleports the character to the target position and rotation.
    /// Temporarily disables the CharacterController to avoid physics conflicts.
    /// Also warps the Cinemachine camera target to keep camera in sync.
    /// </summary>
    /// <param name="targetTransform">Transform to teleport to</param>
    private void TeleportImmediatelyTo(Transform targetTransform)
    {
        if (characterController != null)
        {
            // Store current camera follow position before teleport
            var currentCameraPosition = Vector3.zero;
            if (cinemachineCamera != null && cinemachineCamera.Follow != null)
            {
                currentCameraPosition = cinemachineCamera.Follow.transform.position;
            }

            // Disable CharacterController to safely change position
            characterController.enabled = false;

            // Set character position and rotation to target
            characterController.transform.position = targetTransform.position;
            characterController.transform.rotation = targetTransform.rotation;

            // Re-enable CharacterController after moving
            characterController.enabled = true;

            // Inform Cinemachine camera of the warp to avoid visual glitches
            if (cinemachineCamera != null && cinemachineCamera.Follow != null)
            {
                cinemachineCamera.OnTargetObjectWarped(
                    cinemachineCamera.Follow,
                    cinemachineCamera.Follow.transform.position - currentCameraPosition
                );
            }
        }
    }
}
