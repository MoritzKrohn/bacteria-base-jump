using UnityEngine;
using System.Collections;

public class Spinning : MonoBehaviour
{
    public float Speed = 2f;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, Speed);
    }
}
