using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using StarterAssets;

/// <summary>
/// Allows a player to sit on a seat when entering a trigger zone.
/// Pressing a key exits the seat and restores player control.
/// </summary>
[RequireComponent(typeof(Weldable))]
[DisallowMultipleComponent]
public class Seat : MonoBehaviour, IWeldListener
{
    [Header("Seat Settings")]
    public Transform seatPoint;                 // Position and rotation where the player will sit
    public string playerTag = "Player";         // Tag to identify player objects
    public float debounceDuration = 1f;         // Cooldown time after unseating before another seating is allowed

    public UnityEvent onSeat;                    // Event triggered when player sits
    public UnityEvent onUnseat;                  // Event triggered when player leaves the seat

    private Transform seatedPlayer;              // Currently seated player transform reference
    private Transform originalParent;            // Original parent of the player to restore hierarchy on unseat
    private int originalLayer;                   // Original layer of the player to restore on unseat
    private float debounceTimer = 0f;            // Timer to prevent immediate reseating

    private bool isWelded;                       // Indicates if seat is welded (active/available for seating)

    // List of keys to check for input (can be customized via inspector)
    public List<Key> keysToCheck = new List<Key>();

    /// <summary>
    /// Returns the currently seated player's GameObject, or null if no one is seated.
    /// </summary>
    public GameObject GetSeatedPlayer()
    {
        return seatedPlayer?.gameObject;
    }

    // IWeldListener interface methods (empty here but can be extended)
    public void OnAdded()
    {
    }

    public void OnRemoved()
    {
    }

    // Called when the seat becomes welded (active)
    public virtual void OnWeld()
    {
        isWelded = true;
    }

    // Called when the seat becomes unwelded (inactive)
    public virtual void OnUnweld()
    {
        isWelded = false;
    }

    // Trigger event called when another collider enters this seat's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        if (CanSeatPlayer(other))
            SeatPlayer(other.transform);
    }

    /// <summary>
    /// Finds components of type T connected to this seat via welds or hierarchy.
    /// </summary>
    protected IReadOnlyList<T> FindConnectedComponents<T>() where T : class
    {
        Weldable weldable = GetComponent<Weldable>();
        return Utils.FindAllInHierarchyAndConnections<T>(weldable);
        // Alternative: return transform.root.GetComponentsInChildren<T>(true);
    }

    /// <summary>
    /// Handles keyboard input from the configured keys and notifies listeners.
    /// </summary>
    private void HandleKeyboardInput()
    {
        // Collect all keys pressed this frame from the configured list
        List<Key> keysPressed = new List<Key>();
        foreach (Key key in keysToCheck)
        {
            if (Keyboard.current[key]?.wasPressedThisFrame == true)
            {
                keysPressed.Add(key);
            }
        }

        if (keysPressed.Count == 0) return;

        // Notify all connected IKeypressListener components about the pressed keys
        foreach (IKeypressListener keyPressListener in FindConnectedComponents<IKeypressListener>())
        {
            foreach (Key pressedKey in keysPressed)
            {
                keyPressListener.OnKeyPress(pressedKey);
            }
        }
    }

    // Called every frame to update input and seat state
    protected virtual void Update()
    {
        if (!enabled) return;

        if (seatedPlayer != null)
            HandleKeyboardInput();

        UpdateDebounceTimer();

        if (IsExitRequested())
            ExitSeat();
    }

    /// <summary>
    /// Checks if the player can currently be seated based on various conditions.
    /// </summary>
    private bool CanSeatPlayer(Collider other)
    {
        if (!enabled) return false;

        return isWelded &&                 // Seat must be welded (active)
               seatedPlayer == null &&     // No player currently seated
               debounceTimer <= 0f &&      // Debounce timer must have elapsed
               other.CompareTag(playerTag);// The collider must have the player tag
    }

    /// <summary>
    /// Seats the player by moving them to the seat point and disabling their movement.
    /// Stores the player's original parent and layer for later restoration.
    /// </summary>
    private void SeatPlayer(Transform player)
    {
        if (!enabled) return;

        seatedPlayer = player;

        // Disable player movement if controller component exists
        FirstPersonController firstPersonController = player.GetComponent<FirstPersonController>();
        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;
        }
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        // Store original hierarchy and layer to restore later
        originalParent = player.parent?.parent;
        originalLayer = player.gameObject.layer;

        // Move player to seat position and rotation, and reparent under seatPoint
        player.SetPositionAndRotation(seatPoint.position, seatPoint.rotation);
        player.parent.SetParent(seatPoint);

        // Trigger events related to seating
        onSeat?.Invoke();
        OnSeat(player.gameObject);
        NotifyOnSeatListeners();
    }

    // Virtual method called when player is seated; can be overridden in derived classes
    protected virtual void OnSeat(GameObject player) { }

    // Virtual method called when player unseats; can be overridden in derived classes
    protected virtual void OnUnseat(GameObject player) { }

    /// <summary>
    /// Notifies all connected ISeatListener components about unseating.
    /// </summary>
    protected virtual void NotifyOnUnseatListeners()
    {
        if (!enabled) return;
        foreach (ISeatListener seatListener in FindConnectedComponents<ISeatListener>())
        {
            seatListener.OnUnseat();
        }
    }

    /// <summary>
    /// Notifies all connected ISeatListener components about seating.
    /// </summary>
    protected virtual void NotifyOnSeatListeners()
    {
        if (!enabled) return;
        foreach (ISeatListener seatListener in FindConnectedComponents<ISeatListener>())
        {
            seatListener.OnSeat();
        }
    }

    /// <summary>
    /// Updates the debounce timer, counting down until seating can occur again.
    /// </summary>
    private void UpdateDebounceTimer()
    {
        if (debounceTimer > 0f)
            debounceTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Returns true if the player is seated and the exit key (Jump) is pressed.
    /// </summary>
    private bool IsExitRequested()
    {
        if (!enabled) return true;
        return seatedPlayer != null && InputSystem.GetButtonDown(InputButton.Jump);
    }

    /// <summary>
    /// Handles exiting the seat: restores player movement, hierarchy, layer, and triggers unseat events.
    /// Also resets the debounce timer.
    /// </summary>
    private void ExitSeat()
    {
        if (seatedPlayer == null) return;
        GameObject player = seatedPlayer.gameObject;

        // Trigger events related to seating
        onUnseat?.Invoke();
        OnUnseat(player.gameObject);
        NotifyOnUnseatListeners();

        // Re-enable player movement
        FirstPersonController firstPersonController = player.GetComponent<FirstPersonController>();
        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;
        }
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        // Restore player's original parent in hierarchy
        Transform playerTransform = seatedPlayer;
        playerTransform.parent.SetParent(originalParent);

        // Maintain player's forward direction or reset to upright if invalid
        Vector3 forward = playerTransform.forward.normalized;
        if (forward.sqrMagnitude > 0f)
            playerTransform.forward = forward;
        else
            playerTransform.up = Vector3.up;

        // Restore player's original layer
        playerTransform.gameObject.layer = originalLayer;

        // Clear seated player reference and reset debounce timer
        seatedPlayer = null;
        debounceTimer = debounceDuration;
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    FirstPersonController firstPersonController = null;
    MethodInfo cameraRotationMethod = null;
    
    private void CameraRotation()
    {
        if (seatedPlayer == null) return;
        if (firstPersonController == null)
        {
            GameObject player = seatedPlayer.gameObject;
            firstPersonController = player.GetComponent<FirstPersonController>();
        }
        if (firstPersonController != null && !firstPersonController.enabled)
            {
                if (cameraRotationMethod == null)
                {
                    cameraRotationMethod = typeof(FirstPersonController).GetMethod(
                            "CameraRotation",
                            BindingFlags.Instance | BindingFlags.NonPublic
                        );
                }

                if (cameraRotationMethod != null)
                {
                    cameraRotationMethod.Invoke(firstPersonController, null);
                }
            }
    }
}
