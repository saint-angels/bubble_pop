using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")]
public class BubblesConfig : ScriptableObject
{
    [UnityEngine.Serialization.FormerlySerializedAs("bubbleColorMono")]
    public Color bubbleGridColor;
    public Color bubbleGunColor;
    public Color bubbleDyingColor;

    public int spawnPowerRange = 3;

    public uint maxCombo = 10;

    public int explosionRange = 3;
    public int explosionThresholdPower = 11;

    public int GetPowerForGrid(ulong currentScore, int minBubblePower)
    {
        int scoreSuggestedPower = currentScore == 0 ? 1 : (int)Mathf.Log(Mathf.Sqrt(currentScore), 2);
        int minPossiblePower = Mathf.Min(scoreSuggestedPower, minBubblePower) + 1;
        return Random.Range(minPossiblePower, minPossiblePower + spawnPowerRange);
    }
}
