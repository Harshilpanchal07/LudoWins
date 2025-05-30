using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    // Singleton instance
    public static ParticleEffectManager Instance;
    public GameObject particleEffectPrefab;
    public float effectScale = 0.3f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void TriggerEffect(Vector3 position)
    {
        //Instantiate(particleEffectPrefab, position, Quaternion.identity);

        // Instantiate the particle effect
        GameObject effect = Instantiate(particleEffectPrefab, position, Quaternion.identity);

        // Adjust the size of the particle effect
        effect.transform.localScale = new Vector3(effectScale, effectScale, effectScale);
    }
}

