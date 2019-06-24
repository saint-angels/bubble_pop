using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")] 
public class BubblesConfig : ScriptableObject
{
    public Color bubbleGridColor;
    public Color bubbleGunColor;
    public Color bubbleGunAltColor;
    public Color bubbleDyingColor;

    public Color[] explosionColors;

    public int spawnPowerRange = 3;

    public uint maxCombo = 10;

    public int explosionRange = 3;
    public int explosionThresholdPower = 11;

    public Color ExplosionColorForPower(int power)
    {
        int colorIndex = power % explosionColors.Length;
        return explosionColors[colorIndex];
    }
}
