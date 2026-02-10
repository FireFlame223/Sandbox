using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    /// <summary>
    /// Retrieves all unique components of type T in this object's root-welded hierarchy
    /// and in all connected Weldables, including inactive objects.
    /// Skips duplicates (roots and components).
    /// </summary>
    public static IReadOnlyList<T> FindAllInHierarchyAndConnections<T>(Weldable weldable) where T : class
    {
        if (weldable == null)
            return new List<T>().AsReadOnly();

        var foundSet = new HashSet<T>();
        var scannedRoots = new HashSet<Transform>();

        void ScanRoot(Transform root)
        {
            if (root == null || !scannedRoots.Add(root))
                return;

            foreach (var component in root.GetComponentsInChildren<T>(true))
            {
                if (component != null)
                    foundSet.Add(component);
            }
        }

        ScanRoot(weldable.transform.root);

        foreach (var other in weldable.GetAllConnectedRecursive())
        {
            ScanRoot(other.transform.root);
        }

        var foundList = new List<T>(foundSet);
        return foundList.AsReadOnly();
    }
}
