using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destroyer : MonoBehaviour
{
    // List to keep track of all GameObjects currently inside the trigger area
    private readonly List<GameObject> objectsInside = new List<GameObject>();

    /// <summary>
    /// Called when another collider enters the trigger attached to this GameObject.
    /// Adds the entering GameObject to the list if not already present.
    /// </summary>
    /// <param name="other">Collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only add objects if this component is enabled
        if (enabled)
        {
            // Add the GameObject to the list if not already inside
            if (!objectsInside.Contains(other.gameObject))
            {
                objectsInside.Add(other.gameObject);
            }
        }
    }

    /// <summary>
    /// Called when another collider exits the trigger attached to this GameObject.
    /// Removes the exiting GameObject from the list if present.
    /// </summary>
    /// <param name="other">Collider that exited the trigger</param>
    private void OnTriggerExit(Collider other)
    {
        // Only remove objects if this component is enabled
        if (enabled)
        {
            // Remove the GameObject from the list if it was tracked inside
            if (objectsInside.Contains(other.gameObject))
            {
                objectsInside.Remove(other.gameObject);
            }
        }
    }

    /// <summary>
    /// Public method to start destroying all GameObjects currently inside the trigger.
    /// It triggers a coroutine that destroys objects after one frame delay.
    /// </summary>
    public void DestroyOverlappingItems()
    {
        StartCoroutine(DestroyOneFrameLater());
    }

    /// <summary>
    /// Coroutine that waits one frame, then destroys all tracked GameObjects inside.
    /// Clears the list after destruction to reset tracking.
    /// </summary>
    /// <returns>IEnumerator for coroutine</returns>
    public IEnumerator DestroyOneFrameLater()
    {
        // Wait one frame before executing destruction
        yield return null;

        // Destroy all GameObjects in the list that are not null
        foreach (GameObject obj in objectsInside)
        {
            if (obj != null)
                Destroy(obj);
        }

        // Clear the list after destruction
        objectsInside.Clear();
    }

    /// <summary>
    /// Called when the script is reset or first added in the editor.
    /// Ensures the attached collider is set to trigger mode by default.
    /// </summary>
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }
}
