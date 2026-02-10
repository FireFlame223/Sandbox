using UnityEngine;
using UnityEngine.Events;

public class RandomTrigger : MonoBehaviour
{
    [Tooltip("List of possible UnityEvents from which one will be randomly invoked.")]
    public UnityEvent[] events;

    [Tooltip("Automatically invoke one of the events on Start.")]
    public bool invokeOnStart = false;

    void Start()
    {
        // If invokeOnStart is true, call a random event when the script starts
        if (invokeOnStart)
        {
            InvokeRandomEvent();
        }
    }

    /// <summary>
    /// Invokes a random UnityEvent from the events array.
    /// </summary>
    public void InvokeRandomEvent()
    {
        // Do nothing if this component is disabled
        if (!enabled) return;

        // Check if the events array is null or empty
        if (events == null || events.Length == 0)
        {
            Debug.LogWarning($"{name}: No events available to invoke.");
            return;
        }

        // Pick a random index in the events array
        int index = Random.Range(0, events.Length);

        // Invoke the selected event if it is not null
        if (events[index] != null)
        {
            events[index].Invoke();
        }
        else
        {
            Debug.LogWarning($"{name}: Selected event is null.");
        }
    }
}
