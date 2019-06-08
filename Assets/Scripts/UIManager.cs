using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private BubbleHud bubbleHudPrefab = null;
    [SerializeField] private Transform bubbleHudsContainer = null;

    private List<BubbleHud> bubbleHuds = new List<BubbleHud>();

    public void AddHudToBubble(Bubble bubble)
    {
        Vector3 bubbleCanvasPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, bubble.transform.position);

        BubbleHud newHud = ObjectPool.Spawn(bubbleHudPrefab, Vector3.zero, Quaternion.identity, bubbleHudsContainer);
        newHud.SetPosition(bubbleCanvasPosition);
    }

    void LateUpdate()
    {
        foreach (var bubbleHud in bubbleHuds)
        {
            //Update position
        }        
    }
}
