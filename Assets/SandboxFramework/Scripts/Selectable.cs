using System;
using UnityEngine;

/// <summary>
/// Represents an object that can be selected, with a description field.
/// Often used for UI display or tooltips.
/// </summary>
[DisallowMultipleComponent]
public class Selectable : MonoBehaviour
{
    [Tooltip("The description of this object, shown in UI elements or tooltips.")]
    [SerializeField]
    private string objectDescription = "";

    /// <summary>
    /// Public access to the object's description.
    /// </summary>
    public string ObjectDescription
    {
        get => objectDescription;
        set => objectDescription = value;
    }
}
