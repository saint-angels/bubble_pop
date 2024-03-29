﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGun : MonoBehaviour
{
    private Vector3 AltBubblePoint => muzzlePoint.position + altBubbleOffset;

    [SerializeField] private Transform muzzlePoint = null;
    [SerializeField] private LineRenderer trajectoryLine = null;

    private Bubble currentBubble = null;
    private Bubble alternativeBubble = null;

    private List<Vector3> trajectoryPositionsCurrent = new List<Vector3>();
    private Vector2Int? targetSlot = null;
    private GridManager grid;
    private AnimationCfg animationCfg;
    private BubblesConfig bubblesConfig;
    private GridConfig gridConfig;
    private Vector3 altBubbleOffset;
    float distanceFromCamera;

    //Layers
    private int layerWalls;
    private int layerBubbles;

    private const float aimingRestrictedScreenFraction = .2f;

    private bool paused = false;

    public void Init()
    {
        this.grid = Root.Instance.Grid;
        this.bubblesConfig = Root.Instance.ConfigManager.Bubbles;
        this.animationCfg = Root.Instance.ConfigManager.Animation;
        this.gridConfig = Root.Instance.ConfigManager.Grid;

        altBubbleOffset = Vector3.left * gridConfig.BubbleSize * 1.25f;
        distanceFromCamera = Vector3.Distance(muzzlePoint.position, Camera.main.transform.position);

        layerWalls = LayerMask.NameToLayer("Walls");
        layerBubbles = LayerMask.NameToLayer("Bubbles");

        Root.Instance.UI.OnGamePauseShown += UI_OnGamePauseShown; ;

        LoadGun();
    }

    private void UI_OnGamePauseShown(bool isPauseShown)
    {
        paused = isPauseShown;
    }

    //Shouldn't take control away from the player 
    //TODO: Add spawn animation & animation interruptions
    private void LoadGun()
    {
        if (currentBubble == null)
        {
            Sequence seq = DOTween.Sequence();
            if (alternativeBubble == null)
            {
                Bubble newBubble = Root.Instance.BubbleCreator.GetBubble(Bubble.BubbleState.GUN);
                currentBubble = newBubble;
                currentBubble.transform.position = muzzlePoint.position;
            }
            else
            {
                currentBubble = alternativeBubble;
                alternativeBubble = null;

                MoveBubbleFromAltPositionToCurrent();
            }

            Bubble newAltBubble = Root.Instance.BubbleCreator.GetBubble(Bubble.BubbleState.GUN_ALT);
            newAltBubble.transform.localScale = Vector3.one * gridConfig.AltBubbleSize;
            newAltBubble.transform.position = AltBubblePoint;
            alternativeBubble = newAltBubble;
        }
        else
        {
            Debug.LogError("Gun bubble already loaded!");
        }
    }

    private void MoveBubbleFromAltPositionToCurrent()
    {
        currentBubble.transform.DOMove(muzzlePoint.position, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
        currentBubble.transform.DOScale(gridConfig.BubbleSize, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
        currentBubble.SetState(Bubble.BubbleState.GUN, animationCfg.bubbleShiftDuration);
    }

    void Update()
    {
        bool inAimingZone = Input.mousePosition.y / Screen.height > aimingRestrictedScreenFraction;
        Vector3 mousePosition = Input.mousePosition;


        if (inAimingZone == false && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit2D vHit = Physics2D.Raycast(ray.origin, ray.direction);
            bool alternativeBubbleClicked = vHit.collider != null && vHit.collider.gameObject.layer == layerBubbles;
            if (alternativeBubbleClicked)
            {
                Bubble bufferBubble = alternativeBubble;
                alternativeBubble = currentBubble;
                currentBubble = bufferBubble;

                MoveBubbleFromAltPositionToCurrent();


                alternativeBubble.transform.DOMove(AltBubblePoint, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                alternativeBubble.transform.DOScale(gridConfig.AltBubbleSize, animationCfg.bubbleShiftDuration).SetEase(animationCfg.bubbleShiftEase);
                alternativeBubble.SetState(Bubble.BubbleState.GUN_ALT, animationCfg.bubbleShiftDuration);
            }
        }


        //Process aiming of a ball
        bool pressing = Input.GetMouseButton(0);
        trajectoryLine.gameObject.SetActive(pressing && inAimingZone && currentBubble != null);
        if (pressing && currentBubble != null && inAimingZone)
        {
            trajectoryPositionsCurrent.Clear();
            trajectoryPositionsCurrent.Add(muzzlePoint.position);

            
            Vector3 mouseWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, distanceFromCamera));

            Vector2 castDirection = (mouseWorldPosition - muzzlePoint.position).normalized;

            RaycastHit2D hit = Hit(muzzlePoint.position, castDirection, true);
            if (hit.collider != null)
            {
                trajectoryPositionsCurrent.Add(hit.point);

                int hitDirectObjectLayer = hit.collider.gameObject.layer;
                if (hitDirectObjectLayer == layerWalls)
                {
                    Vector3 reflectedDirection = Vector3.Reflect(castDirection, Vector3.right);
                    RaycastHit2D bounceHit = Hit(hit.point, reflectedDirection, false);
                    if (bounceHit.collider != null)
                    {
                        int bounceHitObjectLayer = bounceHit.collider.gameObject.layer;
                        trajectoryPositionsCurrent.Add(bounceHit.point);
                        if (bounceHitObjectLayer == layerBubbles)
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
                        //Didn't hit anything after bounce
                        grid.SetBubbleOutlineActive(false);
                        trajectoryPositionsCurrent.Clear();
                        targetSlot = null;
                    }
                }
                else if (hitDirectObjectLayer == layerBubbles)
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
        else if(pressing && inAimingZone == false)
        {
            targetSlot = null;
            grid.SetBubbleOutlineActive(false);
        }
        else
        {
            //Can we not call it every frame?
            grid.SetBubbleOutlineActive(false);

            bool canShoot = targetSlot.HasValue;
            if (canShoot)
            {
                //Shooting
                Vector3 slotPosition = grid.IndecesToPosition(targetSlot.Value.x, targetSlot.Value.y);
                currentBubble.transform.DOKill(true);
                Sequence shootSequence = DOTween.Sequence();
                var moveTween = currentBubble.transform.DOMove(slotPosition, animationCfg.shootBubbleFlyDuration);
                var squishTween = currentBubble.transform.DOScale(.5f, animationCfg.shootBubbleFlyDuration).SetEase(Ease.Flash, 2, 0);
                shootSequence.Insert(0, moveTween);
                shootSequence.Insert(0, squishTween);
                Bubble flyingBubble = currentBubble;
                Vector2Int gunTargetSlot = targetSlot.Value;
                targetSlot = null;
                currentBubble = null;
                flyingBubble.SetState(Bubble.BubbleState.GRID, animationCfg.shootBubbleFlyDuration);
                shootSequence.OnComplete(() => 
                {
                    grid.AttachBubble(flyingBubble, gunTargetSlot.x, gunTargetSlot.y, true)
                        .Done(() => LoadGun());
                });

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
        LayerMask mask = 1 << layerBubbles;
        if (withWalls)
        {
            mask |= 1 << layerWalls;
        }
        return Physics2D.Raycast(start, direction, 10f, mask);
    }
}
