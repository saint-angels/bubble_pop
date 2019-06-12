using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class BubbleGrid : MonoBehaviour
{
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private Transform gridOriginPoint;
    [SerializeField] private GameObject bubbleOutline;

    private const float hexSize = .35f;
    private const float bubbleSize = hexSize * 1.732f; //Make bubble fit into a hex
    private const int gridWidth = 12;
    private const int gridHeight = 14;
    private const int topUnderstroyableLinesHeight = 6; //Lines that are restored every turn
    private const float shiftDownChance = .5f;
    private float sqMaxSlotSnapDistance;

    //Shift down settings
    int shiftDownFreeLinesRequired = 3;


    //Hex grid in "doubled" coordinates
    private Bubble[,] grid = new Bubble[gridWidth, gridHeight];
    private BubblesConfig bubblesConfig;

    private AnimationCfg animationCfg;

    private HashSet<Bubble> bubblesMatchedSet = new HashSet<Bubble>();
    

    public void Init(BubblesConfig bubblesConfig, AnimationCfg animationCfg)
    {
        this.bubblesConfig = bubblesConfig;
        this.animationCfg = animationCfg;

        bubbleOutline.transform.localScale = new Vector3(bubbleSize, bubbleSize, 1);

        this.sqMaxSlotSnapDistance = Mathf.Pow(bubbleSize, 2);

        SpawnMore();
    }

    public void AttachBubble(Bubble newBubble, int x, int y)
    {
        SetBubbleGridIndeces(newBubble, x, y);
        newBubble.transform.position = IndecesToPosition(x, y);
        newBubble.SetInteractible(true);

        //TODO: If attach bubble gonna be called not only from gun(multiple matches of diff numbers in a row?), don't clean hashset here
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

                FinishTurn();
            });
        }
        else
        {
            FinishTurn();
        }
    }

    //TODO: Return promise?
    private void FinishTurn()
    {
        //Check gravity
        List<Bubble> hangingBubbles = new List<Bubble>();
        for (int x = 0; x < gridWidth; x++)
        {
            Bubble topBubble = grid[x, gridHeight - 1];
            if (topBubble != null)
            {
                hangingBubbles.Add(topBubble);
                GravityBubbleBubble(topBubble, hangingBubbles);
            }
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Bubble b = grid[x, y];
                if (b != null)
                {
                    if (hangingBubbles.Contains(b) == false)
                    {
                        DestroyBubble(b, Bubble.BubbleDeathType.FALL);
                    }

                }
            }
        }

        //Find number of free lines
        int bottomFreeLinesCount = 0;
        bool bottomBubbleFound = false;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    bottomBubbleFound = true;
                }
            }
            if (bottomBubbleFound)
            {
                break;
            }
            else
            {
                bottomFreeLinesCount++;
            }
        }
        Debug.Log($"Bottom free lines number:{bottomFreeLinesCount}");
            


        //Shift up, if player used whole height of the grid
        if (bottomFreeLinesCount == 0)
        {
            Sequence bubbleShiftUpSequence = DOTween.Sequence();
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    Bubble bubble = grid[x, y];
                    if (bubble != null)
                    {
                        bool topHiddenRows = gridHeight - 2 <= y;
                        if (topHiddenRows)
                        {
                            DestroyBubble(bubble, Bubble.BubbleDeathType.EXPLOSION);
                        }
                        else
                        {
                            SetBubbleGridIndeces(bubble, x, y + 2);
                            Vector3 newBubblePosition = IndecesToPosition(x, y + 2);
                            Tween shiftUpTween = bubble.transform.DOMove(newBubblePosition, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                            bubbleShiftUpSequence.Insert(0, shiftUpTween);
                        }
                    }
                }
            }
        }

        //Try shift down
        //TODO: The more free lines, the bigger the chance of shift. All screen lines free = 100% chance
        if (bottomFreeLinesCount >= shiftDownFreeLinesRequired)
        {
            Sequence shiftDownSequence = DOTween.Sequence();
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = bottomFreeLinesCount - 1; y < gridHeight; y++)
                {
                    Bubble bubble = grid[x, y];
                    if (bubble != null)
                    {
                        SetBubbleGridIndeces(bubble, x, y - 2);
                        Vector3 newBubblePosition = IndecesToPosition(x, y - 2);
                        Tween shiftDownTween = bubble.transform.DOMove(newBubblePosition, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                        shiftDownSequence.Insert(0, shiftDownTween);
                    }
                }
            }
        }


        //Add top bubbles, if needed
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = gridHeight - topUnderstroyableLinesHeight; y < gridHeight; y++)
            {
                if (grid[x, y] == null && PointOnHexGrid(x, y))
                {
                    Bubble newBubble = CreateNewBubble(true);
                    newBubble.transform.position = IndecesToPosition(x, y);
                    SetBubbleGridIndeces(newBubble, x, y);
                }
            }
        }
    }

    private void SetBubbleGridIndeces(Bubble bubble, int x, int y)
    {
        if (grid[x, y] != null)
        {
            Debug.LogWarning($"Overwriting bubble at grid {x}:{y} with bubble at {bubble.Indeces.x}:{bubble.Indeces.y}");
        }

        if (bubble.Indeces != Vector2Int.zero && grid[bubble.Indeces.x, bubble.Indeces.y] != bubble)
        {
            Debug.LogWarning($"Moving bubble from position {bubble.Indeces.x}:{bubble.Indeces.y} where bubble is not that? Eh?");
        }

        grid[bubble.Indeces.x, bubble.Indeces.y] = null;

        grid[x, y] = bubble;
        bubble.Indeces = new Vector2Int(x, y);
    }

    private void GravityBubbleBubble(Bubble bubble, List<Bubble> hangingBubbles)
    {
        List<Bubble> neighbours = NeighbourBubbles(bubble);
        foreach (Bubble neighbourBubble in neighbours)
        {
            if (hangingBubbles.Contains(neighbourBubble) == false)
            {
                hangingBubbles.Add(neighbourBubble);
                GravityBubbleBubble(neighbourBubble, hangingBubbles);
            }
        }
    }

    private void DestroyBubble(Bubble bubble, Bubble.BubbleDeathType deathType)
    {
        grid[bubble.Indeces.x, bubble.Indeces.y] = null;
        bubble.Die(deathType);
    }

    public void SetBubbleOutlineActive(bool isActive, Vector2Int? coordinates = null)
    {
        bubbleOutline.SetActive(isActive);
        if (coordinates.HasValue)
        {
            bubbleOutline.transform.position = IndecesToPosition(coordinates.Value.x, coordinates.Value.y);
        }
    }

    public Vector2Int? CanAttachBubbleTo(Bubble bubble, Vector3 contactPoint)
    {
        List<Vector2Int> attachSlotPositions = bubble.GetNeighbourSlotIndeces()
                                                        .Where(attachPoint => PointOnHexGrid(attachPoint.x, attachPoint.y) && grid[attachPoint.x, attachPoint.y] == null)
                                                        .OrderBy(attachPoint => Vector3.SqrMagnitude(IndecesToPosition(attachPoint.x, attachPoint.y) - contactPoint))
                                                        .ToList();

        if (attachSlotPositions.Count > 0)
        {
            Vector2Int bestSlotPoint = attachSlotPositions[0];
            float sqDistanceToSlot = Vector3.SqrMagnitude(IndecesToPosition(bestSlotPoint.x, bestSlotPoint.y) - contactPoint);
            if (sqDistanceToSlot < sqMaxSlotSnapDistance)
            {
                SetBubbleOutlineActive(true, bestSlotPoint);
                return bestSlotPoint;
            }
        }

        //Out of grid, or all slots filled, or best slot is too far away
        return null;
    }

    public Vector3 IndecesToPosition(int x, int y)
    {
        float positionX = hexSize * Mathf.Sqrt(3f)/2f * x;
        float positionY = hexSize * 3f/2f * y;

        positionX += gridOriginPoint.position.x;
        positionY += gridOriginPoint.position.y;
        return new Vector3(positionX, positionY, 0);
    }

    public Bubble CreateNewBubble(bool interactive)
    {
        BubbleType type = bubblesConfig.GetTypeForSpawn();
        Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, Vector3.zero, Quaternion.identity);
        newBubble.Indeces = Vector2Int.zero;
        newBubble.transform.localScale = new Vector3(bubbleSize, bubbleSize, 1f);
        newBubble.Init(type, animationCfg, interactive);
        Root.Instance.UI.AddHudToBubble(newBubble);
        return newBubble;
    }

    private List<Bubble> NeighbourBubbles(Bubble originBubble)
    {
        List<Bubble> neighbours = new List<Bubble>();

        foreach (Vector2Int offset in originBubble.GetNeighbourSlotIndeces())
        {
            if (PointOnHexGrid(offset.x, offset.y))
            {
                Bubble neighbourBubble = grid[offset.x, offset.y];
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

        foreach (var neighbourBubble in neighbours)
        {

            if (neighbourBubble.Type == originBubble.Type && bubblesMatchedSet.Contains(neighbourBubble) == false)
            {
                bubblesMatchedSet.Add(originBubble);
                bubblesMatchedSet.Add(neighbourBubble);
                CheckMatches(neighbourBubble, neighbourBubble.Indeces.x, neighbourBubble.Indeces.y);
            }
        }
    }

    private bool PointOnHexGrid(int x, int y)
    {
        bool doubledHexGridCheck = (x + y) % 2 == 0;
        return doubledHexGridCheck && x < gridWidth && 0 <= x && y < gridHeight && 0 <= y;
    }

    private void SpawnMore()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 3; y < gridHeight; y++)
            {
                if (PointOnHexGrid(x,y))
                {
                    Bubble newBubble = CreateNewBubble(true);
                    newBubble.transform.position = IndecesToPosition(x, y);
                    SetBubbleGridIndeces(newBubble, x, y);
                }
            }
        }

        //TODO: Spawn only bubbles that can hang
        //GravityCheck();
    }
}
