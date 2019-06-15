using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public event Action<uint> OnScoreUpdated = (newScore) => { };

    [Header("Configs")]
    [SerializeField] private AnimationCfg animationCfg;
    [SerializeField] private BubblesConfig bubblesConfig;

    private uint score = 0;

    private void Start()
    {
        Root.Instance.Grid.Init();
        Root.Instance.Grid.OnBubblesMerged += OnBubblesMerged;

        Root.Instance.Gun.Init();
    }

    private void OnBubblesMerged(BubbleType bubbleType)
    {
        score += bubbleType.number;
        OnScoreUpdated(score);
    }
}
