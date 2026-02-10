using UnityEngine;
using UnityEngine.Events;

public class DragListener : MonoBehaviour, IDragListener
{
    /// <summary>
    /// UnityEvent that will be called when the player grabs this item
    /// </summary>
    public UnityEvent onGrab;

    /// <summary>
    /// UnityEvent that will be called when the player releases this item
    /// </summary>
    public UnityEvent onRelease;

    public void OnGrab() => onGrab?.Invoke();

    public void OnRelease() => onRelease?.Invoke();
}
