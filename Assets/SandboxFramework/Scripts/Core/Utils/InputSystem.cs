using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum InputAxis
{
    Horizontal,
    Vertical
}

[System.Serializable]
public enum InputButton
{
    Weld,
    Unweld,
    Rotate1,
    Rotate2,
    Jump,
    ShowHierarchy
}

[DisallowMultipleComponent]
public class InputSystem : MonoBehaviour
{
    // Threshold below which axis input is considered zero (dead zone)
    const float axesCutOffValue = 0.1f;
    // Smoothing factor for axis input changes (lerp speed)
    const float axesSmoothingFactor = 0.1f;

    // Reference to the generated input actions class
    private static PlayerInputActions input;
    // Maps InputButton enum to actual InputAction from PlayerInputActions
    private static Dictionary<InputButton, InputAction> inputMap = new();

    // Smoothed horizontal and vertical axis values
    private static float horizontalAxis = 0f;
    private static float verticalAxis = 0f;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes input actions and enables them.
    /// Sets up the dictionary mapping InputButton to InputAction.
    /// </summary>
    public void Awake()
    {
        input = new PlayerInputActions();
        input.Enable();
        inputMap = new(){
            { InputButton.Weld, input.Default.Weld },
            { InputButton.Unweld, input.Default.Unweld },
            { InputButton.Rotate1, input.Default.Rotate1 },
            { InputButton.Rotate2, input.Default.Rotate2 },
            { InputButton.Jump, input.Default.Jump },
            { InputButton.ShowHierarchy, input.Default.ShowHierarchy }
        };
    }

    /// <summary>
    /// Called every frame.
    /// Reads raw axis values from input and smooths them using Lerp.
    /// Applies dead zone cutoff to ignore small input noise.
    /// </summary>
    void Update()
    {
        float rawHorizontalAxis = input.Default.VehicleAxisHorizontal.ReadValue<float>();
        float rawVerticalAxis = input.Default.VehicleAxisVertical.ReadValue<float>();
        horizontalAxis = Mathf.Lerp(horizontalAxis, rawHorizontalAxis, axesSmoothingFactor);
        verticalAxis = Mathf.Lerp(verticalAxis, rawVerticalAxis, axesSmoothingFactor);

        if (Mathf.Abs(horizontalAxis) < axesCutOffValue) horizontalAxis = 0f;
        if (Mathf.Abs(verticalAxis) < axesCutOffValue) verticalAxis = 0f;
    }

    /// <summary>
    /// Checks if the specified input button was pressed during the current frame.
    /// Returns false if input system is not initialized or button is not mapped.
    /// </summary>
    /// <param name="button">The InputButton to check.</param>
    /// <returns>True if pressed this frame, false otherwise.</returns>
    public static bool GetButtonDown(InputButton button)
    {
        if (input == null) return false;
        if (inputMap.TryGetValue(button, out var action))
            return action.WasPressedThisFrame();
        return false;
    }

    /// <summary>
    /// Checks if the specified input button is currently held down.
    /// Returns false if input system is not initialized or button is not mapped.
    /// </summary>
    /// <param name="button">The InputButton to check.</param>
    /// <returns>True if currently pressed, false otherwise.</returns>
    public static bool GetButton(InputButton button)
    {
        if (input == null) return false;
        if (inputMap.TryGetValue(button, out var action))
            return action.IsPressed();
        return false;
    }

    /// <summary>
    /// Returns the current position of the pointer (mouse or touch).
    /// Returns zero vector if input system is not initialized.
    /// </summary>
    /// <returns>Current pointer position as Vector2.</returns>
    public static Vector2 GetPointerPosition()
    {
        if (input == null) return Vector2.zero;
        return input.Default.PointerPosition.ReadValue<Vector2>();
    }

    /// <summary>
    /// Checks if the pointer press (mouse click or touch) was pressed this frame.
    /// Returns false if input system is not initialized.
    /// </summary>
    /// <returns>True if pointer pressed down this frame.</returns>
    public static bool GetPointerDown()
    {
        if (input == null) return false;
        return input.Default.PointerPress.WasPressedThisFrame();
    }

    /// <summary>
    /// Checks if the pointer press (mouse click or touch) was released this frame.
    /// Returns false if input system is not initialized.
    /// </summary>
    /// <returns>True if pointer released this frame.</returns>
    public static bool GetPointerUp()
    {
        if (input == null) return false;
        return input.Default.PointerPress.WasReleasedThisFrame();
    }

    /// <summary>
    /// Checks if the pointer press (mouse click or touch) is currently held.
    /// Returns false if input system is not initialized.
    /// </summary>
    /// <returns>True if pointer is currently pressed.</returns>
    public static bool GetPointerHeld()
    {
        if (input == null) return false;
        return input.Default.PointerPress.IsPressed();
    }

    /// <summary>
    /// Returns the smoothed axis value for the given input axis (Horizontal or Vertical).
    /// Returns 0 if input system is not initialized or axis is unrecognized.
    /// </summary>
    /// <param name="axis">The axis to read (Horizontal or Vertical).</param>
    /// <returns>Smoothed float value of the axis.</returns>
    public static float GetAxis(InputAxis axis)
    {
        if (input == null) return 0f;
        switch (axis)
        {
            case InputAxis.Horizontal:
                return horizontalAxis;

            case InputAxis.Vertical:
                return verticalAxis;
        }
        return 0f;
    }
}
