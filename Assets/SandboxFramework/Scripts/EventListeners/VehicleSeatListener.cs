using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// VehicleSeatListener listens for seat, steering, throttle, and weld events on a vehicle part,
/// processing input values and firing relevant UnityEvents.
/// </summary>
public class VehicleSeatListener : KeyPressListener, IVehicleListener, IWeldListener
{
    /// <summary>
    /// Represents possible wheel positions relative to the vehicle.
    /// </summary>
    [System.Serializable]
    public enum WheelPosition
    {
        None,
        FrontLeft, FrontRight,
        RearLeft, RearRight,
        Left, Right, Front, Rear
    }

    /// <summary>
    /// Event triggered when a wheel matches a specific position.
    /// </summary>
    [System.Serializable]
    public struct PositionalEvent
    {
        public WheelPosition position;
        public UnityEvent onWheelPosition;
    }

    /// <summary>
    /// Settings for preprocessing input values such as steering and throttle.
    /// </summary>
    [System.Serializable]
    public struct ValuePreprocessing
    {
        public bool invert;
        public bool makeAbsolute;
        public float cutoffMin;
        public float cutoffMax;
        public float mappingMin;
        public float mappingMax;

        // Constructor with optional parameters for convenience
        public ValuePreprocessing(bool invert = false, bool makeAbsolute = false,
                          float cutoffMin = -1f, float cutoffMax = 1f,
                          float mappingMin = -1f, float mappingMax = 1f)
        {
            this.invert = invert;
            this.makeAbsolute = makeAbsolute;
            this.cutoffMin = cutoffMin;
            this.cutoffMax = cutoffMax;
            this.mappingMin = mappingMin;
            this.mappingMax = mappingMax;
        }
    }

    [Header("Steering")]
    public ValuePreprocessing steeringSettings;
    public UnityEvent<float> onSteer;

    [Header("Throttle")]
    public ValuePreprocessing throttlingSettings;
    public UnityEvent<float> onThrottle;

    [Header("Seating")]
    public UnityEvent onSeat;
    public UnityEvent onUnseat;

    /// <summary>
    /// Current wheel position of this listener.
    /// </summary>
    private WheelPosition wheelPosition = WheelPosition.None;

    /// <summary>
    /// List of positional events triggered based on wheel position.
    /// </summary>
    public List<PositionalEvent> onWheelPosition = new List<PositionalEvent>();

    private bool steerListenerEnabled = true;
    private bool throttleListenerEnabled = true;

    void Start()
    {
        // Initialize steering and throttle values to zero
        OnSteer(0f);
        OnThrottle(0f);
    }

    void Reset()
    {
        // Default preprocessing settings for steering and throttle
        steeringSettings = new ValuePreprocessing
        {
            cutoffMin = -1f,
            cutoffMax = 1f,
            mappingMin = -1f,
            mappingMax = 1f,
        };

        throttlingSettings = new ValuePreprocessing
        {
            cutoffMin = -1f,
            cutoffMax = 1f,
            mappingMin = -1f,
            mappingMax = 1f,
        };
    }

    /// <summary>
    /// Preprocess input value according to the specified settings.
    /// Supports inversion, absolute value, cutoff/clamping, and remapping.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <param name="settings">Preprocessing parameters</param>
    /// <returns>Processed value</returns>
    float PreprocessValue(float value, ValuePreprocessing settings)
    {
        if (settings.invert) value = 1f - value;
        if (settings.makeAbsolute) value = Mathf.Abs(value);

        value = Mathf.Clamp(value, settings.cutoffMin, settings.cutoffMax);

        float inputRange = settings.cutoffMax - settings.cutoffMin;
        if (Mathf.Approximately(inputRange, 0f))
            return settings.mappingMin;

        float t = (value - settings.cutoffMin) / inputRange;
        return Mathf.Lerp(settings.mappingMin, settings.mappingMax, t);
    }

    /// <summary>
    /// Enable or disable steering event firing.
    /// </summary>
    public bool SteerListenerEnabled
    {
        get => steerListenerEnabled;
        set => steerListenerEnabled = value;
    }

    /// <summary>
    /// Enable or disable throttle event firing.
    /// </summary>
    public void SetThrottleListenerEnabled(bool value) => throttleListenerEnabled = value;

    /// <summary>
    /// Called when steering input is received. Applies preprocessing and invokes event if enabled.
    /// </summary>
    public void OnSteer(float value)
    {
        if (!steerListenerEnabled) return;
        value = PreprocessValue(value, steeringSettings);
        onSteer?.Invoke(value);
    }

    /// <summary>
    /// Called when throttle input is received. Applies preprocessing and invokes event if enabled.
    /// </summary>
    public void OnThrottle(float value)
    {
        if (!throttleListenerEnabled) return;
        value = PreprocessValue(value, throttlingSettings);
        onThrottle?.Invoke(value);
    }

    /// <summary>
    /// Checks if two wheel positions are considered equivalent by grouping related positions.
    /// For example, FrontLeft, FrontRight, and Front all count as 'Front'.
    /// </summary>
    private bool ArePositionsEquivalent(WheelPosition a, WheelPosition b)
    {
        if (a == b) return true;

        bool aIsFront = a == WheelPosition.FrontLeft || a == WheelPosition.FrontRight || a == WheelPosition.Front;
        bool bIsFront = b == WheelPosition.FrontLeft || b == WheelPosition.FrontRight || b == WheelPosition.Front;
        if (aIsFront && bIsFront) return true;

        bool aIsRear = a == WheelPosition.RearLeft || a == WheelPosition.RearRight || a == WheelPosition.Rear;
        bool bIsRear = b == WheelPosition.RearLeft || b == WheelPosition.RearRight || b == WheelPosition.Rear;
        if (aIsRear && bIsRear) return true;

        bool aIsLeft = a == WheelPosition.FrontLeft || a == WheelPosition.RearLeft || a == WheelPosition.Left;
        bool bIsLeft = b == WheelPosition.FrontLeft || b == WheelPosition.RearLeft || b == WheelPosition.Left;
        if (aIsLeft && bIsLeft) return true;

        bool aIsRight = a == WheelPosition.FrontRight || a == WheelPosition.RearRight || a == WheelPosition.Right;
        bool bIsRight = b == WheelPosition.FrontRight || b == WheelPosition.RearRight || b == WheelPosition.Right;
        if (aIsRight && bIsRight) return true;

        return false;
    }

    /// <summary>
    /// Finds the closest VehicleSeat component in the hierarchy relative to the given transform.
    /// Performs a breadth-first search up and down the hierarchy.
    /// </summary>
    private VehicleSeat FindClosestSeat(Transform origin)
    {
        HashSet<Transform> visited = new HashSet<Transform>();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(origin);
        visited.Add(origin);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            VehicleSeat seat = current.GetComponent<VehicleSeat>();
            if (seat != null)
                return seat;

            // Enqueue parent if not visited
            Transform parent = current.parent;
            if (parent != null && !visited.Contains(parent))
            {
                queue.Enqueue(parent);
                visited.Add(parent);
            }

            // Enqueue children if not visited
            foreach (Transform child in current)
            {
                if (!visited.Contains(child))
                {
                    queue.Enqueue(child);
                    visited.Add(child);
                }
            }
        }

        return null; // No seat found
    }

    public void OnAdded()
    {
    }

    public void OnRemoved()
    {
    }

    /// <summary>
    /// Called when this part is welded to the vehicle.
    /// Determines wheel position relative to the closest seat and triggers matching events.
    /// </summary>
    public void OnWeld()
    {
        wheelPosition = WheelPosition.None;
        VehicleSeat seat = FindClosestSeat(transform);

        if (seat != null)
        {
            // Determine local position relative to seat to infer wheel position
            Vector3 localOffset = seat.transform.InverseTransformPoint(transform.position);

            if (localOffset.z >= 0)
            {
                wheelPosition = (localOffset.x >= 0) ? WheelPosition.FrontRight : WheelPosition.FrontLeft;
            }
            else
            {
                wheelPosition = (localOffset.x >= 0) ? WheelPosition.RearRight : WheelPosition.RearLeft;
            }
        }

        // Invoke all positional events that match the calculated wheel position
        foreach (var positionalEvent in onWheelPosition)
        {
            if (ArePositionsEquivalent(positionalEvent.position, wheelPosition))
            {
                positionalEvent.onWheelPosition?.Invoke();
            }
        }
    }

    /// <summary>
    /// Called when this part is unwelded from the vehicle.
    /// Resets wheel position.
    /// </summary>
    public void OnUnweld()
    {
        wheelPosition = WheelPosition.None;
    }

    /// <summary>
    /// Invoked when the seat is occupied.
    /// </summary>
    public void OnSeat() => onSeat?.Invoke();

    /// <summary>
    /// Invoked when the seat is vacated.
    /// </summary>
    public void OnUnseat() => onUnseat?.Invoke();
}
