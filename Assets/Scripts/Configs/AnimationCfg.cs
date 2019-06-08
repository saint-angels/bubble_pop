using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCfg", menuName = "Configs/AnimationCfg")]
public class AnimationCfg : ScriptableObject
{
    [Header("Shooting")]
    public float shootBubbleFlyDuration = .2f;

    [Header("Bubbles")]
    public float bubbleFallDuration = 1f;
}
