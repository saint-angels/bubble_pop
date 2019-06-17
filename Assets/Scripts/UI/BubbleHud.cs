using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHud : FadingTextBase
{
    public void Init(int bubblePower)
    {
        string bubbleNumberString = NumberFormatHelper.FormatNumberBubble(bubblePower);
        Init(bubbleNumberString);
    }

    protected override void OnFadeCompleted()
    {
        ObjectPool.Despawn<BubbleHud>(this);
    }
}
