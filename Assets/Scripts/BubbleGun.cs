using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGun : MonoBehaviour
{
    [SerializeField] private Transform bubbleGunPoint = null;
    [SerializeField] private Bubble bubblePrefab = null;
    [SerializeField] private LineRenderer trajectoryLine = null;

    private Bubble gunBubble;
    private List<Vector3> trajectoryPositionsCurrent = new List<Vector3>();
    private Vector2Int? targetSlot = null;
    private BubbleField bubbleField;

    public void Init(BubbleField bubbleField)
    {
        this.bubbleField = bubbleField;
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
            trajectoryPositionsCurrent.Clear();
            trajectoryPositionsCurrent.Add(bubbleGunPoint.position);

            Vector3 mousePosition = Input.mousePosition;

            float distanceFromCamera = Vector3.Distance(bubbleGunPoint.position, Camera.main.transform.position);
            Vector3 mouseWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, distanceFromCamera));

            Vector2 castDirection = (mouseWorldPosition - bubbleGunPoint.position).normalized;

            RaycastHit2D hit = Hit(bubbleGunPoint.position, castDirection, true);
            if (hit.collider != null)
            {
                trajectoryPositionsCurrent.Add(hit.point);

                int hitDirectObjectLayer = hit.collider.gameObject.layer;
                if (hitDirectObjectLayer == LayerMask.NameToLayer("Walls"))
                {
                    Vector3 reflectedDirection = Vector3.Reflect(castDirection, Vector3.right);
                    RaycastHit2D bounceHit = Hit(hit.point, reflectedDirection, false);
                    if (bounceHit.collider != null)
                    {
                        int bounceHitObjectLayer = bounceHit.collider.gameObject.layer;
                        trajectoryPositionsCurrent.Add(bounceHit.point);
                        if (bounceHitObjectLayer == LayerMask.NameToLayer("Bubbles"))
                        {
                            HandleBubbleHit(bounceHit);
                        }
                        else
                        {
                            bubbleField.HideBubbleOutline();
                        }
                    }
                    else
                    {
                        bubbleField.HideBubbleOutline();
                        trajectoryPositionsCurrent.Add(new Vector3(hit.point.x, hit.point.y, 0) + reflectedDirection);
                    }
                }
                else if (hitDirectObjectLayer == LayerMask.NameToLayer("Bubbles"))
                {
                    HandleBubbleHit(hit);
                }
                else
                {
                    Debug.LogError($"Hit unknown object with layer {hitDirectObjectLayer}");
                }
            }
            else
            {
                bubbleField.HideBubbleOutline();
            }

            trajectoryLine.positionCount = trajectoryPositionsCurrent.Count;
            trajectoryLine.SetPositions(trajectoryPositionsCurrent.ToArray());

        }
        else
        {
            //Can we not call it every frame?
            bubbleField.HideBubbleOutline();

            if (targetSlot.HasValue)
            {

            }
        }
    }

    private void HandleBubbleHit(RaycastHit2D hit)
    {
        Bubble bubble = hit.collider.GetComponent<Bubble>();
        if (bubble != null)
        {
            targetSlot = bubbleField.CanAttachBubbleTo(bubble, hit.point);
        }
        else
        {
            Debug.LogError($"Object {hit.collider.gameObject.name} on bubbles layer is not a Bubble!");
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
}
