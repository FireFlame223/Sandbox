using UnityEngine;
using UnityEngine.Events;

public class WeldListener : MonoBehaviour, IWeldListener
{
    /// <summary>
    /// UnityEvent that will be called when the item is welded to a group
    /// </summary>
    public UnityEvent onWeld;
    /// <summary>
    /// UnityEvent that will be called when the item is unwelded from a group
    /// </summary>
    public UnityEvent onUnweld;

    /// <summary>
    /// UnityEvent that will be called when and item is welded to the weldgroup
    /// </summary>
    public UnityEvent onAdded;

    /// <summary>
    /// UnityEvent that will be called when and item is unwelded from the weldgroup
    /// </summary>
    public UnityEvent onRemoved;

    public void OnWeld() => onWeld?.Invoke();
    public void OnUnweld() => onUnweld?.Invoke();
    
    public void OnAdded() => onAdded?.Invoke();
    public void OnRemoved() => onRemoved?.Invoke();
}
