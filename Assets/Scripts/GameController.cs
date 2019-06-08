using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{   
    [SerializeField] private BubbleGrid grid = null;
    [SerializeField] private BubbleGun gun = null;

    [Header("Config")]
    [SerializeField] private AnimationCfg animationCfg;

    private void Start()
    {
        grid.Init();
        gun.Init(grid, animationCfg);
    }
}
