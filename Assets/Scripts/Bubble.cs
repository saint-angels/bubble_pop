using DG.Tweening;
using Promises;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Bubble : MonoBehaviour
{
    public enum BubbleDeathType
    {
        SILENT,
        EXPLOSION,
        DROP
    }

    public event Action<Bubble> OnUpgrade = (bubble) => { };
    public event Action<Bubble> OnDeath = (bubble) => { };

    //Power of 2
    public int Power { get; private set; }

    public int X { get; set; }
    public int Y { get; set; }

    [SerializeField] private new Collider2D collider = null;
    [SerializeField] private new SpriteRenderer renderer = null;
    [SerializeField] private Rigidbody2D rb = null;

    [Header("VFX")]
    [SerializeField] private ParticleEffectBase vfxExplosion = null;

    private AnimationCfg animationCfg;

    private Vector2Int[] neighbourOffsets = new Vector2Int[]
    {
        new Vector2Int(2,0),
        new Vector2Int(1,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-2,0),
        new Vector2Int(-1,1),
        new Vector2Int(1,1),

    };

    public void SetGridPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Init(int power, bool interactible, bool gunBubble)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;

        this.animationCfg = Root.Instance.ConfigManager.Animation;
        Power = power;
        renderer.color = gunBubble ? Root.Instance.ConfigManager.Bubbles.bubbleGunColor : Root.Instance.ConfigManager.Bubbles.bubbleGridColor;
        SetInteractible(interactible);
    }

    public void Upgrade(int newNumber)
    {
        Power = newNumber;
        renderer.color = Root.Instance.ConfigManager.Bubbles.bubbleGridColor;
        OnUpgrade(this);
    }

    public void SetInteractible(bool isInteractible)
    {
        collider.enabled = isInteractible;
    }

    public Vector2Int[] GetNeighbourSlotIndeces()
    {
        Vector2Int[] slots = new Vector2Int[neighbourOffsets.Length];

        for (int i = 0; i < neighbourOffsets.Length; i++)
        {
            slots[i] = new Vector2Int(X, Y) + neighbourOffsets[i];
        }

        return slots;
    }

    public void Die(BubbleDeathType deathType)
    {
        SetInteractible(false);
        transform.DOKill();

        switch (deathType)
        {
            case BubbleDeathType.SILENT:
                Die();
                break;
            case BubbleDeathType.EXPLOSION:
                ParticleEffectBase newParticles = ObjectPool.Spawn<ParticleEffectBase>(vfxExplosion, transform.position, Quaternion.identity);
                newParticles.Init(Root.Instance.ConfigManager.Bubbles.ColorForPower(Power));
                Die();
                break;
            case BubbleDeathType.DROP:
                rb.bodyType = RigidbodyType2D.Dynamic;
                //TODO: Move to config?
                Vector2 force = Vector2.up * 3 + Vector2.right * (UnityEngine.Random.value < .5f ? 1 : -1) * 3f;
                rb.AddForce(force, ForceMode2D.Impulse);

                //Wait until bubble falls under the screen
                Timers.Instance.WaitForTrue(() => { return transform.position.y < -10f; })
                    .Done(() => Die()) ;
                break;
            default:
                Debug.LogError($"Unknown death type {deathType}");
                break;
        }
    }

    private void Die()
    {
        OnDeath(this);
        ObjectPool.Despawn<Bubble>(this);
    }
}
