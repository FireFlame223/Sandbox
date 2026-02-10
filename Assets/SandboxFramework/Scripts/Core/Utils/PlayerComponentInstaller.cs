using UnityEngine;
using System;
using UnityEngine.TextCore.Text;

[DisallowMultipleComponent]
public class PlayerComponentInstaller : MonoBehaviour
{
    [Tooltip("The player capsule (GameObject with CharacterController). If empty, it will be automatically found.")]
    public Transform playerCapsule;

    [Tooltip("List of scripts to add to the player capsule at runtime.")]
    public string[] scriptsToAdd;

    void Start()
    {
        // Search for the player capsule if no reference is set
        if (playerCapsule == null)
        {
            playerCapsule = FindAnyObjectByType<CharacterController>()?.gameObject.transform;
        }

        if (playerCapsule == null)
        {
            Debug.LogWarning("PlayerComponentInstaller: No playerCapsule found!");
            return;
        }

        foreach (var typeName in scriptsToAdd)
        {
            if (string.IsNullOrEmpty(typeName)) continue;

            // Get the Type from the string
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogWarning($"Type '{typeName}' not found. Did you include the namespace?");
                continue;
            }

            // Add the component if it doesn't already exist
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                Debug.LogWarning($"Type '{typeName}' is not a MonoBehaviour.");
                continue;
            }

            if (playerCapsule.GetComponent(type) == null)
            {
                playerCapsule.gameObject.AddComponent(type);
                Debug.Log($"Added {type.Name} to {playerCapsule.name}");
            }
        }
    }
}
