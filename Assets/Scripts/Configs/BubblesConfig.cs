using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")]
public class BubblesConfig : ScriptableObject
{
    public Color[] bubbleColors = null;
    [UnityEngine.Serialization.FormerlySerializedAs("bubbleColorMono")]
    public Color bubbleGridColor;
    public Color bubbleGunColor;

    public int spawnPowerRange = 3;

    public uint maxCombo = 10;

    public int explosionRange = 3;
    public int explosionThresholdPower = 11;

    public Color ColorForPower(int power)
    {
        int colorIndex = power % bubbleColors.Length;
        return bubbleColors[colorIndex];
    }

    public int GetPowerToSpawn(ulong currentScore, int minBubblePower, int randomBottomBubblePower)
    {
        int scoreSuggestedPower = currentScore == 0 ? 1 : (int)Mathf.Log(Mathf.Sqrt(currentScore), 2);
        int minPossiblePower = Mathf.Min(scoreSuggestedPower, minBubblePower) + 1;
        return Random.Range(minPossiblePower, minPossiblePower + spawnPowerRange);

        //More complicated version that doesn't work well
        //bool gridEmpty = randomBottomBubblePower == 0;
        //if (gridEmpty)
        {
            //This version uses current score & min grid bubble number
            //int scoreSuggestedPower = currentScore == 0 ? 1 : (int)Mathf.Log(Mathf.Sqrt(currentScore), 2);

            //int minPossiblePower = Mathf.Min(scoreSuggestedPower, minBubblePower) + 1;
            //return Random.Range(minPossiblePower, minPossiblePower + spawnPowerRange);
        }
        //else
        //{
        //    //Stupid and predictable
        //    return randomBottomBubbleNumber;
        //}
    }
}
