using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 previousPosition;
    private Quaternion previousRotation;

    public float castHeight = 0.3f; // Height of the boxcast above the platform
    public LayerMask characterLayerMask = ~0; // Layer mask for character detection
    private Vector3 boxSize;

    // Store the character's previous local positions and world positions relative to the platform
    private Dictionary<CharacterController, Vector3> characterLocalPositions = new Dictionary<CharacterController, Vector3>();
    private Dictionary<CharacterController, Vector3> characterWorldPositions = new Dictionary<CharacterController, Vector3>();

    void Start()
    {
        // Store the initial position and rotation of the platform
        previousPosition = transform.position;
        previousRotation = transform.rotation;

        // Initialize boxSize from the platform's local scale
        UpdateBoxSize();
    }

    void LateUpdate()
    {
        // Update boxSize dynamically in case the platform size changes
        UpdateBoxSize();

        // Perform BoxCast to find colliders above the platform
        Vector3 boxCenter = transform.position + new Vector3(0f, castHeight + (boxSize.y / 2), 0f);
        
        // Use the platform's current rotation in the OverlapBox method
        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation, characterLayerMask);

        // Track characters in the area and update their positions/rotations
        foreach (Collider hit in hits)
        {
            CharacterController characterController = hit.GetComponent<CharacterController>();
            if (characterController != null)
            {
                if (!characterLocalPositions.ContainsKey(characterController))
                {
                    // When first detected, store the character's local and world positions relative to the platform
                    Vector3 localPosition = transform.InverseTransformPoint(characterController.transform.position);
                    characterLocalPositions.Add(characterController, localPosition);
                    characterWorldPositions.Add(characterController, characterController.transform.position);
                }

                // Update character's position based on platform movement and rotation
                MoveAndRotateCharacter(characterController);
            }
        }

        // Remove characters that are no longer in contact with the platform
        List<CharacterController> charactersToRemove = new List<CharacterController>();
        foreach (var character in characterLocalPositions.Keys)
        {
            if (!CharacterStillInContact(character, hits))
            {
                charactersToRemove.Add(character);
            }
        }

        foreach (var character in charactersToRemove)
        {
            characterLocalPositions.Remove(character);
            characterWorldPositions.Remove(character);
        }

        // Store the current platform position and rotation for the next frame
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    void MoveAndRotateCharacter(CharacterController characterController)
    {
        // Disable the CharacterController to adjust its position and rotation manually
        characterController.enabled = false;

        // Get the stored local position of the character relative to the platform
        Vector3 previousLocalPosition = characterLocalPositions[characterController];
        Vector3 previousWorldPosition = characterWorldPositions[characterController];

        // Calculate the new world position of the character based on the platform's updated transform
        Vector3 newWorldPosition = transform.TransformPoint(previousLocalPosition);

        // Calculate the delta between the new world position and the previous world position
        Vector3 positionDelta = newWorldPosition - previousWorldPosition;

        // Apply the delta movement to the character's position
        characterController.transform.position += positionDelta;

        // Apply the platform's rotation to the character's rotation
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(previousRotation);
        characterController.transform.rotation = rotationDelta * characterController.transform.rotation;

        // Re-enable the CharacterController after applying the transformation
        characterController.enabled = true;

        // Update the local and world positions of the character relative to the platform for the next frame
        characterLocalPositions[characterController] = transform.InverseTransformPoint(characterController.transform.position);
        characterWorldPositions[characterController] = characterController.transform.position;
    }

    bool CharacterStillInContact(CharacterController character, Collider[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.GetComponent<CharacterController>() == character)
                return true;
        }
        return false;
    }

    void UpdateBoxSize()
    {
        // Derive the box size from the platform's local scale, with a small height adjustment for casting
        boxSize = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
    }

    void OnDrawGizmos()
    {
        // Visualize the BoxCast in the editor
        Gizmos.color = Color.green;
        Vector3 boxCenter = transform.position + new Vector3(0f, castHeight + (boxSize.y / 2), 0f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
