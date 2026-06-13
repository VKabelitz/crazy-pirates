using System.Collections.Generic;
using UnityEngine;


public class TowerPlaceManager : MonoBehaviour
{
    public static TowerPlaceManager Instance;

    [SerializeField]
    GameObject[] towerPrefab;

    private double tol = 0.5;

    [SerializeField]
    List<GameObject> buildableSpots = new List<GameObject>();
    List<GameObject> takenSpots = new List<GameObject>();

    [SerializeField]
    LayerMask groundMask;

    [SerializeField]
    float towerHeight = 0.89f;

    [SerializeField]
    float transparentAlpha = 0.4f;
    private GameObject currentTower;
    private Camera mainCamera;
    private List<Renderer> towerRenderers = new List<Renderer>();
    private List<Color[]> originalColors = new List<Color[]>();
    private Color red = new Color(1f, 0f, 0f);
    private Color orange = new Color(1f, 0.7f, 0f);
    public GameObject highlightPrefab;

    private GameObject currentHighlight;

    public bool canBePlaced;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;

        buildableSpots = new List<GameObject>(); // somehow this is already filled.
        
        // Automatically find buildable spots
        GameObject buildableParent = GameObject.FindGameObjectWithTag("Buildable");
        
        if (buildableParent != null)
        {
            // Loop through all first-level children (spots)
            foreach (Transform spot in buildableParent.transform)
            {
                // Loop through second-level children (planes)
                foreach (Transform plane in spot)
                {
                    buildableSpots.Add(plane.gameObject);
                }
            }

            foreach (GameObject spot in buildableSpots)
            {
                spot.SetActive(false);
            }
            Debug.Log($"[TowerPlaceManager] Found {buildableSpots.Count} buildable spots in the scene.");
        }
        else
        {
            Debug.LogWarning("[TowerPlaceManager] No GameObject with tag 'Buildable' found in the scene!");
        }
    }
    void DrawDebugAt(Vector3 pos, Color color)
    {
        Debug.DrawRay(pos, Vector3.up * 2f, color);
        Debug.DrawLine(pos + Vector3.left * 0.5f, pos + Vector3.right * 0.5f, color);
        Debug.DrawLine(pos + Vector3.forward * 0.5f, pos + Vector3.back * 0.5f, color);
    }

    void OnGUI()
    {
        // Hol die aktuelle Mausposition (Y-Achse muss für GUI invertiert werden)
        Vector3 mousePos = Input.mousePosition;
        float guiX = mousePos.x;
        float guiY = Screen.height - mousePos.y;

        // Größe des Debug-Fadenkreuzes
        float size = 20f;
        
        // Setze die Farbe für die GUI-Linien
        GUI.color = Color.red;

        // Horizontale Linie des Fadenkreuzes
        GUI.DrawTexture(new Rect(guiX - size / 2, guiY, size, 2), Texture2D.whiteTexture);
        // Vertikale Linie des Fadenkreuzes
        GUI.DrawTexture(new Rect(guiX, guiY - size / 2, 2, size), Texture2D.whiteTexture);

        // Text mit den genauen Pixelkoordinaten daneben schreiben
        GUI.Label(new Rect(guiX + 15, guiY - 10, 200, 20), $"Mouse: ({mousePos.x:F0}, {mousePos.y:F0})");
    }
    
    void Update()
    {
        foreach (GameObject spot in buildableSpots)
        {
            Debug.DrawRay(spot.transform.position, Vector3.up * 100f, Color.yellow);    
        }
        Ray rayy = mainCamera.ScreenPointToRay(Input.mousePosition);
        // Zeichnet den tatsächlichen Mausstrahl im Scene-View (100 Meter lang)
        Debug.DrawRay(rayy.origin, rayy.direction * 100f, Color.yellow);
        if (Physics.Raycast(rayy, out RaycastHit hitt, Mathf.Infinity, groundMask))
        {
            DrawDebugAt(hitt.point, Color.blue);
        }

        if (currentTower != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                Vector3 towerPosition = hit.point;
                
                // Setze die y-Position fix
                towerPosition.y = towerHeight;
                currentTower.transform.position = towerPosition; // Tower positionieren
                Vector3 snappedPos = GridManager.Instance.GetSnappedPosition(hit.point);

                DrawDebugAt(snappedPos, Color.red);
                if (currentHighlight == null)
                {
                    currentHighlight = Instantiate(
                        highlightPrefab,
                        snappedPos,
                        Quaternion.identity
                    );
                }
                else
                {
                    currentHighlight.transform.position = snappedPos;
                }

                // check whether the thing can be placed. It can be placed when
                // the grid of the current position is empty.
                GameObject validSpot = IsCellFree(snappedPos);
                canBePlaced = validSpot != null;
                Color color = canBePlaced ? orange : red;
                currentHighlight.GetComponent<Renderer>().material.color = color;

                if (canBePlaced && Input.GetMouseButtonDown(0)) // Linksklick
                {
                    
                    PlaceTower(validSpot);
                    Destroy(currentHighlight);
                    currentTower.transform.position = snappedPos;
                    currentTower = null; // Tower platzieren
                    
                }
                else if (canBePlaced && Input.GetMouseButtonDown(1))
                {
                    foreach (GameObject spot in buildableSpots) {
                        spot.SetActive(false);
                    }
                    Destroy(currentHighlight);
                    Destroy(currentTower); // Tower zerstören
                    currentTower = null;
                }
            }
        }
    }

    public void StartPlacingTower(int towerIndex)
    {
        if (towerPrefab == null)
        {
            return;
        }
        foreach (GameObject spot in buildableSpots)
        {
            spot.SetActive(true);
        }

        AudioManager.instance.PlaySound("click");
        currentTower = Instantiate(towerPrefab[towerIndex]); // erstelle einen Tower wenn keiner grade platziert wird
        if (currentTower.TryGetComponent<Tower>(out Tower tower))
        {
            ((MonoBehaviour)tower).enabled = false;
        }

        // Transparenz aktivieren
        towerRenderers.Clear();
        originalColors.Clear();

        foreach (Renderer r in currentTower.GetComponentsInChildren<Renderer>())
        {
            towerRenderers.Add(r);
            Color[] colors = new Color[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                colors[i] = r.materials[i].color;
                Color c = colors[i];
                c.a = transparentAlpha;
                r.materials[i].color = c;

                // wichtig: Shader muss Transparenz unterstützen!
                r.materials[i].SetFloat("_Surface", 1); // nur bei URP nötig
                r.materials[i].renderQueue = 3000; // Transparent-Queue
            }
            originalColors.Add(colors);
        }
    }

    private void PlaceTower(GameObject validSpot)
    {
        AudioManager.instance.PlaySound("turret_build");
        foreach (GameObject spot in buildableSpots)
        {
            spot.SetActive(false);
        }
        if (currentTower.TryGetComponent<Tower>(out Tower tower))
        {
            ((MonoBehaviour)tower).enabled = true;
            Debug.Log("Tower placed, cost: " + tower.GetSprocketCosts());
            SprocketManager.instance.SubstractSprocket(tower.GetSprocketCosts());
        }

        // Transparenz zurücksetzen
        for (int i = 0; i < towerRenderers.Count; i++)
        {
            for (int j = 0; j < towerRenderers[i].materials.Length; j++)
            {
                var mat = towerRenderers[i].materials[j];
                Color c = originalColors[i][j];
                mat.color = c;
            }
        }

        // move current spot from buildableSpot list
        buildableSpots.Remove(validSpot);
        takenSpots.Add(validSpot);
    }
    
    private void colorValidCells(Vector3 snappedPos)
    {
        
    } 
    GameObject IsCellFree(Vector3 snappedPos)
    {
        // Current cell is free if the position corresponds to one of the buildableSpots.
        
        foreach (GameObject spot in buildableSpots)
        {
            if(Vector3.Distance(snappedPos, spot.transform.position) < tol)
            {
                return spot;
            }
        }
        return null;
    }
}
