using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem;  // For Key type

/// <summary>
/// This class responds to keyboard events, they are global events
/// </summary>
public class KeyListener : MonoBehaviour
{

    [System.Serializable]
    public class KeyEventPair
    {
        [Tooltip("Key to listen for")]
        public Key key;

        [Tooltip("Event triggered when the key is pressed")]
        public UnityEvent onKeyPressEvent;
        public UnityEvent onKeyReleaseEvent;
    }

    [Tooltip("List of key-event mappings")]
    public List<KeyEventPair> keyEvents = new();

    private void Update()
    {
        if (enabled)
        {
            foreach (KeyEventPair keyEventPair in keyEvents)
            {
                if (Keyboard.current[keyEventPair.key]?.wasPressedThisFrame == true)
                {
                    keyEventPair.onKeyPressEvent?.Invoke();
                }
                else if (Keyboard.current[keyEventPair.key]?.wasReleasedThisFrame == true)
                {
                    keyEventPair.onKeyReleaseEvent?.Invoke();
                }
            }
        }
    }

}
