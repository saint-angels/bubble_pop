using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHud : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI numberLabel = null;
    [SerializeField] private RectTransform rectTransform = null;

    public void Init(BubbleType bubbleType)
    {
        numberLabel.text = NumberFormatHelper.FormatNumberBubble(bubbleType.number);
    }

    public void SetPosition(Vector2 newPosition)
    {
        //TODO: Make it anchor-independent
        rectTransform.anchoredPosition = newPosition;
        
    }
}
