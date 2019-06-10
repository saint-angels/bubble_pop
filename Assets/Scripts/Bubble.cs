using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum BubbleSide
    {
        LEFT,
        RIGHT,
        BOTTOM
    }

    public enum BubbleDeathType
    {
        EXPLOSION,
        FALL
    }

    [System.Serializable]
    private struct SidePoint
    {
        public BubbleSide sideType;
        public Transform point;
    }

    public event Action<Bubble> OnUpgrade = (bubble) => { };
    public event Action<Bubble> OnDeath = (bubble) => { };

    public BubbleType Type { get; private set; }
    public Vector2Int Coords { get; set; }

    [SerializeField] private new Collider2D collider;
    [SerializeField] private SidePoint[] bubbleSidePoints;
    [SerializeField] private new SpriteRenderer renderer;

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

    public void Init(BubbleType bubbleType, AnimationCfg animationCfg, bool interactible)
    {
        this.animationCfg = animationCfg;
        Type = bubbleType;
        renderer.color = Type.color;
        SetInteractible(interactible);
    }

    public void Upgrade(BubbleType newBubbleType)
    {
        Type = newBubbleType;
        renderer.color = Type.color;
        OnUpgrade(this);
    }

    public void SetInteractible(bool isInteractible)
    {
        collider.enabled = isInteractible;
    }

    public Vector2Int[] GetAttachSlotsPositions()
    {
        Vector2Int[] slots = new Vector2Int[neighbourOffsets.Length];

        for (int i = 0; i < neighbourOffsets.Length; i++)
        {
            slots[i] = Coords + neighbourOffsets[i];
        }

        return slots;
    }

    public void Die(BubbleDeathType deathType)
    {
        SetInteractible(false);

        switch (deathType)
        {
            case BubbleDeathType.EXPLOSION:
                Death();
                break;
            case BubbleDeathType.FALL:
                var fallTween = transform.DOMove(transform.position + Vector3.down * 10f, animationCfg.bubbleFallDuration).SetEase(Ease.InOutQuint);
                fallTween.OnComplete(() =>
                {
                    Death();
                });
                break;
            default:
                Debug.LogError($"Unknown death type {deathType}");
                break;
        }
    }

    private void Death()
    {
        OnDeath(this);
        ObjectPool.Despawn<Bubble>(this);
    }
}
