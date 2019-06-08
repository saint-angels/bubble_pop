using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : SingletonComponent<Root>
{
    public UIManager UI { get {return uiManager; } }

    [SerializeField] private UIManager uiManager = null;


}
