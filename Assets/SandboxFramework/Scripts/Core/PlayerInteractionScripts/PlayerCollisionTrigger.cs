using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detects collisions from the player's character controller and sends appropriate collision events
/// (enter, stay, exit) to objects that have a CollisionListener component.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCollisionTrigger : MonoBehaviour
{
    // Keeps track of objects hit in the current and previous frames.
    private HashSet<GameObject> currentFrameHits = new HashSet<GameObject>();
    private HashSet<GameObject> previousFrameHits = new HashSet<GameObject>();

    /// <summary>
    /// Called by Unity when the character controller hits another collider.
    /// Triggers OnPlayerCollisionEnter or OnPlayerCollisionStay depending on hit history.
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject hitObject = hit.gameObject;
        currentFrameHits.Add(hitObject);

        CollisionListener collisionListener = hitObject.GetComponent<CollisionListener>();
        if (collisionListener == null) return;

        if (previousFrameHits.Contains(hitObject))
        {
            collisionListener.OnPlayerCollisionStay();
        }
        else
        {
            collisionListener.OnPlayerCollisionEnter();
        }
    }

    /// <summary>
    /// Runs after all Updates. Compares current and previous frame hits to detect exits.
    /// </summary>
    private void LateUpdate()
    {
        // Trigger exit events for objects no longer being collided with
        foreach (GameObject obj in previousFrameHits)
        {
            if (!currentFrameHits.Contains(obj))
            {
                obj.GetComponent<CollisionListener>()?.OnPlayerCollisionExit();
            }
        }

        // Swap sets for next frame and clear current frame hits
        var temp = previousFrameHits;
        previousFrameHits = currentFrameHits;
        currentFrameHits = temp;
        currentFrameHits.Clear();
    }
}
