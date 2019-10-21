using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1f;

    private void Update()
    {
        transform.position += Vector3.forward * (speed * Time.deltaTime);
    }
}