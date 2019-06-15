using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
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

    public uint Number { get; private set; }

    public int X { get; set; }
    public int Y { get; set; }

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

    public void SetGridPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Init(uint bubbleNumber, bool interactible)
    {
        this.animationCfg = Root.Instance.ConfigManager.Animation;
        Number = bubbleNumber;
        renderer.color = Root.Instance.ConfigManager.Bubbles.ColorForNumber(Number);
        SetInteractible(interactible);
    }

    public void Upgrade(uint newNumber)
    {
        Number = newNumber;
        renderer.color = Root.Instance.ConfigManager.Bubbles.ColorForNumber(Number);
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
