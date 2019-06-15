using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHud : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI numberLabel = null;
    [SerializeField] private RectTransform rectTransform = null;

    public void Init(uint bubbleNumber)
    {
        numberLabel.text = NumberFormatHelper.FormatNumberBubble(bubbleNumber);
    }

    public void SetPosition(Vector2 newPosition)
    {
        //TODO: Make it anchor-independent
        rectTransform.anchoredPosition = newPosition;
        
    }
}
