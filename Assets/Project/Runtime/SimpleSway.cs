using UnityEngine;

public class SimpleSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amplitude = 0.5f; // How far it moves
    public float frequency = 1f;   // How fast it moves

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float swayX = Mathf.Sin(Time.time * frequency) * amplitude;
        float swayY = Mathf.Cos(Time.time * frequency * 0.8f) * amplitude * 0.5f;

        transform.localPosition = startPos + new Vector3(swayX, swayY, 0f);
    }
}
