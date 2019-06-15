using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public event Action<uint> OnScoreUpdated = (newScore) => { };

    public uint Score => score;

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

    private void OnBubblesMerged(uint bubbleNumber)
    {
        score += bubbleNumber;
        OnScoreUpdated(score);
    }
}
