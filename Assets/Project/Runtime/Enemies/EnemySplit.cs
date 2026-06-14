using UnityEngine;

public class EnemySplit : MonoBehaviour
{
     [Header("Split Settings")]
    public GameObject enemyPrefab;
    public float sizeMultiplier = 0.7f; // 70% der ursprünglichen Größe
    public float minSize = 0.3f;        // Ab dieser Größe nicht mehr teilen

    [Header("Movement")]
    public float splitForce = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            Split();

            Destroy(other.gameObject); // Projektil entfernen
        }
    }

    private void Split()
    {
        float currentSize = transform.localScale.x;

        // Zu klein? Dann einfach zerstören
        if (currentSize <= minSize)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 newScale = transform.localScale * sizeMultiplier;

        // Gegner links erzeugen
        GameObject enemy1 = Instantiate(
            enemyPrefab,
            transform.position + Vector3.left * 0.3f,
            Quaternion.identity);

        // Gegner rechts erzeugen
        GameObject enemy2 = Instantiate(
            enemyPrefab,
            transform.position + Vector3.right * 0.3f,
            Quaternion.identity);

        enemy1.transform.localScale = newScale;
        enemy2.transform.localScale = newScale;

        // Falls Rigidbody2D vorhanden
        Rigidbody rb1 = enemy1.GetComponent<Rigidbody>();
        Rigidbody rb2 = enemy2.GetComponent<Rigidbody>();

        if (rb1 != null)
            rb1.linearVelocity = new Vector2(-splitForce, Random.Range(-1f, 1f));

        if (rb2 != null)
            rb2.linearVelocity = new Vector2(splitForce, Random.Range(-1f, 1f));

        Destroy(gameObject);
    }
}
