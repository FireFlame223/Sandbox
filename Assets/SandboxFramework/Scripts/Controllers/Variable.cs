using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public enum ComparisonMode
{
    SmallerThan,        // Checks if value is less than the comparison value (<)
    GreaterThan,        // Checks if value is greater than the comparison value (>)
    SmallerThanOrEqual, // Checks if value is less than or equal to the comparison value (<=)
    GreaterThanOrEqual  // Checks if value is greater than or equal to the comparison value (>=)
}

[Serializable]
public struct Condition
{
    [Tooltip("Comparison type for this condition.")]
    public ComparisonMode comparisonMode;
    
    [Tooltip("The value to compare against.")]
    public float value;
    
    [Tooltip("Event triggered when this condition evaluates to true.")]
    public UnityEvent OnEvaluateToTrue;
    
    [Tooltip("If true, stops evaluating subsequent conditions once this one evaluates to true.")]
    public bool stopIfTrue; // Added for prioritized evaluation
}

/// <summary>
/// Manages a single variable associated with this GameObject.
/// This script should be attached to a child GameObject of the main object.
/// The GameObject's name is used as an identifier for this variable and also as
/// the name of the corresponding Animator parameter to synchronize.
/// </summary>
public class Variable : MonoBehaviour
{
    [Header("Variable Settings")]
    [Tooltip("The current value of this variable.")]
    public float value;

    [Tooltip("Minimum allowed value for this variable.")]
    public float minValue = 0f;

    [Tooltip("Maximum allowed value for this variable.")]
    public float maxValue = 100f;

    [Tooltip("Rate of change of this variable per second (positive or negative).")]
    public float changePerSecond = 0f;

    [Tooltip("If true, updates the value smoothly every frame instead of in whole steps per second.")]
    public bool smoothUpdate = false;

    [Header("UI & Event Coupling")]
    [Tooltip("Event fired whenever the variable's value changes, passing the new value.")]
    public UnityEvent<float> onValueChange;

    [Tooltip("Conditions that are evaluated whenever the value changes.")]
    public Condition[] conditions;

    private Animator animator = null;   // Reference to parent Animator component
    private string varName = "";        // The variable name, taken from GameObject name
    private float currentTime = 0f;     // Accumulates deltaTime to apply value changes per second

    /// <summary>
    /// Sets the rate of change per second for this variable.
    /// </summary>
    public void SetChangePerSecond(float value)
    {
        changePerSecond = value;
    }

    /// <summary>
    /// Checks if the animator has a parameter matching the variable name and type (Float).
    /// </summary>
    bool VariableExistsInAnimator(Animator animator, string varName)
    {
        if (animator == null) return false;

        for (var i = 0; i < animator.parameterCount; i++)
        {
            var parameter = animator.GetParameter(i);
            if (parameter.name == varName && parameter.type == AnimatorControllerParameterType.Float)
            {
                return true;
            }
        }
        return false;
    }

    void Awake()
    {
        varName = gameObject.name;
        // Search in parent to find the main object's Animator
        animator = GetComponentInParent<Animator>();

        if (!VariableExistsInAnimator(animator, varName))
        {
            // If parameter doesn't exist, ignore the Animator sync
            animator = null;
        }

        // Initialize the value with clamping and event triggers
        SetValue(value);
    }

    void Update()
    {
        if (!enabled) return;

        if (smoothUpdate)
        {
            // Smooth (per frame) update
            SetValue(value + changePerSecond * Time.deltaTime);
        }
        else
        {
            // Stepwise (per second) update
            currentTime += Time.deltaTime;

            if (currentTime >= 1f)
            {
                int steps = Mathf.FloorToInt(currentTime);
                currentTime -= steps;

                SetValue(value + changePerSecond * steps);
            }
        }
    }
    
    /// <summary>
    /// Changes the variable's value by a relative amount, with clamping and event triggers.
    /// </summary>
    public void ChangeValue(float amount)
    {
        SetValue(value + amount);
    }

    /// <summary>
    /// Sets the variable to an absolute value, clamped within min and max.
    /// Triggers onValueChange and evaluates conditions if value changed.
    /// </summary>
    public void SetValue(float newValue)
    {
        float clampedValue = Mathf.Clamp(newValue, minValue, maxValue);

        if (value != clampedValue)
        {
            float oldValue = value;
            value = clampedValue;

            // Always sync animator
            if (animator != null)
            {
                animator.SetFloat(varName, value);
            }

            // Fire value changed event
            onValueChange?.Invoke(value);

            if (enabled)
            {
                foreach (Condition condition in conditions)
                {
                    bool wasTrue = EvaluateCondition(condition, oldValue);
                    bool isTrue  = EvaluateCondition(condition, value);

                    // Edge-trigger: only invoke when transitioning from false → true
                    if (!wasTrue && isTrue)
                    {
                        condition.OnEvaluateToTrue?.Invoke();
                        
                        // NEW: Prioritized evaluation (stop-if-true logic)
                        if (condition.stopIfTrue)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Evaluates a condition against a given value.
    /// </summary>
    private bool EvaluateCondition(Condition condition, float testValue)
    {
        switch (condition.comparisonMode)
        {
            case ComparisonMode.SmallerThan:
                return testValue < condition.value;
            case ComparisonMode.SmallerThanOrEqual:
                return testValue <= condition.value;
            case ComparisonMode.GreaterThan:
                return testValue > condition.value;
            case ComparisonMode.GreaterThanOrEqual:
                return testValue >= condition.value;
            default:
                return false;
        }
    }
}