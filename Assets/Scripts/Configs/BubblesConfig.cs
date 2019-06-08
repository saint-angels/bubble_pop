using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubblesConfig", menuName = "Configs/BubblesConfig")]
public class BubblesConfig : ScriptableObject
{
    //Probabilities
    public BubbleType[] types = null;

    public BubbleType GetTypeForSpawn()
    {
        //return types[Random.Range(0, types.Length)]; ;
        return types[0]; ;
    }
}
