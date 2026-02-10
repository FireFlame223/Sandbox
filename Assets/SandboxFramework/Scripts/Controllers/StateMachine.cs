using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent] // Prevent multiple instances of this component on one GameObject
public class StateMachine : MonoBehaviour
{
    [Serializable]
    public class State
    {
        public string name;           // Name identifier of the state
        public UnityEvent OnStart;    // Event triggered when the state starts
        public UnityEvent OnStop;     // Event triggered when the state stops
    }

    [Tooltip("Start state name, use 'None' for no active state")]
    [SerializeField]
    private string startState = "None";  // Initial state name, 'None' means no active state

    public List<State> states = new List<State>(); // List of all possible states

    private State currentState = null;   // Currently active state

    // Public read-only property to get the current state's name or 'None' if no active state
    public string CurrentState
    {
        get
        {
            if (currentState != null)
            {
                return currentState.name;
            }
            return "None";
        }
    }

    void Start()
    {
        // Set the initial state when the script starts
        SetState(startState);
    }

    /// <summary>
    /// Changes the current state to the specified state by name.
    /// If the state is "None", stops any active state.
    /// </summary>
    /// <param name="stateName">Name of the state to activate</param>
    public void SetState(string stateName)
    {
        if (!enabled) return; // Do nothing if this component is disabled

        // If stateName is "None", stop current state if any and clear active state
        if (string.Equals(stateName, "None", StringComparison.OrdinalIgnoreCase))
        {
            if (currentState != null)
            {
                currentState.OnStop?.Invoke();
                currentState = null;
            }
            return;
        }

        // Find the state with the matching name (case-insensitive)
        var newState = states.Find(s => string.Equals(s.name, stateName, StringComparison.OrdinalIgnoreCase));
        if (newState == null)
        {
            Debug.LogWarning($"StateMachine: State '{stateName}' not found!");
            return;
        }

        // If the new state is already active, do nothing
        if (newState == currentState)
            return;

        // If there is a current active state, invoke its OnStop event
        if (currentState != null)
            currentState.OnStop?.Invoke();

        // Activate the new state and invoke its OnStart event
        currentState = newState;
        currentState.OnStart?.Invoke();
    }
}
