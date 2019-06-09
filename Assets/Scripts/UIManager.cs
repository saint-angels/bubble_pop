using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] private BubbleHud bubbleHudPrefab = null;
    [SerializeField] private RectTransform bubbleHudsContainer = null;

    private Dictionary<Bubble, BubbleHud> bubbleHuds = new Dictionary<Bubble, BubbleHud>();

    public void AddHudToBubble(Bubble bubble)
    {
        Vector3 bubbleCanvasPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, bubble.transform.position);

        BubbleHud newHud = ObjectPool.Spawn(bubbleHudPrefab, Vector3.zero, Quaternion.identity, bubbleHudsContainer);
        newHud.Init(bubble.Type);
        newHud.SetPosition(bubbleCanvasPosition);

        bubbleHuds.Add(bubble, newHud);

        bubble.OnDeath += Bubble_OnDeath;
    }

    private void Bubble_OnDeath(Bubble bubble)
    {
        Destroy(bubbleHuds[bubble].gameObject);
        bubbleHuds.Remove(bubble);
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

    private Vector2 WorldToCanvasPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, out result);
        return canvas.transform.TransformPoint(result);
    }
}
