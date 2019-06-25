using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextLabelFader : MonoBehaviour
{
    public enum FadeType
    {
        MoveUp,
        ZoomIn
    }

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

    public void StartFade(bool withRandomStartOffset, FadeType fadeType)
    {
        AnimationCfg animCfg = Root.Instance.ConfigManager.Animation;

        if (withRandomStartOffset)
        {
            Vector2 randomUnitCircle = UnityEngine.Random.insideUnitCircle.normalized * animCfg.textFloatRandomOffsetRadius;
            rectTransform.localPosition += new Vector3(randomUnitCircle.x, randomUnitCircle.y, 0);
        }


        textLabel.DOFade(0, animCfg.textChangeFloatDuration);


        switch (fadeType)
        {
            case FadeType.MoveUp:
                float targetY = rectTransform.localPosition.y + animCfg.textChangeFloatOffsetY;
                rectTransform.DOLocalMoveY(targetY, animCfg.textChangeFloatDuration).SetEase(animCfg.textChangeFloatEase)
                            .OnComplete(() => OnFadeCompleted());
                break;
            case FadeType.ZoomIn:
                float targetScale = rectTransform.localScale.x * animCfg.textChangeZoomInScale;               
                rectTransform.DOScale(targetScale, animCfg.textChangeFloatDuration).SetEase(animCfg.textChangeFloatEase)
                            .OnComplete(() => OnFadeCompleted());
                break;
            default:
                Debug.LogError($"Unknown fade type {fadeType}");
                break;
        }
    }
}
