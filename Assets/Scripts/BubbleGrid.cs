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
    private const int gridWidth = 7;
    private const int gridHeight = 7;

    private Bubble[,] bubbles = new Bubble[gridWidth, gridHeight];
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
        newBubble.Position = new Vector2Int(x, y);
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
                mergeTween.OnComplete(() => DestroyBubble(mergingBubble, Bubble.BubbleDeathType.EXPLOSION));
                mergeSequence.Insert(0, mergeTween);
            }
            mergeSequence.OnComplete(() =>
            {
                BubbleType newType = bubblesConfig.GetUpgradedType(targetMergeBubble.Type, bubblesMatched.Count - 1);
                targetMergeBubble.Upgrade(newType);

                GravityCheck();
            });
        }
        else
        {
            GravityCheck();
        }
    }

    private void GravityCheck()
    {
        List<Bubble> hangingBubbles = new List<Bubble>();
        for (int x = 0; x < gridWidth; x++)
        {
            Bubble topBubble = bubbles[x, gridHeight - 1];
            if (topBubble != null)
            {
                hangingBubbles.Add(topBubble);
                GravityCheckBubble(topBubble, hangingBubbles);
            }
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Bubble b = bubbles[x, y];
                if (b != null)
                {
                    if (hangingBubbles.Contains(b) == false)
                    {
                        DestroyBubble(b, Bubble.BubbleDeathType.FALL);
                    }

                }
            }
        }
    }

    private void GravityCheckBubble(Bubble bubble, List<Bubble> hangingBubbles)
    {
        List<Bubble> neighbours = NeighbourBubbles(bubble);
        foreach (Bubble neighbourBubble in neighbours)
        {
            if (hangingBubbles.Contains(neighbourBubble) == false)
            {
                hangingBubbles.Add(neighbourBubble);
                GravityCheckBubble(neighbourBubble, hangingBubbles);
            }
        }
    }

    private void DestroyBubble(Bubble bubble, Bubble.BubbleDeathType deathType)
    {
        bubbles[bubble.Position.x, bubble.Position.y] = null;
        bubble.Die(deathType);
    }

    public void HideBubbleOutline()
    {
        bubbleOutline.SetActive(false);
    }

    public Vector2Int? CanAttachBubbleTo(Bubble bubble, Vector3 contactPoint)
    {
        Bubble.BubbleSide bubbleSide = bubble.ClosestSideToPoint(contactPoint);

        int neighbourSlotX = bubble.Position.x;
        int neighbourSlotY = bubble.Position.y;
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

    public Vector3 IndecesToPosition(int x, int y)
    {
        float positionX = bubbleContainer.position.x + (bubbleSize / 2f) * x;
        float positionY = bubbleContainer.position.y + (bubbleSize / 2f) * y;
        return new Vector3(positionX, positionY, 0);
    }

    private List<Bubble> NeighbourBubbles(Bubble originBubble)
    {
        List<Bubble> neighbours = new List<Bubble>();

        Vector2Int[] neighbourOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int offset in neighbourOffsets)
        {
            int neighbourX = originBubble.Position.x + offset.x;
            int neighbourY = originBubble.Position.y + offset.y;
            if (PointOnGrid(neighbourX, neighbourY))
            {
                Bubble neighbourBubble = bubbles[neighbourX, neighbourY];
                if (neighbourBubble != null)
                {
                    neighbours.Add(neighbourBubble);
                }
            }
        }
        return neighbours;
    }

    private void CheckMatches(Bubble originBubble, int x, int y)
    {
        List<Bubble> neighbours = NeighbourBubbles(originBubble);

        Vector2Int[] neighbourOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var neighbourBubble in neighbours)
        {

            if (neighbourBubble.Type == originBubble.Type && bubblesMatchedSet.Contains(neighbourBubble) == false)
            {
                bubblesMatchedSet.Add(originBubble);
                bubblesMatchedSet.Add(neighbourBubble);
                CheckMatches(neighbourBubble, neighbourBubble.Position.x, neighbourBubble.Position.y);
            }
        }
    }

    private bool PointOnGrid(int x, int y)
    {
        return x < gridWidth && 0 <= x && y < gridHeight && 0 <= y;
    }

    private void SpawnMore()
    {
        for (int x = 0; x < gridWidth - 1; x++)
        {
            for (int y = 1; y < gridHeight; y++)
            {
                if (Random.value > .3f)
                {
                    BubbleType type = bubblesConfig.GetTypeForSpawn();
                    Vector3 spawnPosition = IndecesToPosition(x,y);
                    Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, spawnPosition, Quaternion.identity, bubbleContainer);
                    newBubble.Init(type, animationCfg, true);
                    newBubble.Position = new Vector2Int(x, y);
                    Root.Instance.UI.AddHudToBubble(newBubble);

                    bubbles[x, y] = newBubble;
                }
            }
        }

        //TODO: Spawn only bubbles that can hang
        GravityCheck();
    }
}
