using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem;  // For Key type

/// <summary>
/// Listens for key press events and triggers corresponding UnityEvents.
/// Implements IKeypressListener interface.
/// </summary>
public class KeyPressListener : MonoBehaviour, IKeypressListener
{
    [System.Serializable]
    public class KeyEventPair
    {
        [Tooltip("Key to listen for")]
        public Key key;

        [Tooltip("Event triggered when the key is pressed")]
        public UnityEvent onKeyEvent;
    }

    [Tooltip("List of key-event mappings")]
    public List<KeyEventPair> keyEvents = new();

    /// <summary>
    /// Called when a key is pressed.
    /// Invokes all UnityEvents mapped to this key.
    /// </summary>
    /// <param name="keyCode">The key that was pressed</param>
    public void OnKeyPress(Key keyCode)
    {
        // Iterate over all key-event pairs and invoke matching events
        foreach (var keyEvent in keyEvents)
        {
            if (keyEvent.key == keyCode)
            {
                keyEvent.onKeyEvent?.Invoke();
            }
        }
    }
}
