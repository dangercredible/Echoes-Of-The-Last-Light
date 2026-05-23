using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    public UnityEngine.ParticleSystem explosionEffect;

    public void Start()
    {
        Explode();
    }

    public void Explode()
    {
            explosionEffect.Play();
    }
}
