using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCreator : MonoBehaviour
{
    [SerializeField] private Bubble bubblePrefab = null;

    private BubblesConfig bubblesConfig;
    private GridConfig gridConfig;

    public void Init()
    {
        this.bubblesConfig = Root.Instance.ConfigManager.Bubbles;
        this.gridConfig = Root.Instance.ConfigManager.Grid;
    }

    public Bubble GetBubble(Bubble.BubbleState bubbleState)
    {
        int minBubblePower = Root.Instance.Grid.GetMinBubblePower();

        int bubblePower = 0;
        if (bubbleState == Bubble.BubbleState.GRID)
        {
            ulong currentScore = Root.Instance.GameController.Score;

            int scoreSuggestedPower = currentScore == 0 ? 1 : (int)Mathf.Log(Mathf.Sqrt(currentScore), 2);
            int minPossiblePower = Mathf.Min(scoreSuggestedPower, minBubblePower) + 1;
            bubblePower = Random.Range(minPossiblePower, minPossiblePower + bubblesConfig.spawnPowerRange);
        }
        else
        {
            bubblePower = Root.Instance.Grid.GetRandomBottomBubblePower();

        }
        Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, Vector3.zero, Quaternion.identity);
        newBubble.SetGridPosition(0, 0);
        newBubble.transform.localScale = Vector3.one * gridConfig.BubbleSize;
        newBubble.Init(bubblePower, bubbleState);
        Root.Instance.UI.AddHudToBubble(newBubble);
        return newBubble;
    }
}
