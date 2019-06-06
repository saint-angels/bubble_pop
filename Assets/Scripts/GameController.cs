using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Transform bubbleGunPoint = null;
    [SerializeField] private Bubble bubblePrefab;

    [SerializeField] private LineRenderer trajectoryLine;


    private Bubble gunBubble;

    void Start()
    {
        SetBubbleGun();
    }


    private void SetBubbleGun()
    {
        if (gunBubble == null)
        {
            Bubble newBubble = ObjectPool.Spawn<Bubble>(bubblePrefab, bubbleGunPoint.position, Quaternion.identity);
            gunBubble = newBubble;
        }
        else
        {
            Debug.LogError("Gun bubble already set!");
        }
    }

    void Update()
    {
        bool press = Input.GetMouseButton(0);
        trajectoryLine.gameObject.SetActive(press);
        if (press)
        {
            trajectoryLine.SetPosition(0, bubbleGunPoint.position);

            //TODO: Calc full trajectory
            Vector3 mousePosition = Input.mousePosition;
            Vector3 mouseWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            trajectoryLine.SetPosition(1, mouseWorldPosition);
        }    
    }
}
