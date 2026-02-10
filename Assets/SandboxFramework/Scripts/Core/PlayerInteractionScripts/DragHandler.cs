using System;
using UnityEngine;

/// <summary>
/// Represents the current state of the drag operation.
/// </summary>
[Serializable]
public enum DragState
{
    Idle,
    Dragging
}

/// <summary>
/// Handles dragging and rotating of a selected object using the mouse.
/// Requires a SelectionHandler component to function.
/// </summary>
[RequireComponent(typeof(SelectionHandler))]
[DisallowMultipleComponent]
public class DragHandler : MonoBehaviour
{
    /// <summary>
    /// Defines which rotation axes are allowed during dragging.
    /// </summary>
    [Serializable]
    public enum RotationAxisFlags
    {
        None,
        YawOnly,
        All
    }

    [Header("Grid Settings")]
    public bool useGrid = true;
    public Vector3 gridSize = Vector3.one;
    public Vector3 gridCenter = Vector3.zero;

    [Header("Rotation Settings")]
    public RotationAxisFlags allowRotation = RotationAxisFlags.All;
    public Vector3 rotationSnapDegrees = Vector3.zero;

    [Header("Rigidbody")]
    public RigidbodyStateChange rigidbodyStateChangeOnDrag = RigidbodyStateChange.SetKinematic;
    public RigidbodyStateChange rigidbodyStateChangeOnRelease = RigidbodyStateChange.SetNonKinematic;
    public float throwMultiplier = 1.5f;
    public float maxThrowVelocity = 15f;

    private Camera cam;
    private Draggable selectedDraggable;
    private Vector3 localPositionOffset;
    private Quaternion localRotationOffset;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private SelectionHandler selectionHandler;
    private DragState currentState = DragState.Idle;

    public DragState CurrentState => currentState;
    public bool useRotationKey1 = true;
    public bool useRotationKey2 = true;

    public bool toggleToDrag = false;

    private void Start()
    {
        cam = Camera.main;
        selectionHandler = GetComponent<SelectionHandler>();
    }

    private void Update()
    {
        HandleInput();
    }

    public void SetGridEnabled(bool mode)
    {
        useGrid = mode;
    }

    private void FixedUpdate()
    {
        if (selectionHandler.currentSelection == null)
        {
            StopDragging();
            return;
        }

        if (currentState == DragState.Dragging)
        {
            UpdateTargetTransform();
            ApplyTransformToSelection();
        }
    }

    /// <summary>
    /// Handles pointer input for starting or stopping drag operations.
    /// </summary>
    private void HandleInput()
    {
        switch (currentState)
        {
            case DragState.Idle:
                if (InputSystem.GetPointerDown())
                    TryStartDragging();
                break;

            case DragState.Dragging:
                bool endDrag = toggleToDrag ? InputSystem.GetPointerDown() : InputSystem.GetPointerUp();
                if (selectedDraggable == null || endDrag)
                {
                    StopDragging();
                    return;
                }
                HandleRotation();
                break;
        }
    }

    /// <summary>
    /// Tries to start dragging the currently selected object.
    /// </summary>
    private void TryStartDragging()
    {
        GameObject selectedObject = selectionHandler.currentSelection;
        if (selectedObject == null) return;

        var draggable = selectedObject.GetComponent<Draggable>();
        if (draggable == null || !draggable.enabled) return;

        selectionHandler.LockSelection();
        InitializeSelection(selectedObject.transform);

        UpdateTargetTransform();
        selectedDraggable = draggable;
        selectedDraggable.StartDrag(rigidbodyStateChangeOnDrag);

        currentState = DragState.Dragging;
    }

    /// <summary>
    /// Initializes drag offsets for position and rotation relative to the camera.
    /// </summary>
    private void InitializeSelection(Transform selectedTransform)
    {
        localPositionOffset = cam.transform.InverseTransformPoint(selectedTransform.position);
        localRotationOffset = Quaternion.Inverse(cam.transform.rotation) * selectedTransform.rotation;
    }

    /// <summary>
    /// Stops dragging and resets the selection.
    /// </summary>
    private void StopDragging()
    {
        if (currentState != DragState.Dragging) return;

        if (selectedDraggable != null)
        {
            selectedDraggable.EndDrag(rigidbodyStateChangeOnRelease, throwMultiplier, maxThrowVelocity);
            selectedDraggable = null;
        }

        selectionHandler.UnlockSelection();
        currentState = DragState.Idle;
    }

    /// <summary>
    /// Handles rotation input while dragging.
    /// </summary>
    private void HandleRotation()
    {
        if (useRotationKey1 && InputSystem.GetButtonDown(InputButton.Rotate1))
            RotateSelected(0f, 90f, 0f);

        if (useRotationKey2 && InputSystem.GetButtonDown(InputButton.Rotate2))
            RotateSelectedTowardsCamera(-90f);
    }

    private void RotateSelected(float x, float y, float z)
    {
        if (selectedDraggable == null) return;

        // Determine if we rotate the root of the hierarchy
        Transform selectedRoot = selectedDraggable.transform.root;
        if (selectedRoot == null) return;

        // World position of the selected object's pivot
        Vector3 pivotWorldPos = selectedDraggable.transform.position;

        // Desired rotation delta
        Quaternion deltaRotation = Quaternion.Euler(x, y, z);

        // Vector from root to pivot
        Vector3 direction = selectedRoot.position - pivotWorldPos;

        // Move and rotate the root so the selected object rotates around its pivot
        selectedRoot.position = pivotWorldPos + deltaRotation * direction;
        selectedRoot.rotation = deltaRotation * selectedRoot.rotation;

        // Optional: snap rotation if you have a snapping function
        selectedRoot.rotation = GetSnappedRotation(selectedRoot.rotation);

        InitializeSelection(selectedDraggable.transform);
    }

    /// <summary>
    /// Rotates the selected object to face the camera from a fixed angle.
    /// </summary>
    private void RotateSelectedTowardsCamera(float angle)
    {
        if (selectedDraggable == null) return;
        Transform selectedTransform = selectedDraggable.transform;

        Transform selectedRoot = selectedDraggable.transform.root;
        if (selectedRoot == null) return;

        Vector3 cameraOffset = Vector3.zero;
        Vector3 toCamera = cam.transform.position - selectedTransform.position;

        if (Mathf.Abs(toCamera.x) > Mathf.Abs(toCamera.z * 1.5f))
            cameraOffset = toCamera.x > 0f ? Vector3.right : -Vector3.right;
        else
            cameraOffset = toCamera.z > 0f ? Vector3.forward : -Vector3.forward;

        Vector3 rightVector = Vector3.Cross(cameraOffset, Vector3.up);
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, rightVector);
        Vector3 pivot = selectedDraggable.transform.position;

        Vector3 direction = selectedRoot.position - pivot;
        selectedRoot.position = pivot + deltaRotation * direction;
        selectedRoot.rotation = GetSnappedRotation(deltaRotation * selectedRoot.rotation);

        Rigidbody rb = selectedRoot.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.MovePosition(selectedRoot.position);
            rb.MoveRotation(selectedRoot.rotation);
        }

        InitializeSelection(selectedDraggable.transform);
    }

    /// <summary>
    /// Updates the target world position and rotation of the selected object.
    /// </summary>
    private void UpdateTargetTransform()
    {
        if (selectedDraggable == null) return;

        Vector3 worldTargetPosition = cam.transform.TransformPoint(localPositionOffset);
        targetPosition = SnapToGrid(worldTargetPosition);

        Vector3 targetRotationAngles = selectedDraggable.transform.rotation.eulerAngles;

        if (allowRotation != RotationAxisFlags.None)
        {
            Vector3 worldTargetRotation = (cam.transform.rotation * localRotationOffset).eulerAngles;
            if (allowRotation == RotationAxisFlags.All)
                targetRotationAngles = worldTargetRotation;
            else if (allowRotation == RotationAxisFlags.YawOnly)
                targetRotationAngles.y = worldTargetRotation.y;
        }

        targetRotation = Quaternion.Euler(GetSnappedRotation(targetRotationAngles));
    }

    /// <summary>
    /// Applies the calculated target position and rotation to the selected object.
    /// </summary>
    private void ApplyTransformToSelection()
    {
        if (selectedDraggable)
            selectedDraggable.UpdateDrag(targetPosition, targetRotation);
    }

    /// <summary>
    /// Snaps a rotation vector to the defined snapping angles.
    /// </summary>
    private Vector3 GetSnappedRotation(Vector3 rotation)
    {
        if (rotationSnapDegrees.x > 0f)
            rotation.x = Mathf.Round(rotation.x / rotationSnapDegrees.x) * rotationSnapDegrees.x;
        if (rotationSnapDegrees.y > 0f)
            rotation.y = Mathf.Round(rotation.y / rotationSnapDegrees.y) * rotationSnapDegrees.y;
        if (rotationSnapDegrees.z > 0f)
            rotation.z = Mathf.Round(rotation.z / rotationSnapDegrees.z) * rotationSnapDegrees.z;

        return rotation;
    }

    /// <summary>
    /// Returns a quaternion snapped to the defined rotation snap settings.
    /// </summary>
    private Quaternion GetSnappedRotation(Quaternion quaternion)
    {
        return Quaternion.Euler(GetSnappedRotation(quaternion.eulerAngles));
    }

    /// <summary>
    /// Snaps a world position to the defined 3D grid.
    /// </summary>
    private Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 offset = position;

        if (useGrid)
        {
            offset -= gridCenter;
            offset.x = Mathf.Round(offset.x / gridSize.x) * gridSize.x;
            offset.y = Mathf.Round(offset.y / gridSize.y) * gridSize.y;
            offset.z = Mathf.Round(offset.z / gridSize.z) * gridSize.z;
            offset += gridCenter;
        }

        return offset;
    }

    /// <summary>
    /// Verifies whether a quaternion is normalized.
    /// </summary>
    private bool IsNormalized(Quaternion q)
    {
        return Mathf.Abs(1.0f - (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w)) < 0.01f;
    }
}
