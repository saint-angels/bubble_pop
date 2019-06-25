using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectLabel : FadingTextBase
{
    public override void Init(string newText)
    {
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = Vector2.zero;
        base.Init(newText);
    }

    protected override void OnFadeCompleted()
    {
        ObjectPool.Despawn<PerfectLabel>(this);
    }
}
