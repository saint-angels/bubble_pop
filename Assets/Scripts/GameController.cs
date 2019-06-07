﻿using System.Collections;
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
            newBubble.SetInteractible(false);
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

            float distanceFromCamera = Vector3.Distance(bubbleGunPoint.position, Camera.main.transform.position);
            Vector3 mouseWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(mousePosition.x/Screen.width, mousePosition.y/ Screen.height, distanceFromCamera));
            trajectoryLine.SetPosition(1, mouseWorldPosition);

            Vector2 castDirection = (mouseWorldPosition - bubbleGunPoint.position).normalized;

            RaycastHit2D hit = Hit(bubbleGunPoint.position, castDirection, true);
            if (hit.collider != null)
            {
                trajectoryLine.SetPosition(1, hit.point);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Walls"))
                {
                    Vector3 reflectedDirection = Vector3.Reflect(castDirection, Vector3.right);
                    RaycastHit2D bounceHit = Hit(hit.point, reflectedDirection, false);
                    if (bounceHit.collider != null)
                    {
                        trajectoryLine.SetPosition(2, bounceHit.point);
                    }
                    else
                    {
                        trajectoryLine.SetPosition(2, new Vector3(hit.point.x, hit.point.y, 0) + reflectedDirection);
                    }
                }
                else
                {

                }
            }

            
        }
    }


    private RaycastHit2D Hit(Vector3 start, Vector3 direction, bool withWalls)
    {
        LayerMask mask = 1 << LayerMask.NameToLayer("Bubbles");
        if (withWalls)
        {
            mask |= 1 << LayerMask.NameToLayer("Walls");
        }
        return Physics2D.Raycast(start, direction, 10f, mask);
    }

    void OnDrawGizmos()
    {
        if (Input.GetMouseButton(0))
        { 
            Vector3 p = Camera.main.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(p, 1F);
        }
    }
}
