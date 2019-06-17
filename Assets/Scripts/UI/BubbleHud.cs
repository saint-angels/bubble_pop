using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHud : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI numberLabel = null;
    [SerializeField] private RectTransform rectTransform = null;
    

    public void Init(int bubblePower)
    {
        numberLabel.text = NumberFormatHelper.FormatNumberBubble(bubblePower);

        //Reset text color alpha
        Color textColor = numberLabel.color;
        textColor.a = 1;
        numberLabel.color = textColor;
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.localPosition = newPosition;
    }

    public void FloatUp(bool withRandomStartOffset)
    {
        var animCfg = Root.Instance.ConfigManager.Animation;

        if (withRandomStartOffset)
        {
            Vector2 randomUnitCircle = Random.insideUnitCircle.normalized * animCfg.textFloatRandomOffsetRadius;
            rectTransform.localPosition += new Vector3(randomUnitCircle.x, randomUnitCircle.y, 0);
        }

        float targetY = rectTransform.localPosition.y + animCfg.textChangeFloatOffsetY;
        
        numberLabel.DOFade(0, animCfg.textChangeFloatDuration);

        rectTransform.DOLocalMoveY(targetY, animCfg.textChangeFloatDuration).SetEase(animCfg.textChangeFloatEase)
                .OnComplete(() => ObjectPool.Despawn<BubbleHud>(this)); ;
    }
}
