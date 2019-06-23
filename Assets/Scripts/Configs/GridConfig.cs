using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridConfig", menuName = "Configs/GridConfig")]
public class GridConfig : ScriptableObject
{
    public int gridWidth = 12;
    public int gridHeight = 14;

    public float hexSize = .35f;
    
    public int topUnderstroyableLinesHeight = 6; //Lines that are restored every turn
    public int maxLinesFittingIntoScreen = 7;
    public int freeLinesRequiredForShiftDown = 3;

    //Auto generated settings
    public float SqMaxSlotSnapDistance => Mathf.Pow(BubbleSize, 2);
    public float BubbleSize => hexSize * 1.732f; //Make bubble fit into a hex
    public float AltBubbleSize => BubbleSize / 1.5f;
}
