﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : SingletonComponent<Root>
{
    public GameController GameController => gameController;
    public UIManager UI => uiManager;
    public GridManager Grid => grid;
    public BubbleGun Gun => gun;
    public ConfigManager ConfigManager => configManager;
    public CameraController CameraController => cameraController;
    public BubbleCreator BubbleCreator => bubbleCreator;
    public AudioManager AudioManager => audioManager;

    [SerializeField] private GameController gameController = null;
    [SerializeField] private UIManager uiManager = null;
    [SerializeField] private GridManager grid = null;
    [SerializeField] private BubbleGun gun = null;
    [SerializeField] private ConfigManager configManager = null;
    [SerializeField] private CameraController cameraController = null;
    [SerializeField] private BubbleCreator bubbleCreator = null;
    [SerializeField] private AudioManager audioManager = null;


}
