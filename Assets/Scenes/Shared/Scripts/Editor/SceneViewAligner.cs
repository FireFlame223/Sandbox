using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneViewAligner
{
    // Register callback when a scene is opened
    static SceneViewAligner()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    // Manual menu command with shortcut: CMD/CTRL + SHIFT + E
    [MenuItem("Tools/Align Scene View to 'EditorViewPosition' %#e")]
    private static void AlignSceneView()
    {
        AlignToTarget();
    }

    // Called automatically when a scene is opened
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        AlignToTarget();
    }

    // Shared alignment logic
    private static void AlignToTarget()
    {
        GameObject target = GameObject.Find("EditorViewPosition");
        if (target == null)
        {
            Debug.LogWarning("No GameObject named 'EditorViewPosition' found in the scene.");
            return;
        }

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
        {
            Debug.LogWarning("No active SceneView found.");
            return;
        }

        sceneView.pivot = target.transform.position;
        sceneView.rotation = target.transform.rotation;
        sceneView.size = 0f; // Set zoom level to default
        sceneView.Repaint();
    }
}
