using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class BubbleGrid : MonoBehaviour
{
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private GameObject bubbleOutline;

    private const float bubbleSize = 1f;
    private const int fieldWidth = 7;
    private const int fieldHeight = 7;

    private Bubble[,] bubbles = new Bubble[fieldWidth, fieldHeight];
    private BubblesConfig bubblesConfig;

    private AnimationCfg animationCfg;

    private HashSet<Bubble> bubblesMatchedSet = new HashSet<Bubble>();
    

    public void Init(BubblesConfig bubblesConfig, AnimationCfg animationCfg)
    {
        this.bubblesConfig = bubblesConfig;
        this.animationCfg = animationCfg;

        SpawnMore();
    }

    public void AttachBubble(Bubble newBubble, int x, int y)
    {
        bubbles[x, y] = newBubble;
        newBubble.transform.position = IndecesToPosition(x, y);
        newBubble.transform.SetParent(bubbleContainer, true);
        newBubble.SetInteractible(true);

        //TODO: If attach bubble gonna be called not only from gun, don't clean hashset here
        bubblesMatchedSet.Clear();

        CheckMatches(newBubble, x, y);

        if (bubblesMatchedSet.Count > 1)
        {
            List<Bubble> bubblesMatched = new List<Bubble>(bubblesMatchedSet);
            bubblesMatched = bubblesMatched.OrderByDescending((b) => b.transform.position.y).ToList();

            Bubble targetMergeBubble = bubblesMatched[0];

            Sequence mergeSequence = DOTween.Sequence();
            //Merge bubbles & destroy all except first
            for (int bIndex = bubblesMatched.Count - 1; bIndex >= 1; bIndex--)
            {
                Bubble mergingBubble = bubblesMatched[bIndex];
                Tween mergeTween = mergingBubble.transform.DOMove(targetMergeBubble.transform.position, animationCfg.bubbleMergeDuration)
                                                          .SetEase(animationCfg.bubbleMergeEase);
                mergeTween.OnComplete(() => DestroyBubble(mergingBubble));
                mergeSequence.Insert(0, mergeTween);
            }
            mergeSequence.OnComplete(() =>
            {
                BubbleType newType = bubblesConfig.GetUpgradedType(targetMergeBubble.Type, bubblesMatched.Count - 1);
                targetMergeBubble.Upgrade(newType);
            });
        }
    }

    private void DestroyBubble(Bubble bubble)
    {
        int bubbleX, bubbleY;
        if (GetBubbleIndeces(bubble, out bubbleX, out bubbleY))
        {
            bubbles[bubbleX, bubbleY] = null;
            bubble.Explode();
        }
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

            if (PointOnGrid(neighbourSlotX, neighbourSlotY))
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

    private void CheckMatches(Bubble originBubble, int x, int y)
    {
        Vector2Int[] neighbourOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int offset in neighbourOffsets)
        {
            int neighbourX = x + offset.x;
            int neighbourY = y + offset.y;
            if (PointOnGrid(neighbourX, neighbourY))
            {
                Bubble neighbourBubble = bubbles[neighbourX, neighbourY];
                if (neighbourBubble != null && neighbourBubble.Type == originBubble.Type && bubblesMatchedSet.Contains(neighbourBubble) == false)
                {
                    bubblesMatchedSet.Add(originBubble);
                    bubblesMatchedSet.Add(neighbourBubble);
                    CheckMatches(neighbourBubble, neighbourX, neighbourY);
                }
            }
        }
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

    private bool PointOnGrid(int x, int y)
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
                    BubbleType type = bubblesConfig.GetTypeForSpawn();
                    Vector3 spawnPosition = IndecesToPosition(x,y);
                    Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, spawnPosition, Quaternion.identity, bubbleContainer);
                    newBubble.Init(type, animationCfg, true);

                    Root.Instance.UI.AddHudToBubble(newBubble);

                    bubbles[x, y] = newBubble;
                }
            }
        }
    }
}
