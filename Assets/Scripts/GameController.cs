using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public event Action<ulong> OnScoreUpdated = (newScore) => { };
    public event Action<uint> OnComboUpdated = (newCombo) => { };
    public event Action<uint> OnMergeCombo = (mergeCombo) => { };
    public event Action OnGridCleared = () => { };

    public ulong Score => score;

    private AnimationCfg animationCfg;
    private BubblesConfig bubblesConfig;

    private ulong score = 0;
    private uint combo = 0;

    private void Start()
    {
        DOTween.SetTweensCapacity(500, 50);

        animationCfg = Root.Instance.ConfigManager.Animation;
        bubblesConfig = Root.Instance.ConfigManager.Bubbles;

        Root.Instance.BubbleCreator.Init();

        Root.Instance.Grid.Init();
        Root.Instance.Grid.OnBubblesMerged += OnBubblesMerged;
        Root.Instance.Grid.OnNothingMergedTurn += OnNothingMergedTurn;
        Root.Instance.Grid.OnGridCleared += () => OnGridCleared();
        Root.Instance.Grid.OnMergeCombo += (mergeCombo) => OnMergeCombo(mergeCombo);

        Root.Instance.Gun.Init();
    }

    private void OnNothingMergedTurn()
    {
        bool comboChanged = combo != 0;
        if (comboChanged)
        {
            combo = 0;
            OnComboUpdated(combo);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //combo = (uint)Mathf.Min(bubblesConfig.maxCombo, combo + 1);
            //uint scoreBonus = (uint)Mathf.Pow(2, mergedBubblePower) * combo;
            //score *= 2;
            //OnScoreUpdated(score);
            //OnComboUpdated(combo);

            //OnGridCleared();
            //OnMergeCombo(2);
        }
    }

    private void OnBubblesMerged(int mergedBubblePower)
    {
        combo = (uint)Mathf.Min(bubblesConfig.maxCombo, combo + 1);
        uint scoreBonus = (uint)Mathf.Pow(2, mergedBubblePower) * combo;
        score += scoreBonus;
        OnScoreUpdated(score);
        OnComboUpdated(combo);
    }
}
