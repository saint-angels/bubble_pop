using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "AnimationCfg", menuName = "Configs/AnimationCfg")]
public class AnimationCfg : ScriptableObject
{
    [Header("Shooting")]
    public float shootBubbleFlyDuration = .2f;
    public Ease shootBubbleFlyEase = Ease.InOutQuint;

    [Header("Bubbles")]
    public float bubbleFallDuration = 1f;
    public Ease bubbleFallEase = Ease.InOutQuint;

    public float bubbleMergeDuration = .5f;
    public Ease bubbleMergeEase = Ease.InOutQuint;

    public float bubbleMergeTargetScale = .5f;

    public float bubbleShiftDuration = .5f;
    public Ease bubbleShiftEase = Ease.InOutQuint;

    public float bubbleChangeColorDuration = .5f;
    public Ease bubbleChangeColorEase = Ease.InOutQuint;

    [Header("Text Labels")]
    public float textChangeFloatOffsetY = 1f;
    public float textFloatRandomOffsetRadius = 1f;
    public float textChangeFloatDuration = .5f;
    public Ease textChangeFloatEase = Ease.InOutQuint;
}
