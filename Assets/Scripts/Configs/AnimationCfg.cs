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

    public float bubbleShiftDuration = .5f;
    public Ease bubbleShiftEase = Ease.InOutQuint;
}
