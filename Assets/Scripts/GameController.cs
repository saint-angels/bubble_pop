using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{   
    [SerializeField] private BubbleGrid grid = null;
    [SerializeField] private BubbleGun gun = null;

    [Header("Configs")]
    [SerializeField] private AnimationCfg animationCfg;
    [SerializeField] private BubblesConfig bubblesConfig;

    private void Start()
    {
        grid.Init(bubblesConfig, animationCfg);
        gun.Init(grid, bubblesConfig, animationCfg);
    }
}
