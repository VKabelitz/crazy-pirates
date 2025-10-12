using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    private Light lightSource;

    [Header("Flicker Settings")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.1f; // wie oft es flackert
    public bool smooth = true;        // sanftes oder hartes Flackern

    private float targetIntensity;

    void Start()
    {
        lightSource = GetComponent<Light>();
        targetIntensity = lightSource.intensity;
        StartCoroutine(FlickerRoutine());
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);

            if (smooth)
            {
                // Sanft zum neuen Wert überblenden
                float t = 0f;
                float start = lightSource.intensity;
                while (t < 1f)
                {
                    t += Time.deltaTime / flickerSpeed;
                    lightSource.intensity = Mathf.Lerp(start, targetIntensity, t);
                    yield return null;
                }
            }
            else
            {
                // Direktes, unruhiges Flackern
                lightSource.intensity = targetIntensity;
                yield return new WaitForSeconds(flickerSpeed);
            }
        }
    }
}