using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;

public class GridManager : MonoBehaviour
{
    public event Action<int> OnBubblesMerged = (afterMergePower) => { };
    public event Action OnNothingMergedTurn = () => { };
    public event Action OnGridCleared = () => { };

    [SerializeField] private Transform gridOriginPoint = null;
    [SerializeField] private GameObject bubbleOutline = null;

    //Hex grid in "doubled" coordinates
    private Bubble[,] grid;
    private BubblesConfig bubblesConfig;
    private AnimationCfg animationCfg;
    private GridConfig gridConfig;

    private HashSet<Bubble> bubblesActionSet = new HashSet<Bubble>();
    

    public void Init()
    {
        this.bubblesConfig = Root.Instance.ConfigManager.Bubbles;
        this.animationCfg = Root.Instance.ConfigManager.Animation;
        this.gridConfig = Root.Instance.ConfigManager.Grid;

        grid = new Bubble[gridConfig.gridWidth, gridConfig.gridHeight];
        bubbleOutline.transform.localScale = new Vector3(gridConfig.BubbleSize, gridConfig.BubbleSize, 1);

        FinishTurn();
    }

    public void AttachBubble(Bubble newBubble, int x, int y, bool shotFromGun)
    {
        SetBubbleGridIndeces(newBubble, x, y);
        newBubble.transform.position = IndecesToPosition(x, y);
        newBubble.SetState(Bubble.BubbleState.GRID);

        bubblesActionSet.Clear();

        CheckMatches(newBubble);

        if (bubblesActionSet.Count > 1)
        {
            List<Bubble> bubblesMatched = new List<Bubble>(bubblesActionSet);
            int afterMergePower = bubblesMatched[0].Power + bubblesMatched.Count - 1;
            bubblesMatched = bubblesMatched
                                .OrderByDescending((b) => DoesBubbleHaveNeighbourWithPower(b, afterMergePower))
                                .ThenByDescending(b => b.transform.position.y)
                                .ToList();


            Bubble targetMergeBubble = bubblesMatched[0];

            Sequence mergeSequence = DOTween.Sequence();
            //Merge bubbles & destroy all except first
            for (int bIndex = bubblesMatched.Count - 1; bIndex >= 1; bIndex--)
            {
                Bubble mergingBubble = bubblesMatched[bIndex];
                float mergeDuration = animationCfg.bubbleMergeDuration + .01f * bIndex;
                Tween mergeScaleTween = mergingBubble.transform.DOScale(animationCfg.bubbleMergeTargetScale, mergeDuration)
                                                          .SetEase(animationCfg.bubbleMergeEase);
                Tween mergeMoveTween = mergingBubble.transform.DOMove(targetMergeBubble.transform.position, mergeDuration)
                                                          .SetEase(animationCfg.bubbleMergeEase);
                mergeMoveTween.OnComplete(() => DestroyBubble(mergingBubble, Bubble.BubbleDeathType.EXPLOSION));
                mergeSequence.Insert(0, mergeMoveTween);
                mergeSequence.Insert(0, mergeScaleTween);
            }
            mergeSequence.OnComplete(() =>
            {
                OnBubblesMerged(afterMergePower);
                targetMergeBubble.Upgrade(afterMergePower);

                if (targetMergeBubble.Power >= bubblesConfig.explosionThresholdPower)
                {
                    Explode(targetMergeBubble);
                    FinishTurn();
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
            if (sqDistanceToSlot < gridConfig.SqMaxSlotSnapDistance)
            {
                SetBubbleOutlineActive(true, bestSlotPoint);
                return bestSlotPoint;
            }
        }

        //Out of grid, or all slots filled, or best slot is too far away
        return null;
    }

    public int GetMinBubblePower()
    {
        int minBubblePower = int.MaxValue;
        IterateOverGrid((x, y, bubble) =>
        {
            if (bubble != null && bubble.Power < minBubblePower)
            {
                minBubblePower = bubble.Power;
            }
        });
        return minBubblePower;
    }

    public int GetRandomBottomBubblePower()
    {
        List<int> bottomTwoRowsBubbles = new List<int>();
        for (int x = 0; x < gridConfig.gridWidth; x++)
        {
            int columnBubblesReviewed = 0;
            for (int y = 0; y < gridConfig.gridHeight && columnBubblesReviewed < 2; y++)
            {
                if (PointOnHexGrid(x, y))
                {
                    if (grid[x, y] != null)
                    {
                        bottomTwoRowsBubbles.Add(grid[x, y].Power);
                        columnBubblesReviewed++;
                    }
                }
            }
        }

        int randomBottomBubbleIndex = UnityEngine.Random.Range(0, bottomTwoRowsBubbles.Count);
        int randomBottomBubblePower = bottomTwoRowsBubbles.Count == 0 ? 0 : bottomTwoRowsBubbles[randomBottomBubbleIndex];
        return randomBottomBubblePower;
    }

    public Vector3 IndecesToPosition(int x, int y)
    {
        float positionX = gridConfig.hexSize * Mathf.Sqrt(3f)/2f * x;
        float positionY = gridConfig.hexSize * 3f/2f * y;

        positionX += gridOriginPoint.position.x;
        positionY += gridOriginPoint.position.y;
        return new Vector3(positionX, positionY, 0);
    }

    private int Distance(Bubble b1, Bubble b2)
    {
        int dx = Mathf.Abs(b1.X - b2.X);
        int dy = Mathf.Abs(b1.Y - b2.Y);
        return dy + Mathf.Max(0, (dx - dy) / 2);
    }

    private void Explode(Bubble originBubble)
    {
        List<Bubble> bubblesToExplode = new List<Bubble>();

        IterateOverGrid((x, y, bubble) =>
        {
            //Can optimize that
            if (bubble != null && Distance(originBubble, bubble) <= bubblesConfig.explosionRange)
            {
                bubblesToExplode.Add(bubble);
            }
        });

        int explosionPower = int.MaxValue;
        int explodingBubbles = bubblesToExplode.Count;
        for (int i = bubblesToExplode.Count - 1; i >= 0; i--)
        {
            if (bubblesToExplode[i].Power < explosionPower)
            {
                explosionPower = bubblesToExplode[i].Power;
            }
            DestroyBubble(bubblesToExplode[i], Bubble.BubbleDeathType.EXPLOSION);
        }

        OnBubblesMerged(explosionPower * explodingBubbles);
    }

    private bool DoesBubbleHaveNeighbourWithPower(Bubble bubble, int power)
    {
        foreach (Vector2Int neighbourIndeces in bubble.GetNeighbourSlotIndeces())
        {
            if (PointOnHexGrid(neighbourIndeces.x, neighbourIndeces.y))
            {
                Bubble neighbourBubble = grid[neighbourIndeces.x, neighbourIndeces.y];
                if (bubble != null && bubble.Power == power)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void FinishTurn()
    {
        //Check gravity
        List<Bubble> hangingBubbles = new List<Bubble>();
        for (int x = 0; x < gridConfig.gridWidth; x++)
        {
            Bubble topBubble = grid[x, gridConfig.gridHeight - 1];
            if (topBubble != null)
            {
                hangingBubbles.Add(topBubble);
                CheckGravityForBubble(topBubble, hangingBubbles);
            }
        }

        for (int x = 0; x < gridConfig.gridWidth; x++)
        {
            for (int y = 0; y < gridConfig.gridHeight; y++)
            {
                Bubble b = grid[x, y];
                if (b != null)
                {
                    if (hangingBubbles.Contains(b) == false)
                    {
                        DestroyBubble(b, Bubble.BubbleDeathType.DROP);
                    }

                }
            }
        }

        //Shift up, if whole height of the grid is used
        if (GetBottomFreeLines() == 0)
        {
            Sequence bubbleShiftUpSequence = DOTween.Sequence();
            for (int x = 0; x < gridConfig.gridWidth; x++)
            {
                for (int y = gridConfig.gridHeight - 1; y >= 0; y--)
                {
                    Bubble bubble = grid[x, y];
                    if (bubble != null)
                    {
                        bool topHiddenRows = gridConfig.gridHeight - 2 <= y;
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
        int bottomFreeLinesCount = GetBottomFreeLines();

        if (bottomFreeLinesCount >= gridConfig.maxLinesFittingIntoScreen)
        {
            OnGridCleared();
        }

        if (bottomFreeLinesCount >= gridConfig.freeLinesRequiredForShiftDown)
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
        for (int x = 0; x < gridConfig.gridWidth; x++)
        {
            for (int y = gridConfig.gridHeight - gridConfig.topUnderstroyableLinesHeight; y < gridConfig.gridHeight; y++)
            {
                if (grid[x, y] == null && PointOnHexGrid(x, y))
                {
                    Bubble newBubble = Root.Instance.BubbleCreator.GetBubble(Bubble.BubbleState.GRID);
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
        for (int y = 0; y < gridConfig.gridHeight; y++)
        {
            for (int x = 0; x < gridConfig.gridWidth; x++)
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

    private void CheckGravityForBubble(Bubble bubble, List<Bubble> hangingBubbles)
    {
        List<Bubble> neighbours = NeighbourBubbles(bubble);
        foreach (Bubble neighbourBubble in neighbours)
        {
            if (hangingBubbles.Contains(neighbourBubble) == false)
            {
                hangingBubbles.Add(neighbourBubble);
                CheckGravityForBubble(neighbourBubble, hangingBubbles);
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
        for (int x = 0; x < gridConfig.gridWidth; x++)
        {
            for (int y = 0; y < gridConfig.gridHeight; y++)
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

            if (neighbourBubble.Power == originBubble.Power 
                && bubblesActionSet.Contains(neighbourBubble) == false
                && neighbourBubble.Y <= gridConfig.maxLinesFittingIntoScreen) //Don't allow to bubbles off-screen blow up
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
        return doubledHexGridCheck && x < gridConfig.gridWidth && 0 <= x && y < gridConfig.gridHeight && 0 <= y;
    }
}
