using UnityEngine;

public class GridManager : MonoBehaviour
{

public int width = 10;
    public int depth = 10;
    public float cellSize = 1f;

    public static GridManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        // Erst lokale Position relativ zum Grid berechnen
        Vector3 localPos = worldPos - transform.position;

        float x = Mathf.Round(localPos.x / cellSize) * cellSize;
        float z = Mathf.Round(localPos.z / cellSize) * cellSize;

        // Dann wieder in Weltkoordinaten zurückrechnen
        return new Vector3(x, 0, z) + transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize) + transform.position;
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.01f, cellSize));
            }
        }
    }
   
}
