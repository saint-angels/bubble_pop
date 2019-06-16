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
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.localPosition = newPosition;
    }
}
