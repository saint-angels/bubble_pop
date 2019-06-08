using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGrid : MonoBehaviour
{
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private GameObject bubbleOutline;

    private const float bubbleSize = 1f;
    private const int fieldWidth = 7;
    private const int fieldHeight = 7;

    private Bubble[,] bubbles = new Bubble[fieldWidth, fieldHeight];
    private BubbleType[] bubbleTypes;

    public void Init(BubbleType[] bubbleTypes)
    {
        this.bubbleTypes = bubbleTypes;
        SpawnMore();
    }

    public void AddBubble(Bubble newBubble, int x, int y)
    {
        bubbles[x, y] = newBubble;
        newBubble.transform.position = IndecesToPosition(x, y);
        newBubble.transform.SetParent(bubbleContainer, true);
        newBubble.SetInteractible(true);
    }

    public void HideBubbleOutline()
    {
        bubbleOutline.SetActive(false);
    }

    public Vector2Int? CanAttachBubbleTo(Bubble bubble, Vector3 contactPoint)
    {
        Bubble.BubbleSide bubbleSide = bubble.ClosestSideToPoint(contactPoint);

        int bubbleX, bubbleY;
        if (GetBubbleIndeces(bubble, out bubbleX, out bubbleY))
        {
            int neighbourSlotX = bubbleX;
            int neighbourSlotY = bubbleY;
            switch (bubbleSide)
            {
                case Bubble.BubbleSide.LEFT:
                    neighbourSlotX--;
                    break;
                case Bubble.BubbleSide.RIGHT:
                    neighbourSlotX++;
                    break;
                case Bubble.BubbleSide.BOTTOM:
                    neighbourSlotY--;
                    break;
            }

            if (PointInField(neighbourSlotX, neighbourSlotY))
            {
                Bubble neighbourBubble = bubbles[neighbourSlotX, neighbourSlotY];
                if (neighbourBubble == null)
                {
                    bubbleOutline.transform.position = IndecesToPosition(neighbourSlotX, neighbourSlotY);
                    bubbleOutline.SetActive(true);
                    return new Vector2Int(neighbourSlotX, neighbourSlotY);
                }
            }
            //Out of field, or slot is alredy occupied
            return null;
        }
        else
        {
            Debug.LogError($"Can't find the bubble {bubble.gameObject.name} in the field!");
            return null;
        }
    }

    public Vector3 IndecesToPosition(int x, int y)
    {
        float positionX = bubbleContainer.position.x + (bubbleSize / 2f) * x;
        float positionY = bubbleContainer.position.y + (bubbleSize / 2f) * y;
        return new Vector3(positionX, positionY, 0);
    }

    private bool GetBubbleIndeces(Bubble bubble, out int bubbleX, out int bubbleY)
    {
        for (int x = 0; x < fieldWidth; x++)
        {
            for (int y = 0; y < fieldHeight; y++)
            {
                if (bubbles[x,y] == bubble)
                {
                    bubbleX = x;
                    bubbleY = y;
                    return true;
                }
            }
        }

        Debug.LogError($"Can't find the bubble {bubble.gameObject.name} in the field");
        bubbleX = 0;
        bubbleY = 0;
        return false;
    }

    private bool PointInField(int x, int y)
    {
        return x < fieldWidth && 0 <= x && y < fieldHeight && 0 <= y;
    }

    private void SpawnMore()
    {
        for (int x = 0; x < fieldWidth - 1; x++)
        {
            for (int y = 1; y < fieldHeight; y++)
            {
                if (Random.value > .3f)
                {
                    BubbleType randomType = bubbleTypes[Random.Range(0, bubbleTypes.Length)];
                    Vector3 spawnPosition = IndecesToPosition(x,y);
                    Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, spawnPosition, Quaternion.identity, bubbleContainer);
                    newBubble.Init(randomType);
                    newBubble.SetInteractible(true);

                    bubbles[x, y] = newBubble;
                }
            }
        }
    }
}
