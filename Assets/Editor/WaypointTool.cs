using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool to quickly place and edit the enemy path waypoints of a level.
///
/// The runtime expects:
///   WayPoints (tag "WayPoints", WayPointManager component)
///     ├─ child 0   -> waypoint 0
///     ├─ child 1   -> waypoint 1   (sibling order defines the path order)
///     └─ ...
///
/// WayPointManager.Awake() collects the direct children in order; LinearMovement
/// walks them by transform.position. Each waypoint is therefore just an empty
/// Transform. This tool appends/removes/drags those transforms, keeps them named
/// in order and registers everything with Undo. An always-on gizmo draws the path
/// in the Scene view even when the window is closed.
/// </summary>
public class WaypointTool : EditorWindow
{
    private bool placementActive;
    private bool snapToGrid;

    private const string ParentName = "WayPoints";
    private static readonly Color PathColor = new Color(0.2f, 0.8f, 1f, 1f);
    private static readonly Color StartColor = new Color(0.2f, 1f, 0.3f, 1f);
    private static readonly Color EndColor = new Color(1f, 0.3f, 0.2f, 1f);
    private static readonly Color RemoveColor = new Color(1f, 0f, 0f, 1f);

    [MenuItem("Tools/Waypoints/Waypoint Placer")]
    public static void Open()
    {
        GetWindow<WaypointTool>("Waypoint Placer");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Build the enemy path by clicking in the Scene view.\n\n" +
            "• Left click: append a waypoint to the end of the path\n" +
            "• Shift + left click: remove the waypoint under the cursor\n" +
            "• Drag the blue dots to move existing waypoints\n\n" +
            "Waypoints are followed in order (green = start, red = end).",
            MessageType.Info);

        EditorGUILayout.Space();

        snapToGrid = EditorGUILayout.ToggleLeft(
            new GUIContent("Snap to grid", "Snap new waypoints to the GridManager cells."), snapToGrid);

        GameObject parent = FindParent(false);
        int count = parent != null ? parent.transform.childCount : 0;
        EditorGUILayout.LabelField("Waypoints in path", count.ToString());

        EditorGUILayout.Space();
        GUI.backgroundColor = placementActive ? Color.green : Color.white;
        if (GUILayout.Button(placementActive ? "Placement: ON (click to stop)" : "Start Placing", GUILayout.Height(32)))
        {
            placementActive = !placementActive;
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(count < 2))
            {
                if (GUILayout.Button("Reverse Path"))
                {
                    ReversePath(parent);
                }
            }
            using (new EditorGUI.DisabledScope(count == 0))
            {
                if (GUILayout.Button("Clear All") &&
                    EditorUtility.DisplayDialog("Clear waypoints", $"Delete all {count} waypoints?", "Delete", "Cancel"))
                {
                    ClearAll(parent);
                }
            }
        }

        if (snapToGrid && FindGrid() == null)
        {
            EditorGUILayout.HelpBox("No GridManager found — snapping will be ignored.", MessageType.Warning);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        GameObject parent = FindParent(false);

        // Draggable handles + numbered labels for existing waypoints.
        if (parent != null)
        {
            int i = 0;
            int last = parent.transform.childCount - 1;
            foreach (Transform wp in parent.transform)
            {
                Handles.color = i == 0 ? StartColor : (i == last ? EndColor : PathColor);
                float size = HandleUtility.GetHandleSize(wp.position) * 0.12f;

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.FreeMoveHandle(wp.position, size, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(wp, "Move Waypoint");
                    wp.position = snapToGrid ? Snap(newPos) : newPos;
                }

                Handles.Label(wp.position + Vector3.up * size * 3f, i.ToString());
                i++;
            }
        }

        if (!placementActive)
        {
            return;
        }

        Event e = Event.current;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlId);
        }

        if (!TryGetCursorPoint(e.mousePosition, out Vector3 worldPoint))
        {
            return;
        }

        Vector3 target = snapToGrid ? Snap(worldPoint) : worldPoint;
        bool removeMode = e.shift;

        // Preview: where the next point goes, and the segment from the current end.
        Handles.color = removeMode ? RemoveColor : StartColor;
        float pSize = HandleUtility.GetHandleSize(target) * 0.15f;
        Handles.DrawWireDisc(target, Vector3.up, pSize);
        if (!removeMode && parent != null && parent.transform.childCount > 0)
        {
            Transform end = parent.transform.GetChild(parent.transform.childCount - 1);
            Handles.color = new Color(StartColor.r, StartColor.g, StartColor.b, 0.5f);
            Handles.DrawDottedLine(end.position, target, 4f);
        }
        sceneView.Repaint();

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (removeMode)
            {
                RemoveNear(parent, target);
            }
            else
            {
                AppendWaypoint(target);
            }
            e.Use();
        }
    }

    private void AppendWaypoint(Vector3 position)
    {
        GameObject parent = FindParent(true);
        var wp = new GameObject();
        Undo.RegisterCreatedObjectUndo(wp, "Add Waypoint");
        Undo.SetTransformParent(wp.transform, parent.transform, "Add Waypoint");
        wp.transform.position = position;
        wp.name = $"Waypoint {parent.transform.childCount - 1}";
        Selection.activeGameObject = wp;
        EditorUtility.SetDirty(parent);
    }

    private void RemoveNear(GameObject parent, Vector3 position)
    {
        if (parent == null)
        {
            return;
        }

        Transform closest = null;
        float closestDist = float.MaxValue;
        foreach (Transform wp in parent.transform)
        {
            float d = Vector3.Distance(position, wp.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = wp;
            }
        }

        if (closest != null && closestDist < HandleUtility.GetHandleSize(closest.position))
        {
            Undo.DestroyObjectImmediate(closest.gameObject);
            Renumber(parent);
        }
    }

    private static void ReversePath(GameObject parent)
    {
        if (parent == null)
        {
            return;
        }
        Undo.RegisterFullObjectHierarchyUndo(parent, "Reverse Path");
        int n = parent.transform.childCount;
        for (int i = 0; i < n; i++)
        {
            parent.transform.GetChild(i).SetSiblingIndex(0); // pull each to front -> reversed order
        }
        Renumber(parent);
    }

    private static void ClearAll(GameObject parent)
    {
        if (parent == null)
        {
            return;
        }
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.transform.GetChild(i).gameObject);
        }
    }

    private static void Renumber(GameObject parent)
    {
        int i = 0;
        foreach (Transform wp in parent.transform)
        {
            wp.name = $"Waypoint {i++}";
        }
    }

    private static GameObject FindParent(bool createIfMissing)
    {
        GameObject parent = GameObject.FindGameObjectWithTag("WayPoints");
        if (parent == null && createIfMissing)
        {
            parent = new GameObject(ParentName) { tag = "WayPoints" };
            parent.AddComponent<WayPointManager>();
            Undo.RegisterCreatedObjectUndo(parent, "Create WayPoints Parent");
        }
        return parent;
    }

    private static GridManager FindGrid()
    {
        return Object.FindFirstObjectByType<GridManager>();
    }

    private static Vector3 Snap(Vector3 worldPoint)
    {
        GridManager grid = FindGrid();
        return grid != null ? grid.GetSnappedPosition(worldPoint) : worldPoint;
    }

    /// <summary>
    /// Resolves the world point under the cursor: scene colliders first, then a
    /// horizontal plane at the grid's height (or y = 0).
    /// </summary>
    private static bool TryGetCursorPoint(Vector2 guiPosition, out Vector3 point)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            point = hit.point;
            return true;
        }

        GridManager grid = FindGrid();
        float planeY = grid != null ? grid.transform.position.y : 0f;
        Plane ground = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        if (ground.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    /// <summary>Always-on path gizmo so the route is visible without opening the tool.</summary>
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    private static void DrawPathGizmo(WayPointManager manager, GizmoType gizmoType)
    {
        Transform root = manager.transform;
        if (root.childCount == 0)
        {
            return;
        }

        Gizmos.color = PathColor;
        Transform prev = null;
        int i = 0;
        int last = root.childCount - 1;
        foreach (Transform wp in root)
        {
            Gizmos.color = i == 0 ? StartColor : (i == last ? EndColor : PathColor);
            Gizmos.DrawSphere(wp.position, 0.15f);
            if (prev != null)
            {
                Gizmos.color = PathColor;
                Gizmos.DrawLine(prev.position, wp.position);
            }
            prev = wp;
            i++;
        }
    }
}
