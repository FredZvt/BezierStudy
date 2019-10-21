using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gen : MonoBehaviour
{
    public Transform playerPos;

    [NonSerialized] private float chunkSize = 4f;
    [NonSerialized] private float halfChunkSize;
    [NonSerialized] private int lastUpdatePositionIndex;

    private void Start()
    {
        halfChunkSize = chunkSize * 0.5f;
        lastUpdatePositionIndex = GetPositionIndex();
    }

    private void Update()
    {
        var currPositionIndex = GetPositionIndex();

        if (currPositionIndex > lastUpdatePositionIndex)
        {
            Debug.Log("UP");
        }
        else if (currPositionIndex < lastUpdatePositionIndex)
        {
            Debug.Log("DOWN");
        }

        lastUpdatePositionIndex = currPositionIndex;
    }

    private int GetPositionIndex()
    {
        return Mathf.CeilToInt((playerPos.position.z - halfChunkSize) / chunkSize);
    }
}
