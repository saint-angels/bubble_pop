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

    public enum BubbleState
    {
        GUN,
        GUN_ALT,
        GRID,
        DYING
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
    [SerializeField] private TrailRenderer trail = null;

    [Header("VFX")]
    [SerializeField] private ParticleEffectBase vfxExplosion = null;

    private AnimationCfg animationCfg;
    private BubblesConfig bubblesConfig;

    private Vector2Int[] neighbourOffsets = new Vector2Int[]
    {
        new Vector2Int(2,0),
        new Vector2Int(1,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-2,0),
        new Vector2Int(-1,1),
        new Vector2Int(1,1),
    };

    public void SetState(BubbleState newState)
    {
        Color targetColor;

        switch (newState)
        {
            case BubbleState.GUN:
                SetInteractible(false);
                targetColor = bubblesConfig.bubbleGunColor;
                trail.gameObject.SetActive(true);
                break;
            case BubbleState.GUN_ALT:
                SetInteractible(true);
                targetColor = bubblesConfig.bubbleGunAltColor;
                trail.gameObject.SetActive(false);
                break;
            case BubbleState.GRID:
                SetInteractible(true);
                targetColor = bubblesConfig.bubbleGridColor;
                trail.gameObject.SetActive(false);
                break;
            case BubbleState.DYING:
                SetInteractible(false);
                targetColor = bubblesConfig.bubbleGridColor;
                trail.gameObject.SetActive(false);
                break;
            default:
                Debug.LogWarning($"Unknown state {newState}");
                targetColor = Color.white;
                break;
        }

        renderer.DOKill();
        renderer.DOColor(targetColor, animationCfg.bubbleChangeColorDuration).SetEase(animationCfg.bubbleChangeColorEase);
    }

    public void OnGunBubbleAttached(int neighbourX, int neighbourY)
    {
        Vector3 recoilOffset = new Vector3(X - neighbourX, Y - neighbourY, 0) * animationCfg.bubbleShotRecoilOffset;
        transform.DOBlendableLocalMoveBy(recoilOffset, animationCfg.bubbleShotRecoilDuration).SetEase(animationCfg.bubbleRecoilEase, 2, 0);
    }

    public void SetGridPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Init(int power, BubbleState bubbleState)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;

        Power = power;
        SetState(bubbleState);
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
        SetState(BubbleState.DYING);
        transform.DOKill();

        switch (deathType)
        {
            case BubbleDeathType.SILENT:
                Die();
                break;
            case BubbleDeathType.EXPLOSION:
                ParticleEffectBase newParticles = ObjectPool.Spawn<ParticleEffectBase>(vfxExplosion, transform.position, Quaternion.identity);
                newParticles.Init(bubblesConfig.ExplosionColorForPower(Power));
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

    private void Awake()
    {
        this.animationCfg = Root.Instance.ConfigManager.Animation;
        this.bubblesConfig = Root.Instance.ConfigManager.Bubbles;
    }

    private void Die()
    {
        OnDeath(this);
        ObjectPool.Despawn<Bubble>(this);
    }
}
