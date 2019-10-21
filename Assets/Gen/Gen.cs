using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Gen : MonoBehaviour
{
    public Transform playerPos;
    public int chunksRows = 3;
    public int chunksOnARow = 5; 

    [NonSerialized] private float chunkSize = 4f;
    [NonSerialized] private float halfChunkSize;
    [NonSerialized] private int middleColumnIndex;
    [NonSerialized] private int lastUpdatePositionIndex;
    [NonSerialized] private TerrainChunk[,] chunks;
    [NonSerialized] private int nextRowToPull;

    private void OnValidate()
    {
        if (chunksRows < 1) chunksRows = 1;
        if (chunksOnARow % 2 == 0) chunksOnARow++;
    }

    private void Start()
    {
        halfChunkSize = chunkSize * 0.5f;
        lastUpdatePositionIndex = GetPositionIndex();
        nextRowToPull = 0;

        middleColumnIndex = (chunksOnARow - 1) / 2;
        chunks = new TerrainChunk[chunksRows, chunksOnARow];
        for (var row = 0; row < chunksRows; row++)
        {
            for (var col = 0; col < chunksOnARow; col++)
            {
                var newChunkObj = new GameObject();
                newChunkObj.transform.parent = transform;
                newChunkObj.transform.position = new Vector3(
                    (col - middleColumnIndex) * chunkSize,
                    0,row * chunkSize
                );
                
                var newChunk = newChunkObj.AddComponent<TerrainChunk>();
                newChunk.chunkSize = chunkSize;
                chunks[row, col] = newChunk;
            }
        }
        
        Debug.Log($"lastUpdatePositionIndex: {lastUpdatePositionIndex}");
    }

    private void Update()
    {
        var currPositionIndex = GetPositionIndex();
        if (currPositionIndex > lastUpdatePositionIndex)
        {
            for (var col = 0; col < chunksOnARow; col++)
            {
                var chunk = chunks[nextRowToPull, col];
                chunk.transform.position = new Vector3(
                    (col - middleColumnIndex) * chunkSize,
                    0,(currPositionIndex + 1) * chunkSize
                );
            }
            
            nextRowToPull++;
            if (nextRowToPull > chunksRows - 1)
                nextRowToPull = 0;
            
            lastUpdatePositionIndex = currPositionIndex;
            Debug.Log($"lastUpdatePositionIndex: {lastUpdatePositionIndex}"); 
        }
    }

    private int GetPositionIndex()
    {
        return Mathf.CeilToInt((playerPos.position.z - halfChunkSize) / chunkSize);
    }
}