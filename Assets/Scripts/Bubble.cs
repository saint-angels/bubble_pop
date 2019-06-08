using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum BubbleSide
    {
        LEFT,
        RIGHT,
        BOTTOM
    }

    [System.Serializable]
    private struct SidePoint
    {
        public BubbleSide sideType;
        public Transform point;
    }


    [SerializeField] private new Collider2D collider;

    [SerializeField] private SidePoint[] bubbleSidePoints;

    public void SetInteractible(bool isInteractible)
    {
        collider.enabled = isInteractible;
    }
    
    public BubbleSide ClosestSideToPoint(Vector3 point)
    {
        float minSqDistance = float.MaxValue;
        BubbleSide closestSide = BubbleSide.BOTTOM;

        for (int i = 0; i < bubbleSidePoints.Length; i++)
        {
            float sqDistance = Vector3.SqrMagnitude(point - bubbleSidePoints[i].point.position);
            if (sqDistance < minSqDistance)
            {
                closestSide = bubbleSidePoints[i].sideType;
                minSqDistance = sqDistance;
            }
        }
        return closestSide;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
