using Promises;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectBase : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles = null;
    [SerializeField] private new ParticleSystemRenderer renderer = null;

    public void Init(Color color)
    {
        ParticleSystem.MainModule settings = particles.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
        particles.Play();
        Timers.Instance.WaitUnscaled(1)
            .Done(() => ObjectPool.Despawn<ParticleEffectBase>(this));
    }
}
