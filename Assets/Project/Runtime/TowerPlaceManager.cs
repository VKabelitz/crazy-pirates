using System.Collections.Generic;
using UnityEngine;

public class TowerPlaceManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] towerPrefab;

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

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (currentTower != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                Vector3 towerPosition = hit.point;
                towerPosition.y = towerHeight; // Setze die y-Position fix
                currentTower.transform.position = towerPosition; // Tower positionieren

                if (Input.GetMouseButtonDown(0)) // Linksklick
                {
                    //noch einbauen dass dann das Geld abgezogen wird
                    PlaceTower();
                    currentTower = null; // Tower platzieren
                }
                else if (Input.GetMouseButtonDown(1))
                {
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
}
