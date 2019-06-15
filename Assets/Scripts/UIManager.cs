﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] private BubbleHud bubbleHudPrefab = null;
    [SerializeField] private RectTransform bubbleHudsContainer = null;

    [SerializeField] private TMPro.TextMeshProUGUI scoreLabel = null;

    private Dictionary<Bubble, BubbleHud> bubbleHuds = new Dictionary<Bubble, BubbleHud>();

    public void AddHudToBubble(Bubble bubble)
    {
        Vector3 bubbleCanvasPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, bubble.transform.position);

        BubbleHud newHud = ObjectPool.Spawn(bubbleHudPrefab, Vector3.zero, Quaternion.identity, bubbleHudsContainer);
        newHud.Init(bubble.Type);
        newHud.SetPosition(bubbleCanvasPosition);

        bubbleHuds.Add(bubble, newHud);

        bubble.OnDeath -= Bubble_OnDeath;
        bubble.OnDeath += Bubble_OnDeath;

        bubble.OnUpgrade -= Bubble_OnUpgrade;
        bubble.OnUpgrade += Bubble_OnUpgrade;
    }

    private void Bubble_OnUpgrade(Bubble bubble)
    {
        bubbleHuds[bubble].Init(bubble.Type);
    }

    private void Bubble_OnDeath(Bubble bubble)
    {
        Destroy(bubbleHuds[bubble].gameObject);
        bubbleHuds.Remove(bubble);
    }


    private Vector2 WorldToCanvasPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, out result);
        return canvas.transform.TransformPoint(result);
    }

    private void OnScoreUpdated(uint newScore)
    {
        scoreLabel.text = NumberFormatHelper.FormatNumberScore(newScore * newScore);
    }

    private void Start()
    {
        Root.Instance.GameController.OnScoreUpdated += OnScoreUpdated;
        //Clear the score on start
        OnScoreUpdated(0);
    }
    
    void LateUpdate()
    {
        foreach (var bubbleHudPair in bubbleHuds)
        {
            Bubble bubble = bubbleHudPair.Key;
            BubbleHud hud = bubbleHudPair.Value;
            Vector2 canvasPos = WorldToCanvasPosition(canvas, bubbleHudsContainer, Camera.main, bubble.transform.position);
            hud.SetPosition(canvasPos);
        }
    }

}
