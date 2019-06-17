using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextLabelFader : MonoBehaviour
{
    public event Action OnFadeCompleted = () => { };

    private TMPro.TextMeshProUGUI textLabel;
    private RectTransform rectTransform;

    public void Init(TMPro.TextMeshProUGUI textLabel)
    {
        this.textLabel = textLabel;
        rectTransform = textLabel.GetComponent<RectTransform>();

        //Reset text color alpha
        Color textColor = textLabel.color;
        textColor.a = 1;
        textLabel.color = textColor;
    }

    public void StartFade(bool withRandomStartOffset)
    {
        AnimationCfg animCfg = Root.Instance.ConfigManager.Animation;

        if (withRandomStartOffset)
        {
            Vector2 randomUnitCircle = UnityEngine.Random.insideUnitCircle.normalized * animCfg.textFloatRandomOffsetRadius;
            rectTransform.localPosition += new Vector3(randomUnitCircle.x, randomUnitCircle.y, 0);
        }

        float targetY = rectTransform.localPosition.y + animCfg.textChangeFloatOffsetY;

        textLabel.DOFade(0, animCfg.textChangeFloatDuration);

        rectTransform.DOLocalMoveY(targetY, animCfg.textChangeFloatDuration).SetEase(animCfg.textChangeFloatEase)
                    .OnComplete(() => OnFadeCompleted()); ;
    }
}
