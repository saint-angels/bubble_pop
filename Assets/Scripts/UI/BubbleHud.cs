using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHud : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;

    public void SetPosition(Vector2 newPosition)
    {
        rectTransform.anchoredPosition = newPosition;
    }
}
