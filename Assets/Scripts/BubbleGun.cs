using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGun : MonoBehaviour
{
    [SerializeField] private Transform bubbleGunPoint = null;
    [SerializeField] private Bubble bubblePrefab = null;
    [SerializeField] private LineRenderer trajectoryLine = null;

    private Bubble currentGunBubble;
    private List<Vector3> trajectoryPositionsCurrent = new List<Vector3>();
    private Vector2Int? targetSlot = null;
    private BubbleGrid grid;
    private AnimationCfg animationCfg;

    private BubblesConfig bubblesConfig;
    public void Init(BubbleGrid grid, BubblesConfig bubblesConfig, AnimationCfg animationCfg)
    {
        this.grid = grid;
        this.bubblesConfig = bubblesConfig;
        this.animationCfg = animationCfg;
        LoadGun();
    }

    private void LoadGun()
    {
        if (currentGunBubble == null)
        {
            Bubble newBubble = grid.CreateNewBubble(false);
            newBubble.transform.position = bubbleGunPoint.position;
            currentGunBubble = newBubble;
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
        if (press && currentGunBubble != null)
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
                            grid.SetBubbleOutlineActive(false);
                        }
                    }
                    else
                    {
                        grid.SetBubbleOutlineActive(false);
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
                grid.SetBubbleOutlineActive(false);
            }

            trajectoryLine.positionCount = trajectoryPositionsCurrent.Count;
            trajectoryLine.SetPositions(trajectoryPositionsCurrent.ToArray());

        }
        else
        {
            //Can we not call it every frame?
            grid.SetBubbleOutlineActive(false);

            bool canShoot = targetSlot.HasValue;
            if (canShoot)
            {
                Vector3 slotPosition = grid.IndecesToPosition(targetSlot.Value.x, targetSlot.Value.y);
                var moveTween = currentGunBubble.transform.DOMove(slotPosition, animationCfg.shootBubbleFlyDuration);
                Bubble flyingBubble = currentGunBubble;
                Vector2Int gunTargetSlot = targetSlot.Value;
                moveTween.OnComplete(() => 
                {
                    grid.AttachBubble(flyingBubble, gunTargetSlot.x, gunTargetSlot.y);
                    LoadGun();
                });

                targetSlot = null;
                currentGunBubble = null;
            }
        }
    }

    private void HandleBubbleHit(RaycastHit2D hit)
    {
        Bubble bubble = hit.collider.GetComponent<Bubble>();
        if (bubble != null)
        {
            targetSlot = grid.CanAttachBubbleTo(bubble, hit.point);
            if (targetSlot == null)
            {
                grid.SetBubbleOutlineActive(false);
            }
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
