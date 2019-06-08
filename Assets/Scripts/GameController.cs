using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{   
    [SerializeField] private BubbleField field = null;
    [SerializeField] private BubbleGun gun = null;

    private void Start()
    {
        field.Init();
        gun.Init(field);
    }
}
