using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;

public class BubbleGrid : MonoBehaviour
{
    public event Action<uint> OnBubblesMerged = (afterMergeNumber) => { };
    public event Action OnNothingMergedTurn = () => { };


    //Move to settings?
    public float BubbleSize => bubbleSize;
    public float AltBubbleSize => BubbleSize / 1.5f;


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
    int freeLinesRequiredForShiftDown = 3;


    //Hex grid in "doubled" coordinates
    private Bubble[,] grid = new Bubble[gridWidth, gridHeight];
    private BubblesConfig bubblesConfig;

    private AnimationCfg animationCfg;

    private HashSet<Bubble> bubblesActionSet = new HashSet<Bubble>();
    

    public void Init()
    {
        this.bubblesConfig = Root.Instance.ConfigManager.Bubbles;
        this.animationCfg = Root.Instance.ConfigManager.Animation;

        bubbleOutline.transform.localScale = new Vector3(bubbleSize, bubbleSize, 1);

        this.sqMaxSlotSnapDistance = Mathf.Pow(bubbleSize, 2);

        //SpawnMore();
        FinishTurn();
    }

    public void AttachBubble(Bubble newBubble, int x, int y, bool shotFromGun)
    {
        SetBubbleGridIndeces(newBubble, x, y);
        newBubble.transform.position = IndecesToPosition(x, y);
        newBubble.SetInteractible(true);

        bubblesActionSet.Clear();

        CheckMatches(newBubble);

        if (bubblesActionSet.Count > 1)
        {
            List<Bubble> bubblesMatched = new List<Bubble>(bubblesActionSet);
            uint afterMergeNumber = bubblesConfig.GetUpgradedType(bubblesMatched[0].Number, bubblesMatched.Count - 1);
            bubblesMatched = bubblesMatched
                                .OrderByDescending((b) => DoesBubbleHaveNeighbourWithNumber(b, afterMergeNumber))
                                .ThenByDescending(b => b.transform.position.y)
                                .ToList();


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
                OnBubblesMerged(afterMergeNumber);
                targetMergeBubble.Upgrade(afterMergeNumber);

                if (targetMergeBubble.Number >= bubblesConfig.explosionThreshold)
                {
                    Explode(targetMergeBubble, bubblesConfig.explosionRange);
                }
                else
                {
                    AttachBubble(targetMergeBubble, targetMergeBubble.X, targetMergeBubble.Y, false);
                }

            });
        }
        else
        {
            if (shotFromGun)
            {
                OnNothingMergedTurn();
            }
            FinishTurn();
        }
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

    public Bubble CreateNewBubble(bool interactive, bool altBubbleSize = false)
    {
        uint minBubbleNumber = uint.MaxValue;
        IterateOverGrid((x, y, bubble) =>
        {
            if (bubble != null && bubble.Number < minBubbleNumber)
            {
                minBubbleNumber = bubble.Number;
            }
        });


        List<uint> bottomTwoRowsBubbles = new List<uint>();
        for (int x = 0; x < gridWidth; x++)
        {
            int columnBubblesReviewed = 0;
            for (int y = 0; y < gridHeight && columnBubblesReviewed < 2; y++)
            {
                if (PointOnHexGrid(x, y))
                {
                    if (grid[x, y] != null)
                    {
                        bottomTwoRowsBubbles.Add(grid[x, y].Number);
                        columnBubblesReviewed++;
                    }
                }
            }
        }

        int randomBottomBubbleIndex = UnityEngine.Random.Range(0, bottomTwoRowsBubbles.Count);
        uint randomBottomBubbleNumber = bottomTwoRowsBubbles.Count == 0 ? 0 : bottomTwoRowsBubbles[randomBottomBubbleIndex];

        uint bubbleNumber = bubblesConfig.GetNumberToSpawn(Root.Instance.GameController.Score, minBubbleNumber, randomBottomBubbleNumber);
        Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, Vector3.zero, Quaternion.identity);
        newBubble.SetGridPosition(0, 0);
        float size = altBubbleSize ? AltBubbleSize : bubbleSize;
        newBubble.transform.localScale = Vector3.one * size;
        newBubble.Init(bubbleNumber, interactive);
        Root.Instance.UI.AddHudToBubble(newBubble);
        return newBubble;
    }

    private int Distance(Bubble b1, Bubble b2)
    {
        int dx = Mathf.Abs(b1.X - b2.X);
        int dy = Mathf.Abs(b1.Y - b2.Y);
        return dy + Mathf.Max(0, (dx - dy) / 2);
    }

    private void Explode(Bubble originBubble, int maxDistance)
    {
        List<Bubble> bubblesToExplode = new List<Bubble>();

        IterateOverGrid((x, y, bubble) =>
        {
            //Can optimize that
            if (bubble != null && Distance(originBubble, bubble) <= maxDistance)
            {
                bubblesToExplode.Add(bubble);
            }
        });

        for (int i = bubblesToExplode.Count - 1; i >= 0; i--)
        {
            DestroyBubble(bubblesToExplode[i], Bubble.BubbleDeathType.EXPLOSION);
        }
    }

    private bool DoesBubbleHaveNeighbourWithNumber(Bubble bubble, uint number)
    {
        foreach (var neighbourIndeces in bubble.GetNeighbourSlotIndeces())
        {
            if (PointOnHexGrid(neighbourIndeces.x, neighbourIndeces.y))
            {
                Bubble neighbourBubble = grid[neighbourIndeces.x, neighbourIndeces.y];
                if (bubble != null && bubble.Number == number)
                {
                    return true;
                }
            }
        }
        return false;
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

        //Shift up, if whole height of the grid is used
        if (GetBottomFreeLines() == 0)
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
                            DestroyBubble(bubble, Bubble.BubbleDeathType.SILENT);
                        }
                        else
                        {
                            SetBubbleGridIndeces(bubble, x, y + 2);
                            Vector3 newBubblePosition = IndecesToPosition(x, y + 2);
                            bubble.transform.DOKill();
                            Tween shiftUpTween = bubble.transform.DOMove(newBubblePosition, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                            bubbleShiftUpSequence.Insert(0, shiftUpTween);
                        }
                    }
                }
            }
        }

        FillTopGridSpace();

        //Try shift down
        //TODO: The more free lines, the bigger the chance of shift. All screen lines free = 100% chance?
        int bottomFreeLinesCount = GetBottomFreeLines();
        if (bottomFreeLinesCount >= freeLinesRequiredForShiftDown)
        {
            Sequence shiftDownSequence = DOTween.Sequence();
            int shiftOffsetY = 2 * ((bottomFreeLinesCount - 1) / 2);
            IterateOverGrid((x, y, bubble) =>
            {
                if (bubble != null)
                {
                    bubble.transform.DOKill();
                    SetBubbleGridIndeces(bubble, x, y - shiftOffsetY);
                    Vector3 newBubblePosition = IndecesToPosition(x, y - shiftOffsetY);
                    Tween shiftDownTween = bubble.transform.DOMove(newBubblePosition, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                    shiftDownSequence.Insert(0, shiftDownTween);
                }
            });
        }


        //Add top bubbles, if needed
        FillTopGridSpace();
    }

    private void FillTopGridSpace()
    {
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

    private int GetBottomFreeLines()
    {
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
        //Debug.Log($"Bottom free lines number:{bottomFreeLinesCount}");
        return bottomFreeLinesCount;
    }

    private void SetBubbleGridIndeces(Bubble bubble, int x, int y)
    {
        if (grid[x, y] != null && bubble.X != x && bubble.Y != y)
        {
            Debug.LogWarning($"Overwriting bubble at grid {x}:{y} with bubble at {bubble.X}:{bubble.Y}");
        }

        if (bubble.X != 0 && bubble.Y != 0 && grid[bubble.X, bubble.Y] != bubble)
        {
            Debug.LogWarning($"Moving bubble from position {bubble.X}:{bubble.Y} where bubble is not that? Eh?");
        }

        grid[bubble.X, bubble.Y] = null;

        grid[x, y] = bubble;
        bubble.SetGridPosition(x, y);
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
        grid[bubble.X, bubble.Y] = null;
        bubble.Die(deathType);
    }

    private void IterateOverGrid(Action<int,int, Bubble> action)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                action(x, y, grid[x,y]);
            }
        }
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

    private void CheckMatches(Bubble originBubble)
    {
        List<Bubble> neighbours = NeighbourBubbles(originBubble);

        foreach (var neighbourBubble in neighbours)
        {

            if (neighbourBubble.Number == originBubble.Number && bubblesActionSet.Contains(neighbourBubble) == false)
            {
                bubblesActionSet.Add(originBubble);
                bubblesActionSet.Add(neighbourBubble);
                CheckMatches(neighbourBubble);
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
    }
}
