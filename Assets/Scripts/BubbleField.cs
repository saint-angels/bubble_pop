﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleField : MonoBehaviour
{

    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private Transform fieldOrigin;

    private const float bubbleSize = 1f;
    private const float fieldWidth = 7f;
    private const float fieldHeight = 7f;

    private List<Bubble> bubbles;

    void Start()
    {
        SpawnMore();
    }

    


    void Update()
    {
        
    }

    private void SpawnMore()
    {
        for (int x = 0; x < fieldWidth; x++)
        {
            for (int y = 0; y < fieldHeight; y++)
            {
                float positionX = fieldOrigin.position.x + (bubbleSize / 2f) * x;
                float positionY = fieldOrigin.position.y + (bubbleSize / 2f) * y;

                Vector3 spawnPosition = new Vector3(positionX, positionY, 0);
                Bubble newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
                newBubble.transform.SetParent(transform);
            }
        }
    }
}
