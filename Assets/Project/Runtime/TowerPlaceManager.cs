using System.Collections.Generic;
using UnityEngine;

public class TowerPlaceManager : MonoBehaviour
{
    public static TowerPlaceManager Instance;

    [SerializeField]
    GameObject[] towerPrefab;

    [SerializeField]
    GameObject[] buildableSpots;

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
        canBePlaced = false;
    }

    void Update()
    {
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
                if (canBePlaced == true)
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString("#DDB572", out color))
                    {
                        currentHighlight.GetComponent<Renderer>().material.color = color;
                    }

                    Debug.Log("CanBePlaced");
                }
                else
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString("#DD8472", out color))
                    {
                        currentHighlight.GetComponent<Renderer>().material.color = color;
                    }
                }

                //snappedPos.y = hit.point.y;

                if (Input.GetMouseButtonDown(0)) // Linksklick
                {
                    if (canBePlaced == true && IsCellFree(snappedPos))
                    {
                        PlaceTower();
                        canBePlaced = false;
                        Destroy(currentHighlight);
                        currentTower.transform.position = snappedPos;
                        currentTower = null; // Tower platzieren
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    foreach (GameObject spot in buildableSpots)
                    {
                        spot.SetActive(false);
                    }
                    canBePlaced = false;
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

    private void PlaceTower()
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

        //Geld abziehen von Singlketon Sprocket Börse
    }

    bool IsCellFree(Vector3 snappedPos)
    {
        int currentTowerCounter = 0;
        Collider[] hits = Physics.OverlapBox(snappedPos, new Vector3(0.4f, 0.4f, 0.4f));
        if (hits.Length == 0)
        {
            return true;
        }
        else
        {
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Tower"))
                {
                    currentTowerCounter++;
                }
            }
            if (currentTowerCounter > 1)
            {
                return false;
            }
            return true;
        }
    }
}
