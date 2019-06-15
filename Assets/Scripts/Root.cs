using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : SingletonComponent<Root>
{
    public GameController GameController => gameController;
    public UIManager UI => uiManager;
    public BubbleGrid Grid => grid;
    public BubbleGun Gun => gun;
    public ConfigManager ConfigManager => configManager;

    [SerializeField] private GameController gameController = null;
    [SerializeField] private UIManager uiManager = null;
    [SerializeField] private BubbleGrid grid = null;
    [SerializeField] private BubbleGun gun = null;
    [SerializeField] private ConfigManager configManager = null;
}
