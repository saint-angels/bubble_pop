using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] private BubbleHud bubbleHudPrefab = null;
    [SerializeField] private RectTransform bubbleHudsContainer = null;

    [SerializeField] private TMPro.TextMeshProUGUI scoreLabel = null;
    [SerializeField] private TMPro.TextMeshProUGUI comboLabel = null;

    private Dictionary<Bubble, BubbleHud> bubbleHuds = new Dictionary<Bubble, BubbleHud>();

    private AnimationCfg animationCfg;

    public void AddHudToBubble(Bubble bubble)
    {
        BubbleHud newHud = ObjectPool.Spawn(bubbleHudPrefab, Vector3.zero, Quaternion.identity, bubbleHudsContainer);
        newHud.Init(bubble.Power);

        UpdateBubbleHudPosition(bubble, newHud);

        bubbleHuds.Add(bubble, newHud);

        bubble.OnDeath -= Bubble_OnDeath;
        bubble.OnDeath += Bubble_OnDeath;

        bubble.OnUpgrade -= Bubble_OnUpgrade;
        bubble.OnUpgrade += Bubble_OnUpgrade;
    }

    private void Bubble_OnUpgrade(Bubble bubble)
    {
        bubbleHuds[bubble].Init(bubble.Power);
    }

    private void Bubble_OnDeath(Bubble bubble)
    {
        bubbleHuds[bubble].FloatUp(true);
        //Destroy(bubbleHuds[bubble].gameObject);
        bubbleHuds.Remove(bubble);
    }

    private void OnScoreUpdated(uint newScore)
    {
        scoreLabel.text = NumberFormatHelper.FormatNumberScore(newScore);
    }
    private void OnComboUpdated(uint newCombo)
    {
        comboLabel.text = $"x{newCombo}";
    }

    //private void FloatUpTextLabel(TMPro.TextMeshProUGUI textLabel)
    //{
    //    RectTransform labelRectTransform = textLabel.GetComponent<RectTransform>();

    //    if (withRandomStartOffset)
    //    {
    //        Vector2 randomUnitCircle = Random.insideUnitCircle.normalized * animCfg.textFloatRandomOffsetRadius;
    //        rectTransform.localPosition += new Vector3(randomUnitCircle.x, randomUnitCircle.y, 0);
    //    }

    //    float targetY = rectTransform.localPosition.y + animCfg.textChangeFloatOffsetY;

    //    textLabel.DOFade(0, animCfg.textChangeFloatDuration);

    //    labelRectTransform.DOLocalMoveY(targetY, animCfg.textChangeFloatDuration).SetEase(animCfg.textChangeFloatEase)
    //            .OnComplete(() => ObjectPool.Despawn<BubbleHud>(this)); ;
    //}

    private void UpdateBubbleHudPosition(Bubble bubble, BubbleHud hud)
    {
        Vector2 screenPoint = Root.Instance.CameraController.WorldToScreenPoint(bubble.transform.position);
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bubbleHudsContainer, screenPoint, null, out localPoint))
        {
            hud.SetPosition(localPoint);
        }
    }

    private void Start()
    {
        animationCfg = Root.Instance.ConfigManager.Animation;


        Root.Instance.GameController.OnScoreUpdated += OnScoreUpdated;
        Root.Instance.GameController.OnComboUpdated += OnComboUpdated;

        //Clear the score & combo on start
        OnScoreUpdated(0);
        OnComboUpdated(0);
    }
    
    void LateUpdate()
    {
        foreach (var bubbleHudPair in bubbleHuds)
        {
            Bubble bubble = bubbleHudPair.Key;
            BubbleHud hud = bubbleHudPair.Value;
            UpdateBubbleHudPosition(bubble, hud);
        }
    }

}
