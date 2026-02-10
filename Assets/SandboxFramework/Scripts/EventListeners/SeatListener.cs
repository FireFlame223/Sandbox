using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SeatListener : KeyPressListener, ISeatListener
{
    /// <summary>
    /// UnityEvent that will be called when the player sits down
    /// </summary>
    public UnityEvent onSeat;
    /// <summary>
    /// UnityEvent that will be called when the player stands up
    /// </summary>
    public UnityEvent onUnseat;

    public void OnSeat() => onSeat?.Invoke();
    public void OnUnseat() => onUnseat?.Invoke();

}
