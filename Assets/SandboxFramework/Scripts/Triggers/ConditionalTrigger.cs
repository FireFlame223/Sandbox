using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConditionalTrigger : MonoBehaviour
{
    [Serializable]
    public enum ContinueOption
    {
        Success,    // Continue to next on success
        Fail,       // Continue to next on failure
        Both        // Continue to next regardless of result
    }

    [Serializable]
    public enum ConditionType
    {
        ObjectName,     // Check GameObject name
        Tag,            // Check GameObject tag
        Layer,          // Check GameObject layer
        State,          // Check a custom state from a StateMachine component
        MaterialName,   // Check renderer material name
        AnimatorState,  // Check current animator state
        Any             // Match any collider in trigger
    }

    [Serializable]
    public class FilterCondition
    {
        public ConditionType conditionType;  // Type of condition to check
        public string value;                 // Value to compare against
        public bool negate;                  // Whether to negate the condition result
    }

    public List<FilterCondition> conditions = new List<FilterCondition>();

    public enum LogicalOperator
    {
        And,    // All conditions must be true
        Or      // Any condition must be true
    }

    public LogicalOperator conditionLogic = LogicalOperator.And;  // Logic to combine conditions
    public bool autoTrigger = true;                                // Automatically trigger on collider enter/exit
    public UnityEvent onConditionsMet;                             // Event fired when conditions are met
    public UnityEvent onConditionsFailed;                          // Event fired when conditions fail

    public float TimeToEvaluate = 0f;  // Delay time before evaluating conditions
    private float evaluationTimer = -1f; // Internal timer counting down evaluation delay

    [Tooltip("Lower values evaluate first. Higher values act like 'else' branches.")]
    public int evaluationOrder = 0;   // Order of evaluation to chain multiple invokers

    public ContinueOption ProceedOrderOnResult = ContinueOption.Both; // When to proceed to next evaluationOrder

    private readonly HashSet<Collider> collidersInTrigger = new HashSet<Collider>(); // Colliders currently inside trigger

    private int currentEvaluatingOrder = -1;  // Current evaluation order being processed

    private void Update()
    {
        // Countdown the evaluation timer if active
        if (evaluationTimer >= 0f)
        {
            evaluationTimer -= Time.deltaTime;
            if (evaluationTimer <= 0f)
            {
                evaluationTimer = -1f;
                EvaluateAndChain(currentEvaluatingOrder); // Evaluate conditions when timer ends
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || other == null || other.gameObject == null)
            return;

        collidersInTrigger.Add(other); // Add collider to set

        if (autoTrigger)
        {
            BeginEvaluation(0); // Automatically start evaluation at order 0
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled || other == null || other.gameObject == null)
            return;

        collidersInTrigger.Remove(other); // Remove collider from set

        if (autoTrigger)
        {
            BeginEvaluation(0); // Automatically start evaluation at order 0
        }
    }

    /// <summary>
    /// Begins evaluation if this invoker's evaluationOrder matches the given order.
    /// </summary>
    public void BeginEvaluation(int order)
    {
        if (!enabled) return;
        
        if (evaluationOrder != order)
            return;

        currentEvaluatingOrder = order;

        // If there's a delay, start timer, otherwise evaluate immediately
        if (TimeToEvaluate > 0f)
        {
            evaluationTimer = TimeToEvaluate;
        }
        else
        {
            EvaluateAndChain(order);
        }
    }

    /// <summary>
    /// Evaluates the conditions and triggers events.
    /// Also starts the next evaluation order if applicable.
    /// </summary>
    private void EvaluateAndChain(int order)
    {
        bool result = Evaluate();

        if (result)
        {
            onConditionsMet?.Invoke();  // Invoke success event
        }
        else
        {
            onConditionsFailed?.Invoke(); // Invoke failure event
        }

        // Check if next evaluation order should be triggered based on result and settings
        if (ProceedOrderOnResult == ContinueOption.Both ||
            (ProceedOrderOnResult == ContinueOption.Success && result) ||
            (ProceedOrderOnResult == ContinueOption.Fail && !result))
        {
            int nextOrder = order + 1;
            var invokers = GetComponents<ConditionalTrigger>();
            foreach (var invoker in invokers)
            {
                if (invoker.evaluationOrder == nextOrder)
                {
                    invoker.BeginEvaluation(nextOrder); // Trigger next order invoker
                }
            }
        }
    }

    /// <summary>
    /// Evaluates all filter conditions against colliders currently in trigger.
    /// Combines results using the configured logical operator.
    /// Returns true if conditions are met.
    /// </summary>
    private bool Evaluate()
    {
        // Start with base result depending on logic (true for AND, false for OR)
        bool result = (conditionLogic == LogicalOperator.And);

        foreach (var condition in conditions)
        {
            bool anyMatch = false;

            if (condition.conditionType == ConditionType.Any)
            {
                // If condition type is Any, check if any collider is present
                bool hasAny = collidersInTrigger.Count > 0;
                anyMatch = condition.negate ? !hasAny : hasAny;
            }
            else
            {
                // Check each collider for condition match
                foreach (var col in collidersInTrigger)
                {
                    if (EvaluateCondition(col, condition))
                    {
                        anyMatch = true;
                        break;
                    }
                }
            }

            // Combine matches according to logical operator
            if (conditionLogic == LogicalOperator.And && !anyMatch)
            {
                result = false; // AND fails if any condition not met
                break;
            }
            else if (conditionLogic == LogicalOperator.Or && anyMatch)
            {
                result = true;  // OR succeeds if any condition met
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Evaluates a single condition against a collider.
    /// Returns true if condition matches, considering negation.
    /// </summary>
    private bool EvaluateCondition(Collider other, FilterCondition condition)
    {
        bool match = false;
        string targetValue = condition.value.ToLowerInvariant();

        switch (condition.conditionType)
        {
            case ConditionType.ObjectName:
                // Check if collider name matches condition value (case-insensitive)
                match = other.name.Equals(condition.value, StringComparison.OrdinalIgnoreCase);
                break;

            case ConditionType.Tag:
                // Check if collider has matching tag
                match = other.CompareTag(condition.value);
                break;

            case ConditionType.Layer:
                // Check if collider's GameObject layer name matches condition value
                match = LayerMask.LayerToName(other.gameObject.layer)
                                .Equals(condition.value, StringComparison.OrdinalIgnoreCase);
                break;

            case ConditionType.State:
                // Check custom StateMachine component's current state
                var stateMachine = other.gameObject.GetComponent<StateMachine>();
                if (stateMachine != null)
                {
                    match = string.Equals(stateMachine.CurrentState, condition.value, StringComparison.OrdinalIgnoreCase);
                }
                break;

            case ConditionType.MaterialName:
                // Check if collider's renderer material name contains the condition value
                var renderer = other.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    match = renderer.sharedMaterial.name.ToLowerInvariant().Contains(targetValue);
                }
                break;

            case ConditionType.AnimatorState:
                // Check if any animator layer's current state matches condition value
                var animator = other.GetComponent<Animator>();
                if (animator != null)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
                        if (stateInfo.IsName(condition.value))
                        {
                            match = true;
                            break;
                        }
                    }
                }
                break;
        }

        // Return result considering negation flag
        return condition.negate ? !match : match;
    }
}
