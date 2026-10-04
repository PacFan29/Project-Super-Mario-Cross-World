using UnityEngine;

public class EffectShader : MonoBehaviour
{
    ParticleSystemRenderer m_ParticleSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystemRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.isPaused)
        {
            m_ParticleSystem.material.SetFloat("_UnscaledTime", Time.time);
        }
        else
        {
            m_ParticleSystem.material.SetFloat("_UnscaledTime", Time.unscaledTime);
        }
    }
}
