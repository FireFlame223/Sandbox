using UnityEngine;
using UnityEngine.Events;

public class GameObjectListener : MonoBehaviour
{
    // Event invoked when the GameObject is started
    public UnityEvent onStart;

    // Event invoked every frame
    public UnityEvent onUpdate;

    // Event invoked every fixed frame-rate frame
    public UnityEvent onFixedUpdate;

    // Event invoked after all Update functions have been called
    public UnityEvent onLateUpdate;

    // Event invoked when the GameObject is destroyed
    public UnityEvent onDestroy;

    // Start is called before the first frame update
    void Start()
    {
        if (enabled)
            onStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
            onUpdate?.Invoke();
    }

    // FixedUpdate is called at a fixed interval
    void FixedUpdate()
    {
        if (enabled)
            onFixedUpdate?.Invoke();
    }

    // LateUpdate is called every frame after all Update calls
    void LateUpdate()
    {
        if (enabled)
            onLateUpdate?.Invoke();
    }

    // OnDestroy is called when the MonoBehaviour will be destroyed
    void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
