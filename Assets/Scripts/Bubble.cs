﻿using DG.Tweening;
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

    [System.Serializable]
    private struct SidePoint
    {
        public BubbleSide sideType;
        public Transform point;
    }

    public event Action<Bubble> OnUpgrade = (bubble) => { };
    public event Action<Bubble> OnDeath = (bubble) => { };

    public BubbleType Type { get; private set; }

    [SerializeField] private new Collider2D collider;
    [SerializeField] private SidePoint[] bubbleSidePoints;
    [SerializeField] private new SpriteRenderer renderer;

    private AnimationCfg animationCfg;

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
    
    public BubbleSide ClosestSideToPoint(Vector3 point)
    {
        float minSqDistance = float.MaxValue;
        BubbleSide closestSide = BubbleSide.BOTTOM;

        for (int i = 0; i < bubbleSidePoints.Length; i++)
        {
            float sqDistance = Vector3.SqrMagnitude(point - bubbleSidePoints[i].point.position);
            if (sqDistance < minSqDistance)
            {
                closestSide = bubbleSidePoints[i].sideType;
                minSqDistance = sqDistance;
            }
        }
        return closestSide;
    }

    public void Fall()
    {
        var fallTween = transform.DOMove(transform.position + Vector3.down * 10f, animationCfg.bubbleFallDuration).SetEase(Ease.InOutQuint);
        fallTween.OnComplete(() =>
        {
            Die();
        });
    }

    public void Explode()
    {
        Die();
    }

    private void Die()
    {
        OnDeath(this);
        ObjectPool.Despawn<Bubble>(this);
    }
}
