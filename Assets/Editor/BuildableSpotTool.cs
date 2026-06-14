using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool to quickly add buildable spots to the current level.
///
/// The runtime (TowerPlaceManager) expects this hierarchy:
///   BuildAblePlaces  (tag "Buildable")        -> the parent
///     └─ Spot        (first-level child)       -> visual marker
///          └─ Plane  (second-level child)      -> the actual buildable spot
///
/// This tool duplicates an existing spot as a template (so meshes, colliders and
/// scale match the level exactly), snaps it to the GridManager grid and parents it
/// under the "Buildable" object. Everything is registered with Undo.
/// </summary>
public class BuildableSpotTool : EditorWindow
{
    private GameObject spotTemplate;
    private bool placementActive;

    private static readonly Color PreviewColor = new Color(1f, 0.7f, 0f, 1f);
    private static readonly Color RemoveColor = new Color(1f, 0f, 0f, 1f);

    [MenuItem("Tools/Buildable Spots/Spot Placer")]
    public static void Open()
    {
        GetWindow<BuildableSpotTool>("Spot Placer");
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
            "Place buildable spots by clicking in the Scene view.\n\n" +
            "• Left click: add a snapped spot\n" +
            "• Shift + left click: remove the spot under the cursor\n\n" +
            "New spots are duplicated from the template and parented under the " +
            "'Buildable' object so they match the rest of the level.",
            MessageType.Info);

        EditorGUILayout.Space();

        spotTemplate = (GameObject)EditorGUILayout.ObjectField(
            new GUIContent("Spot Template",
                "A spot to clone (first-level child of the Buildable parent, or a prefab). " +
                "Leave empty to auto-use the first existing spot in the scene."),
            spotTemplate, typeof(GameObject), true);

        GameObject parent = FindBuildableParent(false);
        GameObject template = ResolveTemplate(parent);

        using (new EditorGUI.DisabledScope(template == null))
        {
            EditorGUILayout.Space();
            GUI.backgroundColor = placementActive ? Color.green : Color.white;
            if (GUILayout.Button(placementActive ? "Placement: ON (click to stop)" : "Start Placing", GUILayout.Height(32)))
            {
                placementActive = !placementActive;
                SceneView.RepaintAll();
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space();
        if (parent == null)
        {
            EditorGUILayout.HelpBox("No GameObject tagged 'Buildable' in the scene. One will be created on first placement.", MessageType.Warning);
        }
        if (template == null)
        {
            EditorGUILayout.HelpBox("No template available. Assign a Spot Template or add at least one spot to the scene first.", MessageType.Error);
        }
        if (FindGrid() == null)
        {
            EditorGUILayout.HelpBox("No GridManager found. Spots will be placed at the clicked point without grid snapping.", MessageType.Warning);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!placementActive)
        {
            return;
        }

        GameObject template = ResolveTemplate(FindBuildableParent(false));
        if (template == null)
        {
            return;
        }

        Event e = Event.current;

        // Take control so clicks don't deselect / drag in the scene.
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlId);
        }

        if (!TryGetCursorPoint(e.mousePosition, out Vector3 worldPoint))
        {
            return;
        }

        Vector3 snapped = Snap(worldPoint);
        bool removeMode = e.shift;

        // Preview.
        Handles.color = removeMode ? RemoveColor : PreviewColor;
        float size = HandleUtility.GetHandleSize(snapped) * 0.5f;
        Handles.DrawWireDisc(snapped, Vector3.up, size);
        Handles.DrawLine(snapped, snapped + Vector3.up * size * 2f);
        sceneView.Repaint();

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (removeMode)
            {
                RemoveSpotNear(snapped);
            }
            else
            {
                PlaceSpot(template, snapped);
            }
            e.Use();
        }
    }

    private void PlaceSpot(GameObject template, Vector3 snapped)
    {
        GameObject parent = FindBuildableParent(true);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
        if (instance == null)
        {
            // Template is a plain scene object, not a prefab asset.
            instance = Instantiate(template);
        }
        instance.name = template.name;
        Undo.RegisterCreatedObjectUndo(instance, "Add Buildable Spot");
        Undo.SetTransformParent(instance.transform, parent.transform, "Add Buildable Spot");

        // Position so that the actual buildable plane (second-level child) lands on
        // the snapped grid cell, which is what the runtime checks against.
        Transform plane = instance.transform.childCount > 0 ? instance.transform.GetChild(0) : instance.transform;
        Vector3 planeOffset = plane.position - instance.transform.position;
        instance.transform.position = snapped - planeOffset;

        Selection.activeGameObject = instance;
        EditorUtility.SetDirty(parent);
    }

    private void RemoveSpotNear(Vector3 snapped)
    {
        GameObject parent = FindBuildableParent(false);
        if (parent == null)
        {
            return;
        }

        Transform closest = null;
        float closestDist = 0.6f; // a bit above the runtime tolerance (0.5)
        foreach (Transform spot in parent.transform)
        {
            Transform plane = spot.childCount > 0 ? spot.GetChild(0) : spot;
            float d = Vector3.Distance(snapped, plane.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = spot;
            }
        }

        if (closest != null)
        {
            Undo.DestroyObjectImmediate(closest.gameObject);
        }
    }

    private GameObject ResolveTemplate(GameObject parent)
    {
        if (spotTemplate != null)
        {
            return spotTemplate;
        }
        // Auto-detect: first existing spot under the Buildable parent.
        if (parent != null && parent.transform.childCount > 0)
        {
            return parent.transform.GetChild(0).gameObject;
        }
        return null;
    }

    private static GameObject FindBuildableParent(bool createIfMissing)
    {
        GameObject parent = GameObject.FindGameObjectWithTag("Buildable");
        if (parent == null && createIfMissing)
        {
            parent = new GameObject("BuildAblePlaces") { tag = "Buildable" };
            GridManager grid = FindGrid();
            if (grid != null)
            {
                parent.transform.position = grid.transform.position;
            }
            Undo.RegisterCreatedObjectUndo(parent, "Create Buildable Parent");
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
    /// Resolves the world point under the cursor: first tries scene colliders,
    /// then falls back to a horizontal plane at the grid's height.
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
}
