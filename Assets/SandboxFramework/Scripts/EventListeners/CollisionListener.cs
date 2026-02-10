using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exposes collision events and custom player-triggered events via UnityEvents.
/// </summary>
public class CollisionListener : MonoBehaviour
{
    [Header("Physics Collision Events")]
    public UnityEvent onCollisionEnter;
    public UnityEvent onCollisionStay;
    public UnityEvent onCollisionExit;

    [Header("Player Collision Events")]
    public UnityEvent onPlayerCollisionEnter;
    public UnityEvent onPlayerCollisionStay;
    public UnityEvent onPlayerCollisionExit;

    /// <summary>
    /// Unity built-in method called when another collider starts colliding with this object.
    /// Triggers the corresponding UnityEvent.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        InvokeEventIfEnabled(onCollisionEnter);
    }

    /// <summary>
    /// Unity built-in method called while another collider remains in contact with this object.
    /// Triggers the corresponding UnityEvent.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        InvokeEventIfEnabled(onCollisionStay);
    }

    /// <summary>
    /// Unity built-in method called when another collider stops colliding with this object.
    /// Triggers the corresponding UnityEvent.
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        InvokeEventIfEnabled(onCollisionExit);
    }

    /// <summary>
    /// Should be called by the player controller when it collides with this object.
    /// Triggers the "enter" event for a player-driven collision.
    /// </summary>
    public void OnPlayerCollisionEnter()
    {
        InvokeEventIfEnabled(onPlayerCollisionEnter);
    }

    /// <summary>
    /// Should be called continuously by the player controller while staying in contact with this object.
    /// Triggers the "stay" event for a player-driven collision.
    /// </summary>
    public void OnPlayerCollisionStay()
    {
        InvokeEventIfEnabled(onPlayerCollisionStay);
    }

    /// <summary>
    /// Should be called by the player controller when it stops colliding with this object.
    /// Triggers the "exit" event for a player-driven collision.
    /// </summary>
    public void OnPlayerCollisionExit()
    {
        InvokeEventIfEnabled(onPlayerCollisionExit);
    }

    /// <summary>
    /// Invokes the given UnityEvent only if this component is enabled.
    /// </summary>
    private void InvokeEventIfEnabled(UnityEvent unityEvent)
    {
        if (enabled && unityEvent != null)
        {
            unityEvent.Invoke();
        }
    }
}
