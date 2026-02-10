using UnityEngine;

[DisallowMultipleComponent] // Prevents adding multiple Spawner components to the same GameObject
public class Spawner : MonoBehaviour
{
    public Transform spawnLocation;
    // Array of prefabs to spawn from
    public GameObject[] prefabs;

    // Scale factor to apply to the spawned object
    public float scale = 1f;

    void Start()
    {
        // If no spawn location assigned, use the current GameObject's transform as spawn point
        if (spawnLocation == null)
        {
            spawnLocation = transform;
        }
    }

    /// <summary>
    /// Spawns a random prefab from the array at the spawn location with specified scale.
    /// </summary>
    public void Spawn()
    {
        // Only proceed if this component is enabled
        if (enabled)
        {
            // Ensure spawnLocation is assigned, fallback to this transform if not
            if (spawnLocation == null)
            {
                spawnLocation = transform;
            }

            GameObject prefab;

            // Select a random prefab if any are assigned
            if (prefabs.Length > 0)
            {
                prefab = prefabs[Random.Range(0, prefabs.Length)];

                // Warn and exit if the selected prefab is null
                if (prefab == null)
                {
                    Debug.LogWarning("Prefab is not assigned!");
                    return;
                }
            }
            else
            {
                // Warn and exit if no prefabs have been assigned
                Debug.LogWarning("Prefabs are not assigned");
                return;
            }

            // Instantiate the selected prefab at the spawn location's position and rotation
            GameObject instance = Instantiate(prefab, spawnLocation.position, spawnLocation.rotation);

            // Adjust the scale of the spawned instance relative to the original prefab's scale
            instance.transform.localScale = prefab.transform.localScale * scale;

            // Assign the spawned instance the same name as the prefab for clarity
            instance.name = prefab.name;
        }
    }
}
