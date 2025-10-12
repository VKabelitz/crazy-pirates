using UnityEngine;

public class Lightning : MonoBehaviour
{


    public Light lightningLight;
    public float minDelay = 3f;
    public float maxDelay = 8f;
    public float flashDuration = 0.2f;
    //public AudioSource thunderSound;

    private void Start()
    {
        if (lightningLight == null)
            lightningLight = GetComponent<Light>();

        lightningLight.enabled = false;
        StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        while (true)
        {
            // Warte zufällig zwischen Blitzen
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Ein kurzer Blitz mit kleinen Flackern
            for (int i = 0; i < Random.Range(2, 4); i++)
            {
                lightningLight.enabled = true;
                lightningLight.intensity = Random.Range(2f, 5f);
                yield return new WaitForSeconds(Random.Range(0.05f, flashDuration));
                lightningLight.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
            }

            // Donner mit Verzögerung (optional)
            //if (thunderSound != null)
              //  StartCoroutine(PlayThunder());
        }
    }

    System.Collections.IEnumerator PlayThunder()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Verzögerung zwischen Blitz & Donner
        //thunderSound.Play();
    }
}

