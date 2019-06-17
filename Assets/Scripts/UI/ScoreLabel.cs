using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreLabel : FadingTextBase
{
    public override void Init(string newText)
    {
        rectTransform.anchoredPosition = Vector2.zero;
        base.Init(newText);
    }

    protected override void OnFadeCompleted()
    {
        ObjectPool.Despawn<ScoreLabel>(this);
    }
}
