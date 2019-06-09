using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")]
public class BubblesConfig : ScriptableObject
{
    //Add Probabilities here?
    public BubbleType[] types = null;

    public BubbleType GetTypeForSpawn()
    {
        return types[Random.Range(0, 4)]; ;
    }

    public BubbleType GetUpgradedType(BubbleType bubbleType, int upgradeLevel)
    {
        int indexOfType = System.Array.IndexOf(types, bubbleType);
        int upgradedTypeIndex = Mathf.Min(indexOfType + upgradeLevel, types.Length - 1);
        return types[upgradedTypeIndex];
    }
}
