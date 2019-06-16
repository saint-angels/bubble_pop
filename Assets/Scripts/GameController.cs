using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public event Action<uint> OnScoreUpdated = (newScore) => { };
    public event Action<uint> OnComboUpdated = (newCombo) => { };

    public uint Score => score;

    private AnimationCfg animationCfg;
    private BubblesConfig bubblesConfig;

    private uint score = 0;
    private uint combo = 0;

    private void Start()
    {
        animationCfg = Root.Instance.ConfigManager.Animation;
        bubblesConfig = Root.Instance.ConfigManager.Bubbles;


        Root.Instance.Grid.Init();
        Root.Instance.Grid.OnBubblesMerged += OnBubblesMerged;
        Root.Instance.Grid.OnNothingMergedTurn += OnNothingMergedTurn;

        Root.Instance.Gun.Init();
    }

    private void OnNothingMergedTurn()
    {
        combo = 0;
        OnComboUpdated(combo);

    }

    private void OnBubblesMerged(int mergedBubblePower)
    {
        combo = (uint)Mathf.Min(bubblesConfig.maxCombo, combo + 1);
        uint scoreBonus = (uint)Mathf.Pow(2, mergedBubblePower) * combo;
        //print($"Score bonus {scoreBonus}");
        score += scoreBonus;
        OnScoreUpdated(score);
        OnComboUpdated(combo);
    }
}
