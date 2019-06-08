using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCfg", menuName = "Config/AnimationCfg")]
public class AnimationCfg : ScriptableObject
{
    [Header("Shooting")]
    public float shootBubbleFlyDuration = .2f;
}
