using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")]
public class BubblesConfig : ScriptableObject
{
    public Color[] bubbleColors = null;
    public int rangeOfPowers = 4;


    public int explosionRange = 3;
    public uint explosionThreshold = 2048;

    public Color ColorForNumber(uint number)
    {
        int powerOfTwo = (int)Mathf.Log(number, 2);
        int colorIndex = powerOfTwo % bubbleColors.Length;
        return bubbleColors[colorIndex];
    }

    public uint GetNumberToSpawn(uint currentScore, uint minBubbleNumber)
    {
        int scoreSuggestedPower = currentScore == 0 ? 1 : (int)Mathf.Log(Mathf.Sqrt(currentScore), 2);
        int bubblesSuggestedPower = (int)Mathf.Log(minBubbleNumber, 2);

        int minPossiblePower = Mathf.Min(scoreSuggestedPower, bubblesSuggestedPower) + 1;
        return (uint)Mathf.Pow(2, Random.Range(minPossiblePower, minPossiblePower + rangeOfPowers));
    }

    public uint GetUpgradedType(uint bubbleNumber, int upgradeLevel)
    {
        int powerOfTwoOriginal = (int)System.Math.Log(bubbleNumber, 2);
        return (uint)Mathf.Pow(2, powerOfTwoOriginal + upgradeLevel);
    }
}
