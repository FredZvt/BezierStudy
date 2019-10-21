using System;
using UnityEngine;
using Random = System.Random;

public class TerrainChunk : MonoBehaviour
{
    public float chunkSize;

    [NonSerialized] private Color color;
    
    private void Start()
    {
        color = new Color(
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(chunkSize, 0.1f, chunkSize));
    }
}